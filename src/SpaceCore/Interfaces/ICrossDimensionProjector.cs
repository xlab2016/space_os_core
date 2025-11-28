using System.Threading;
using System.Threading.Tasks;
using SpaceCore.Models;

namespace SpaceCore.Interfaces;

/// <summary>
/// Groups noise chunks into clusters using online clustering algorithms.
/// This is the first stage of transforming raw entropy into structured data.
/// </summary>
public interface ICrossDimensionProjector
{
    /// <summary>
    /// Clusters noise chunks into groups based on similarity/patterns.
    /// </summary>
    /// <param name="chunks">The noise chunks to cluster.</param>
    /// <param name="options">Clustering options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Array of clusters.</returns>
    Task<Cluster[]> ClusterAsync(NoiseChunk[] chunks, ClusterOptions options, CancellationToken ct = default);

    /// <summary>
    /// Updates existing clusters with new noise chunks (online learning).
    /// </summary>
    /// <param name="chunk">New noise chunk.</param>
    /// <param name="existingClusters">Existing clusters to update.</param>
    /// <param name="options">Clustering options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Updated array of clusters.</returns>
    Task<Cluster[]> UpdateClustersAsync(NoiseChunk chunk, Cluster[] existingClusters, ClusterOptions options, CancellationToken ct = default);

    /// <summary>
    /// Assigns a noise chunk to the nearest existing cluster.
    /// </summary>
    /// <param name="chunk">Noise chunk to assign.</param>
    /// <param name="clusters">Available clusters.</param>
    /// <returns>The index of the nearest cluster.</returns>
    int AssignToCluster(NoiseChunk chunk, Cluster[] clusters);
}
