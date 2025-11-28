using System;
using System.Threading;
using System.Threading.Tasks;
using SpaceCore.Models;

namespace SpaceCore.Interfaces;

/// <summary>
/// Orchestrates the entire consciousness flow pipeline.
/// Coordinates all components from entropy generation to action execution.
/// </summary>
public interface IConsciousnessAxisOrchestrator
{
    /// <summary>
    /// Processes a percept through the entire pipeline.
    /// </summary>
    /// <param name="percept">Input percept.</param>
    /// <param name="context">Agent context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Processing result.</returns>
    Task<ConsciousnessProcessingResult> ProcessPerceptAsync(Percept percept, AgentContext context, CancellationToken ct = default);

    /// <summary>
    /// Starts a continuous consciousness flow for a session.
    /// </summary>
    /// <param name="sessionId">Session identifier.</param>
    /// <param name="context">Agent context.</param>
    /// <param name="ct">Cancellation token.</param>
    Task StartFlowAsync(Guid sessionId, AgentContext context, CancellationToken ct = default);

    /// <summary>
    /// Stops the consciousness flow for a session.
    /// </summary>
    /// <param name="sessionId">Session identifier.</param>
    Task StopFlowAsync(Guid sessionId);

    /// <summary>
    /// Gets the current state of the I-point for a session.
    /// </summary>
    /// <param name="sessionId">Session identifier.</param>
    /// <returns>Current I-point state.</returns>
    IPointState? GetCurrentIPointState(Guid sessionId);

    /// <summary>
    /// Gets the current context state for a session.
    /// </summary>
    /// <param name="sessionId">Session identifier.</param>
    /// <returns>Current context state.</returns>
    Task<ContextState?> GetContextStateAsync(Guid sessionId, CancellationToken ct = default);
}

/// <summary>
/// Result of consciousness processing.
/// </summary>
/// <param name="SessionId">Session identifier.</param>
/// <param name="States">Generated subjective states.</param>
/// <param name="Thoughts">Generated thoughts.</param>
/// <param name="Solutions">Proposed solutions.</param>
/// <param name="IPointState">Updated I-point state.</param>
/// <param name="Trace">Full processing trace.</param>
public record ConsciousnessProcessingResult(
    Guid SessionId,
    SubjectiveState[] States,
    Thought[] Thoughts,
    Solution[] Solutions,
    IPointState IPointState,
    ConsciousnessTrace Trace);

/// <summary>
/// Trace of the consciousness processing pipeline.
/// </summary>
/// <param name="PerceptId">Input percept identifier.</param>
/// <param name="NoiseHashes">Hashes of noise chunks used.</param>
/// <param name="ClusterCount">Number of clusters generated.</param>
/// <param name="PointCount">Number of points projected.</param>
/// <param name="EdgeCount">Number of edges created.</param>
/// <param name="ShapeCount">Number of shapes formed.</param>
/// <param name="DeterministicKey">Key for reproducing this processing.</param>
/// <param name="ProcessingTimeMs">Total processing time in milliseconds.</param>
public record ConsciousnessTrace(
    Guid PerceptId,
    string[] NoiseHashes,
    int ClusterCount,
    int PointCount,
    int EdgeCount,
    int ShapeCount,
    string DeterministicKey,
    long ProcessingTimeMs);
