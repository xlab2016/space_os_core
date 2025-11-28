using System;
using System.Security.Cryptography;

namespace SpaceCore.Models;

/// <summary>
/// Represents a chunk of random noise from the entropy generator.
/// </summary>
/// <param name="Id">Unique identifier for this noise chunk.</param>
/// <param name="Bytes">Raw random bytes.</param>
/// <param name="Sequence">Sequence number in the stream.</param>
/// <param name="Timestamp">When this chunk was generated.</param>
/// <param name="NoiseHash">SHA256 hash of seed || bytes || timestamp for reproducibility.</param>
/// <param name="Seed">The seed used to generate this chunk (for deterministic replay).</param>
public record NoiseChunk(
    Guid Id,
    byte[] Bytes,
    long Sequence,
    DateTimeOffset Timestamp,
    string NoiseHash,
    byte[]? Seed = null)
{
    /// <summary>
    /// Computes the hash for this noise chunk.
    /// </summary>
    public static string ComputeHash(byte[] seed, byte[] bytes, DateTimeOffset timestamp)
    {
        using var sha256 = SHA256.Create();
        var combined = new byte[seed.Length + bytes.Length + sizeof(long)];
        Buffer.BlockCopy(seed, 0, combined, 0, seed.Length);
        Buffer.BlockCopy(bytes, 0, combined, seed.Length, bytes.Length);
        var timestampBytes = BitConverter.GetBytes(timestamp.Ticks);
        Buffer.BlockCopy(timestampBytes, 0, combined, seed.Length + bytes.Length, sizeof(long));
        return Convert.ToHexString(sha256.ComputeHash(combined));
    }

    /// <summary>
    /// Estimates the entropy in bits for this chunk (assumes uniform distribution).
    /// </summary>
    public double EntropyBits => Bytes.Length * 8;
}
