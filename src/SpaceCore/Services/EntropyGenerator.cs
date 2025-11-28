using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using SpaceCore.Interfaces;
using SpaceCore.Models;

namespace SpaceCore.Services;

/// <summary>
/// Generates cryptographically secure random noise.
/// Supports both streaming and single-shot generation with optional seeding for reproducibility.
/// </summary>
public class EntropyGenerator : IEntropyGenerator
{
    private long _sequence;
    private readonly object _sequenceLock = new();

    /// <inheritdoc />
    public async IAsyncEnumerable<NoiseChunk> StreamNoise(
        Guid sessionId,
        EntropyOptions options,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        while (!ct.IsCancellationRequested)
        {
            yield return GenerateChunk(sessionId, options);
            await System.Threading.Tasks.Task.Delay(1, ct); // Yield control
        }
    }

    /// <inheritdoc />
    public NoiseChunk GenerateChunk(Guid sessionId, EntropyOptions options)
    {
        var timestamp = DateTimeOffset.UtcNow;
        var seed = options.UseSeed && options.Seed != null
            ? options.Seed
            : GenerateRandomSeed();

        byte[] bytes;
        if (options.UseSeed && options.Seed != null)
        {
            // Use deterministic generation with seed
            bytes = GenerateDeterministicBytes(seed, options.ChunkSize, GetNextSequence());
        }
        else
        {
            // Use CSPRNG for true randomness
            bytes = RandomNumberGenerator.GetBytes(options.ChunkSize);
        }

        var noiseHash = NoiseChunk.ComputeHash(seed, bytes, timestamp);

        return new NoiseChunk(
            Id: Guid.NewGuid(),
            Bytes: bytes,
            Sequence: GetNextSequence(),
            Timestamp: timestamp,
            NoiseHash: noiseHash,
            Seed: seed
        );
    }

    /// <inheritdoc />
    public NoiseChunk[] GenerateChunks(Guid sessionId, int count, EntropyOptions options)
    {
        var chunks = new NoiseChunk[count];
        for (int i = 0; i < count; i++)
        {
            chunks[i] = GenerateChunk(sessionId, options);
        }
        return chunks;
    }

    /// <inheritdoc />
    public double EstimateEntropyQuality()
    {
        // Generate test samples and measure entropy
        const int sampleSize = 1000;
        const int bytesPerSample = 64;

        var counts = new int[256];
        for (int i = 0; i < sampleSize; i++)
        {
            var bytes = RandomNumberGenerator.GetBytes(bytesPerSample);
            foreach (var b in bytes)
            {
                counts[b]++;
            }
        }

        // Calculate Shannon entropy
        double totalBytes = sampleSize * bytesPerSample;
        double entropy = 0;
        foreach (var count in counts)
        {
            if (count > 0)
            {
                var p = count / totalBytes;
                entropy -= p * Math.Log2(p);
            }
        }

        // Normalize to 0-1 range (max entropy for 256 symbols is 8 bits)
        return entropy / 8.0;
    }

    private long GetNextSequence()
    {
        lock (_sequenceLock)
        {
            return ++_sequence;
        }
    }

    private static byte[] GenerateRandomSeed()
    {
        return RandomNumberGenerator.GetBytes(32); // 256-bit seed
    }

    private static byte[] GenerateDeterministicBytes(byte[] seed, int size, long sequence)
    {
        // Use HKDF-like expansion from seed + sequence
        using var sha256 = SHA256.Create();
        var result = new byte[size];
        var sequenceBytes = BitConverter.GetBytes(sequence);

        int offset = 0;
        int counter = 0;

        while (offset < size)
        {
            var counterBytes = BitConverter.GetBytes(counter++);
            var input = new byte[seed.Length + sequenceBytes.Length + counterBytes.Length];
            Buffer.BlockCopy(seed, 0, input, 0, seed.Length);
            Buffer.BlockCopy(sequenceBytes, 0, input, seed.Length, sequenceBytes.Length);
            Buffer.BlockCopy(counterBytes, 0, input, seed.Length + sequenceBytes.Length, counterBytes.Length);

            var hash = sha256.ComputeHash(input);
            var bytesToCopy = Math.Min(hash.Length, size - offset);
            Buffer.BlockCopy(hash, 0, result, offset, bytesToCopy);
            offset += bytesToCopy;
        }

        return result;
    }
}
