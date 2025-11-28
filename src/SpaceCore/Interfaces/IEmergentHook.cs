using System.Threading;
using System.Threading.Tasks;
using SpaceCore.Models;

namespace SpaceCore.Interfaces;

/// <summary>
/// Builds connections/edges between projected points.
/// Creates temporal, semantic, and causal links.
/// </summary>
public interface IEmergentHook
{
    /// <summary>
    /// Creates edges between projected points.
    /// </summary>
    /// <param name="points">Points to link.</param>
    /// <param name="options">Linking options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Array of edges.</returns>
    Task<EmergentEdge[]> LinkAsync(ProjectedPoint[] points, LinkOptions options, CancellationToken ct = default);

    /// <summary>
    /// Creates edges from a new point to existing points.
    /// </summary>
    /// <param name="newPoint">The new point.</param>
    /// <param name="existingPoints">Existing points to potentially link to.</param>
    /// <param name="options">Linking options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Array of new edges.</returns>
    Task<EmergentEdge[]> LinkNewPointAsync(ProjectedPoint newPoint, ProjectedPoint[] existingPoints, LinkOptions options, CancellationToken ct = default);

    /// <summary>
    /// Prunes weak edges from the graph.
    /// </summary>
    /// <param name="edges">Edges to prune.</param>
    /// <param name="minWeight">Minimum weight to keep.</param>
    /// <returns>Pruned array of edges.</returns>
    EmergentEdge[] PruneEdges(EmergentEdge[] edges, double minWeight = 0.3);
}
