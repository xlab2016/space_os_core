using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SpaceCore.Interfaces;
using SpaceCore.Models;

namespace SpaceCore.Services;

/// <summary>
/// Groups noise chunks into clusters using online k-means clustering.
/// Implements streaming clustering for continuous noise processing.
/// </summary>
public class CrossDimensionProjector : ICrossDimensionProjector
{
    private readonly Random _random = new();

    /// <inheritdoc />
    public Task<Cluster[]> ClusterAsync(NoiseChunk[] chunks, ClusterOptions options, CancellationToken ct = default)
    {
        if (chunks.Length == 0)
            return Task.FromResult(Array.Empty<Cluster>());

        // Convert noise chunks to feature vectors
        var features = chunks.Select(ExtractFeatures).ToArray();

        // Perform k-means clustering
        var centroids = InitializeCentroids(features, options.K);
        var assignments = new int[features.Length];

        // Run k-means iterations
        const int maxIterations = 100;
        for (int iter = 0; iter < maxIterations; iter++)
        {
            // Assign points to nearest centroid
            bool changed = false;
            for (int i = 0; i < features.Length; i++)
            {
                int nearest = FindNearestCentroid(features[i], centroids);
                if (assignments[i] != nearest)
                {
                    assignments[i] = nearest;
                    changed = true;
                }
            }

            if (!changed) break;

            // Update centroids
            UpdateCentroids(features, assignments, centroids);
        }

        // Build clusters
        var clusters = new List<Cluster>();
        for (int k = 0; k < options.K; k++)
        {
            var clusterPoints = features
                .Select((f, i) => (f, i))
                .Where(x => assignments[x.i] == k)
                .Select(x => x.f)
                .ToArray();

            if (clusterPoints.Length == 0) continue;

            var centroid = centroids[k];
            var createdAt = DateTimeOffset.UtcNow;
            var clusterHash = Cluster.ComputeHash(centroid, options.Algorithm, createdAt);
            var entropyEstimate = EstimateClusterEntropy(clusterPoints);

            clusters.Add(new Cluster(
                Id: Guid.NewGuid(),
                Algorithm: options.Algorithm,
                Size: clusterPoints.Length,
                Centroid: centroid,
                EntropyEstimate: entropyEstimate,
                ClusterHash: clusterHash,
                CreatedAt: createdAt
            ));
        }

        return Task.FromResult(clusters.ToArray());
    }

