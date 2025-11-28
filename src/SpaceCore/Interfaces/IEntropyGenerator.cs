using System;
using System.Collections.Generic;
using System.Threading;
using SpaceCore.Models;

namespace SpaceCore.Interfaces;

/// <summary>
/// Generates random noise from a cryptographically secure source.
/// Provides both streaming and single-shot generation modes.
/// </summary>
public interface IEntropyGenerator
{
    /// <summary>
    /// Streams noise chunks continuously.
    /// </summary>
    /// <param name="sessionId">Session identifier for tracing.</param>
    /// <param name="options">Generation options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Async enumerable of noise chunks.</returns>
    IAsyncEnumerable<NoiseChunk> StreamNoise(Guid sessionId, EntropyOptions options, CancellationToken ct = default);

    /// <summary>
    /// Generates a single noise chunk.
    /// </summary>
    /// <param name="sessionId">Session identifier for tracing.</param>
    /// <param name="options">Generation options.</param>
    /// <returns>A single noise chunk.</returns>
    NoiseChunk GenerateChunk(Guid sessionId, EntropyOptions options);

    /// <summary>
    /// Generates multiple noise chunks at once.
    /// </summary>
    /// <param name="sessionId">Session identifier for tracing.</param>
    /// <param name="count">Number of chunks to generate.</param>
    /// <param name="options">Generation options.</param>
    /// <returns>Array of noise chunks.</returns>
    NoiseChunk[] GenerateChunks(Guid sessionId, int count, EntropyOptions options);

    /// <summary>
    /// Estimates the entropy quality of the generator.
    /// </summary>
    /// <returns>Entropy quality score (0.0 to 1.0).</returns>
    double EstimateEntropyQuality();
}
