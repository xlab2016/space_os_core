using System;
using System.Security.Cryptography;
using System.Text.Json;

namespace SpaceCore.Models;

/// <summary>
/// Represents a point projected into N-dimensional semantic space.
/// </summary>
/// <param name="Id">Unique identifier for this point.</param>
/// <param name="ClusterId">The cluster this point was derived from.</param>
/// <param name="Dimensionality">The dimensionality of the vector space.</param>
/// <param name="Vector">The point coordinates in N-dimensional space.</param>
/// <param name="Confidence">Confidence score for this projection (0.0 to 1.0).</param>
/// <param name="SemanticTag">A semantic tag describing this point.</param>
/// <param name="Trace">JSON trace containing noiseHash, clusterHash, projectorVersion, seed.</param>
public record ProjectedPoint(
    Guid Id,
    Guid ClusterId,
    int Dimensionality,
    float[] Vector,
    double Confidence,
    string SemanticTag,
    string Trace)
{
    /// <summary>
    /// Creates a trace JSON string for this projected point.
    /// </summary>
    public static string CreateTrace(string noiseHash, string clusterHash, string projectorVersion, byte[]? seed)
    {
        var trace = new
        {
            noiseHash,
            clusterHash,
            projectorVersion,
            seed = seed != null ? Convert.ToHexString(seed) : null,
            timestamp = DateTimeOffset.UtcNow
        };
        return JsonSerializer.Serialize(trace);
    }

    /// <summary>
    /// Computes the Euclidean distance to another point.
    /// </summary>
    public double DistanceTo(ProjectedPoint other)
    {
        if (Vector.Length != other.Vector.Length)
            throw new ArgumentException("Vectors must have the same dimensionality");

        double sum = 0;
        for (int i = 0; i < Vector.Length; i++)
        {
            var diff = Vector[i] - other.Vector[i];
            sum += diff * diff;
        }
        return Math.Sqrt(sum);
    }

    /// <summary>
    /// Computes the cosine similarity to another point.
    /// </summary>
    public double CosineSimilarity(ProjectedPoint other)
    {
        if (Vector.Length != other.Vector.Length)
            throw new ArgumentException("Vectors must have the same dimensionality");

        double dotProduct = 0;
        double normA = 0;
        double normB = 0;

        for (int i = 0; i < Vector.Length; i++)
        {
            dotProduct += Vector[i] * other.Vector[i];
            normA += Vector[i] * Vector[i];
            normB += other.Vector[i] * other.Vector[i];
        }

        if (normA == 0 || normB == 0) return 0;
        return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }
}