    /// <inheritdoc />
    public Task<Cluster[]> UpdateClustersAsync(
        NoiseChunk chunk,
        Cluster[] existingClusters,
        ClusterOptions options,
        CancellationToken ct = default)
    {
        if (existingClusters.Length == 0)
        {
            return ClusterAsync(new[] { chunk }, options, ct);
        }

        var features = ExtractFeatures(chunk);
        var nearestIdx = FindNearestCentroid(features, existingClusters.Select(c => c.Centroid).ToArray());

        // Update the nearest cluster using online k-means update
        var nearest = existingClusters[nearestIdx];
        var learningRate = 1.0f / (nearest.Size + 1);

        var newCentroid = new float[nearest.Centroid.Length];
        for (int i = 0; i < newCentroid.Length; i++)
        {
            newCentroid[i] = nearest.Centroid[i] + learningRate * (features[i] - nearest.Centroid[i]);
        }

        var createdAt = DateTimeOffset.UtcNow;
        var updatedCluster = nearest with
        {
            Size = nearest.Size + 1,
            Centroid = newCentroid,
            ClusterHash = Cluster.ComputeHash(newCentroid, options.Algorithm, createdAt),
            CreatedAt = createdAt
        };

        var result = existingClusters.ToArray();
        result[nearestIdx] = updatedCluster;

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public int AssignToCluster(NoiseChunk chunk, Cluster[] clusters)
    {
        if (clusters.Length == 0)
            throw new ArgumentException("No clusters available");

        var features = ExtractFeatures(chunk);
        return FindNearestCentroid(features, clusters.Select(c => c.Centroid).ToArray());
    }

    private float[] ExtractFeatures(NoiseChunk chunk)
    {
        // Extract statistical features from the noise bytes
        // Using 64-dimensional feature vector
        const int featureDim = 64;
        var features = new float[featureDim];

        // Basic statistics
        if (chunk.Bytes.Length > 0)
        {
            features[0] = (float)(chunk.Bytes.Average(b => b) / 255.0);
            features[1] = (float)StandardDeviation(chunk.Bytes) / 255f;
            features[2] = chunk.Bytes.Min() / 255f;
            features[3] = chunk.Bytes.Max() / 255f;

            // Byte frequency histogram (normalized to fit remaining features)
            var histogram = new int[256];
            foreach (var b in chunk.Bytes)
                histogram[b]++;

            // Sample histogram into remaining features
            int binSize = 256 / (featureDim - 4);
            for (int i = 4; i < featureDim; i++)
            {
                int startBin = (i - 4) * binSize;
                int endBin = Math.Min(startBin + binSize, 256);
                float sum = 0;
                for (int j = startBin; j < endBin; j++)
                    sum += histogram[j];
                features[i] = sum / chunk.Bytes.Length;
            }
        }

        return features;
    }

    private float[][] InitializeCentroids(float[][] data, int k)
    {
        // K-means++ initialization
        var centroids = new float[k][];
        var used = new HashSet<int>();

        // First centroid is random
        var firstIdx = _random.Next(data.Length);
        centroids[0] = (float[])data[firstIdx].Clone();
        used.Add(firstIdx);

        // Remaining centroids using k-means++ selection
        for (int c = 1; c < k; c++)
        {
            var distances = new double[data.Length];
            double totalDist = 0;

            for (int i = 0; i < data.Length; i++)
            {
                if (used.Contains(i))
                {
                    distances[i] = 0;
                    continue;
                }

                double minDist = double.MaxValue;
                for (int j = 0; j < c; j++)
                {
                    var dist = EuclideanDistance(data[i], centroids[j]);
                    minDist = Math.Min(minDist, dist);
                }
                distances[i] = minDist * minDist;
                totalDist += distances[i];
            }

            // Weighted random selection
            double threshold = _random.NextDouble() * totalDist;
            double cumulative = 0;
            int selectedIdx = 0;
            for (int i = 0; i < data.Length; i++)
            {
                cumulative += distances[i];
                if (cumulative >= threshold)
                {
                    selectedIdx = i;
                    break;
                }
            }

            centroids[c] = (float[])data[selectedIdx].Clone();
            used.Add(selectedIdx);
        }

        return centroids;
    }

    private int FindNearestCentroid(float[] point, float[][] centroids)
    {
        int nearest = 0;
        double minDist = double.MaxValue;

        for (int i = 0; i < centroids.Length; i++)
        {
            var dist = EuclideanDistance(point, centroids[i]);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = i;
            }
        }

        return nearest;
    }

    private void UpdateCentroids(float[][] data, int[] assignments, float[][] centroids)
    {
        var counts = new int[centroids.Length];
        var sums = centroids.Select(c => new float[c.Length]).ToArray();

        for (int i = 0; i < data.Length; i++)
        {
            int cluster = assignments[i];
            counts[cluster]++;
            for (int j = 0; j < data[i].Length; j++)
                sums[cluster][j] += data[i][j];
        }

        for (int k = 0; k < centroids.Length; k++)
        {
            if (counts[k] > 0)
            {
                for (int j = 0; j < centroids[k].Length; j++)
                    centroids[k][j] = sums[k][j] / counts[k];
            }
        }
    }

    private double EuclideanDistance(float[] a, float[] b)
    {
        double sum = 0;
        for (int i = 0; i < a.Length; i++)
        {
            var diff = a[i] - b[i];
            sum += diff * diff;
        }
        return Math.Sqrt(sum);
    }

    private double StandardDeviation(byte[] data)
    {
        var mean = data.Average(b => (double)b);
        var variance = data.Average(b => (b - mean) * (b - mean));
        return Math.Sqrt(variance);
    }

    private double EstimateClusterEntropy(float[][] points)
    {
        if (points.Length == 0) return 0;

        // Estimate entropy based on point spread
        var center = new float[points[0].Length];
        foreach (var point in points)
            for (int i = 0; i < point.Length; i++)
                center[i] += point[i] / points.Length;

        double totalVariance = 0;
        foreach (var point in points)
        {
            for (int i = 0; i < point.Length; i++)
            {
                var diff = point[i] - center[i];
                totalVariance += diff * diff;
            }
        }

        return Math.Min(1.0, totalVariance / (points.Length * points[0].Length));
    }
}
