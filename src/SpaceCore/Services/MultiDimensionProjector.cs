using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using SpaceCore.Interfaces;
using SpaceCore.Models;

namespace SpaceCore.Services;

/// <summary>
/// Projects clusters into N-dimensional semantic space using random projection.
/// Implements deterministic projection seeded by cluster hash for reproducibility.
/// </summary>
public class MultiDimensionProjector : IMultiDimensionProjector
{
    private const string ProjectorVersionValue = "1.0.0";

    /// <inheritdoc />
    public string Version => ProjectorVersionValue;

    /// <inheritdoc />
    public Task<ProjectedPoint[]> ProjectAsync(
        Cluster cluster,
        int dimension,
        ProjectionOptions options,
        CancellationToken ct = default)
    {
        var points = new ProjectedPoint[options.Candidates];

        for (int i = 0; i < options.Candidates; i++)
        {
            var point = ProjectSingle(cluster, dimension, options, i);
            points[i] = point;
        }

        return Task.FromResult(points);
    }

    /// <inheritdoc />
    public Task<ProjectedPoint[]> ProjectBatchAsync(
        Cluster[] clusters,
        int dimension,
        ProjectionOptions options,
        CancellationToken ct = default)
    {
        var allPoints = clusters
            .SelectMany(c => ProjectAsync(c, dimension, options, ct).Result)
            .ToArray();

        return Task.FromResult(allPoints);
    }

    private ProjectedPoint ProjectSingle(Cluster cluster, int dimension, ProjectionOptions options, int candidateIndex)
    {
        // Generate deterministic random projection matrix seeded by cluster hash + candidate index
        var projectionMatrix = GenerateProjectionMatrix(
            cluster.ClusterHash,
            candidateIndex,
            cluster.Centroid.Length,
            dimension,
            options.Determinism);

        // Project the centroid
        var vector = new float[dimension];
        for (int i = 0; i < dimension; i++)
        {
            float sum = 0;
            for (int j = 0; j < cluster.Centroid.Length; j++)
            {
                sum += cluster.Centroid[j] * projectionMatrix[j, i];
            }
            vector[i] = sum;
        }

        // Apply creativity scaling (higher creativity = more variance)
        if (options.Creativity > 0)
        {
            var creativityNoise = GenerateCreativityNoise(
                cluster.ClusterHash,
                candidateIndex,
                dimension,
                options.Creativity);

            for (int i = 0; i < dimension; i++)
            {
                vector[i] += creativityNoise[i];
            }
        }

        // Normalize the vector
        Normalize(vector);

        // Calculate confidence based on determinism and cluster quality
        var confidence = options.Determinism * (1 - cluster.EntropyEstimate);

        // Generate semantic tag based on vector characteristics
        var semanticTag = GenerateSemanticTag(vector, options);

        // Create trace
        var trace = ProjectedPoint.CreateTrace(
            noiseHash: cluster.ClusterHash,
            clusterHash: cluster.ClusterHash,
            projectorVersion: ProjectorVersionValue,
            seed: null);

        return new ProjectedPoint(
            Id: Guid.NewGuid(),
            ClusterId: cluster.Id,
            Dimensionality: dimension,
            Vector: vector,
            Confidence: confidence,
            SemanticTag: semanticTag,
            Trace: trace
        );
    }

    private float[,] GenerateProjectionMatrix(string seed, int candidateIndex, int inputDim, int outputDim, double determinism)
    {
        // Use seed for deterministic random matrix generation
        var matrix = new float[inputDim, outputDim];
        var seedBytes = System.Text.Encoding.UTF8.GetBytes(seed + candidateIndex);

        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(seedBytes);

        // Create seeded random from hash
        var seedInt = BitConverter.ToInt32(hash, 0);
        var random = new Random(seedInt);

        // Gaussian random projection (Johnson-Lindenstrauss)
        for (int i = 0; i < inputDim; i++)
        {
            for (int j = 0; j < outputDim; j++)
            {
                // Box-Muller transform for Gaussian
                double u1 = 1.0 - random.NextDouble();
                double u2 = 1.0 - random.NextDouble();
                double gaussian = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

                matrix[i, j] = (float)(gaussian / Math.Sqrt(outputDim));
            }
        }

        return matrix;
    }

    private float[] GenerateCreativityNoise(string seed, int candidateIndex, int dimension, double creativity)
    {
        var noise = new float[dimension];
        var seedBytes = System.Text.Encoding.UTF8.GetBytes($"creativity_{seed}_{candidateIndex}");

        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(seedBytes);
        var random = new Random(BitConverter.ToInt32(hash, 0));

        for (int i = 0; i < dimension; i++)
        {
            // Scaled noise based on creativity level
            noise[i] = (float)((random.NextDouble() - 0.5) * creativity * 0.5);
        }

        return noise;
    }

    private void Normalize(float[] vector)
    {
        double norm = Math.Sqrt(vector.Sum(v => v * v));
        if (norm > 0)
        {
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] /= (float)norm;
            }
        }
    }

    private string GenerateSemanticTag(float[] vector, ProjectionOptions options)
    {
        // Simple semantic tagging based on vector characteristics
        var avgValue = vector.Average();
        var variance = vector.Average(v => (v - avgValue) * (v - avgValue));

        if (variance > 0.1)
        {
            return avgValue > 0 ? "high-energy-positive" : "high-energy-negative";
        }
        else if (variance > 0.05)
        {
            return avgValue > 0 ? "moderate-positive" : "moderate-negative";
        }
        else
        {
            return "neutral-stable";
        }
    }
}
