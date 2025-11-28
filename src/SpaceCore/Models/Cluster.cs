using System;
using System.Security.Cryptography;

namespace SpaceCore.Models;

/// <summary>
/// Represents a cluster of noise chunks grouped by similarity.
/// </summary>
/// <param name="Id">Unique identifier for this cluster.</param>
/// <param name="Algorithm">The clustering algorithm used (e.g., "online-kmeans", "dbscan").</param>
/// <param name="Size">Number of noise chunks in this cluster.</param>
/// <param name="Centroid">The centroid vector of this cluster in N-dimensional space.</param>
/// <param name="EntropyEstimate">Estimated entropy level of this cluster.</param>
/// <param name="ClusterHash">Hash for reproducibility and tracing.</param>
/// <param name="CreatedAt">When this cluster was created.</param>
public record Cluster(
    Guid Id,
    string Algorithm,
    int Size,
    float[] Centroid,
    double EntropyEstimate,
    string ClusterHash,
    DateTimeOffset CreatedAt)
{
    /// <summary>
    /// Computes the hash for this cluster.
    /// </summary>
    public static string ComputeHash(float[] centroid, string algorithm, DateTimeOffset createdAt)
    {
        using var sha256 = SHA256.Create();
        var centroidBytes = new byte[centroid.Length * sizeof(float)];
        Buffer.BlockCopy(centroid, 0, centroidBytes, 0, centroidBytes.Length);
        var algorithmBytes = System.Text.Encoding.UTF8.GetBytes(algorithm);
        var timestampBytes = BitConverter.GetBytes(createdAt.Ticks);

        var combined = new byte[centroidBytes.Length + algorithmBytes.Length + timestampBytes.Length];
        Buffer.BlockCopy(centroidBytes, 0, combined, 0, centroidBytes.Length);
        Buffer.BlockCopy(algorithmBytes, 0, combined, centroidBytes.Length, algorithmBytes.Length);
        Buffer.BlockCopy(timestampBytes, 0, combined, centroidBytes.Length + algorithmBytes.Length, timestampBytes.Length);

        return Convert.ToHexString(sha256.ComputeHash(combined));
    }

    /// <summary>
    /// Gets the dimensionality of the cluster centroid.
    /// </summary>
    public int Dimensionality => Centroid.Length;
}
