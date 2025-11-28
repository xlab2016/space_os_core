using System.Threading;
using System.Threading.Tasks;
using SpaceCore.Models;

namespace SpaceCore.Interfaces;

/// <summary>
/// Projects clusters into N-dimensional semantic space.
/// Uses deterministic random projection or learned mappings.
/// </summary>
public interface IMultiDimensionProjector
{
    /// <summary>
    /// Projects a cluster into N-dimensional space.
    /// </summary>
    /// <param name="cluster">The cluster to project.</param>
    /// <param name="dimension">Target dimensionality.</param>
    /// <param name="options">Projection options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Array of projected points.</returns>
    Task<ProjectedPoint[]> ProjectAsync(Cluster cluster, int dimension, ProjectionOptions options, CancellationToken ct = default);

    /// <summary>
    /// Projects multiple clusters into N-dimensional space.
    /// </summary>
    /// <param name="clusters">The clusters to project.</param>
    /// <param name="dimension">Target dimensionality.</param>
    /// <param name="options">Projection options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Array of projected points.</returns>
    Task<ProjectedPoint[]> ProjectBatchAsync(Cluster[] clusters, int dimension, ProjectionOptions options, CancellationToken ct = default);

    /// <summary>
    /// Gets the version identifier for this projector (for tracing).
    /// </summary>
    string Version { get; }
}
