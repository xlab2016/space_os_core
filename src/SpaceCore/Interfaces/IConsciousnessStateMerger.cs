using System.Threading;
using System.Threading.Tasks;
using SpaceCore.Models;

namespace SpaceCore.Interfaces;

/// <summary>
/// Merges new pipeline results into the existing consciousness state
/// based on the I-point position (entropy vs determinism influence).
/// </summary>
public interface IConsciousnessStateMerger
{
    /// <summary>
    /// Initializes consciousness state from entropy when the state is empty.
    /// Called when a session first starts and no persisted state exists.
    /// </summary>
    /// <param name="state">The empty consciousness state to initialize.</param>
    /// <param name="clusters">Initial clusters from entropy.</param>
    /// <param name="highDimPoints">Initial high-dimensional points.</param>
    /// <param name="lowDimPoints">Initial low-dimensional points.</param>
    /// <param name="edges">Initial graph edges.</param>
    /// <param name="shapes">Initial emergent shapes.</param>
    /// <param name="subjectiveStates">Initial subjective states.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Initialized consciousness state.</returns>
    Task<ConsciousnessState> InitializeFromEntropyAsync(
        ConsciousnessState state,
        Cluster[] clusters,
        ProjectedPoint[] highDimPoints,
        ProjectedPoint[] lowDimPoints,
        EmergentEdge[] edges,
        EmergentShape[] shapes,
        SubjectiveState[] subjectiveStates,
        CancellationToken ct = default);

    /// <summary>
    /// Merges new pipeline results into existing consciousness state.
    /// The impact strength is determined by the I-point position:
    /// - Right (entropic): stronger impact from entropy on existing state
    /// - Left (deterministic): weaker impact but stronger influence on Thought
    ///
    /// Merge operations:
    /// - Adjust weights (+/-) of existing items
    /// - Add new points/edges with low initial weight
    /// - Strengthen or weaken existing connections
    /// </summary>
    /// <param name="existingState">The current consciousness state.</param>
    /// <param name="iPointPosition">Current I-point axis position (-1 to +1).</param>
    /// <param name="clusters">New clusters from this processing cycle.</param>
    /// <param name="highDimPoints">New high-dimensional points.</param>
    /// <param name="lowDimPoints">New low-dimensional points.</param>
    /// <param name="edges">New graph edges.</param>
    /// <param name="shapes">New emergent shapes.</param>
    /// <param name="subjectiveStates">New subjective states.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Updated consciousness state with merged data.</returns>
    Task<ConsciousnessState> MergeAsync(
        ConsciousnessState existingState,
        double iPointPosition,
        Cluster[] clusters,
        ProjectedPoint[] highDimPoints,
        ProjectedPoint[] lowDimPoints,
        EmergentEdge[] edges,
        EmergentShape[] shapes,
        SubjectiveState[] subjectiveStates,
        CancellationToken ct = default);

    /// <summary>
    /// Prunes items with weights below the minimum threshold.
    /// Called periodically to clean up low-influence items.
    /// </summary>
    /// <param name="state">The consciousness state to prune.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Pruned consciousness state.</returns>
    Task<ConsciousnessState> PruneAsync(ConsciousnessState state, CancellationToken ct = default);
}
