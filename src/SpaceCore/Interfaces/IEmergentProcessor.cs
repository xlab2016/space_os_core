using System.Threading;
using System.Threading.Tasks;
using SpaceCore.Models;

namespace SpaceCore.Interfaces;

/// <summary>
/// Processes the graph of connected points to generate emergent shapes.
/// Implements graph dynamics, activation propagation, and pattern detection.
/// </summary>
public interface IEmergentProcessor
{
    /// <summary>
    /// Processes points and edges to generate emergent shapes.
    /// </summary>
    /// <param name="points">Points in the graph.</param>
    /// <param name="edges">Edges connecting points.</param>
    /// <param name="options">Processing options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Array of emergent shapes.</returns>
    Task<EmergentShape[]> ProcessAsync(ProjectedPoint[] points, EmergentEdge[] edges, ProcessorOptions options, CancellationToken ct = default);

    /// <summary>
    /// Runs one step of activation propagation on the graph.
    /// </summary>
    /// <param name="points">Points with their current activation levels.</param>
    /// <param name="edges">Edges for propagation.</param>
    /// <param name="activations">Current activation levels (point ID -> activation).</param>
    /// <param name="options">Processing options.</param>
    /// <returns>Updated activation levels.</returns>
    System.Collections.Generic.Dictionary<System.Guid, double> PropagateStep(
        ProjectedPoint[] points,
        EmergentEdge[] edges,
        System.Collections.Generic.Dictionary<System.Guid, double> activations,
        ProcessorOptions options);

    /// <summary>
    /// Detects connected components in the graph.
    /// </summary>
    /// <param name="points">Points in the graph.</param>
    /// <param name="edges">Edges connecting points.</param>
    /// <returns>Array of shapes (connected components).</returns>
    EmergentShape[] DetectComponents(ProjectedPoint[] points, EmergentEdge[] edges);
}
