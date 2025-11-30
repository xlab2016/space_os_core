using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SpaceCore.Interfaces;
using SpaceCore.Models;

namespace SpaceCore.Services;

/// <summary>
/// Implements consciousness state merging based on I-point position.
///
/// The I-point position determines how strongly new entropy affects existing state:
/// - Position closer to +1.0 (entropic/right): entropy has stronger impact on state
/// - Position closer to -1.0 (deterministic/left): entropy has weaker impact,
///   but existing "Thought" patterns are reinforced more strongly
///
/// Merge strategy:
/// 1. Decay existing weights based on time and I-point position
/// 2. Add new items with initial weight scaled by entropy factor
/// 3. Strengthen similar existing items when new items reinforce them
/// 4. Prune items below minimum weight threshold
/// </summary>
public class ConsciousnessStateMerger : IConsciousnessStateMerger
{
    /// <summary>
    /// Base decay factor applied to existing items per merge cycle.
    /// </summary>
    private const double BaseDecayFactor = 0.95;

    /// <summary>
    /// Base weight for new items when I-point is at center (0.0).
    /// </summary>
    private const double BaseNewItemWeight = 0.5;

    /// <summary>
    /// Maximum number of items to keep in each storage.
    /// </summary>
    private const int MaxStorageSize = 1000;

    /// <summary>
    /// Similarity threshold for merging similar items.
    /// </summary>
    private const double SimilarityThreshold = 0.8;

    /// <inheritdoc />
    public Task<ConsciousnessState> InitializeFromEntropyAsync(
        ConsciousnessState state,
        Cluster[] clusters,
        ProjectedPoint[] highDimPoints,
        ProjectedPoint[] lowDimPoints,
        EmergentEdge[] edges,
        EmergentShape[] shapes,
        SubjectiveState[] subjectiveStates,
        CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        // Initialize with full weight for all items from entropy
        var clusterStorage = clusters.ToDictionary(
            c => c.Id,
            c => new WeightedCluster(c, 1.0, now));

        var highDimStorage = highDimPoints.ToDictionary(
            p => p.Id,
            p => new WeightedPoint(p, 1.0, now));

        var lowDimStorage = lowDimPoints.ToDictionary(
            p => p.Id,
            p => new WeightedPoint(p, 1.0, now));

        var edgeStorage = edges.ToDictionary(
            e => WeightedEdge.CreateKey(e.FromPointId, e.ToPointId),
            e => new WeightedEdge(e, 1.0, now));

        var shapeStorage = shapes.ToDictionary(
            s => s.Id,
            s => new WeightedShape(s, 1.0, now));

        var stateStorage = subjectiveStates.ToDictionary(
            s => s.Id,
            s => new WeightedSubjectiveState(s, 1.0, now));

        var initializedState = state with
        {
            ClusterStorage = clusterStorage,
            HighDimPointStorage = highDimStorage,
            LowDimPointStorage = lowDimStorage,
            EdgeStorage = edgeStorage,
            ShapeStorage = shapeStorage,
            StateStorage = stateStorage,
            IsInitialized = true,
            LastIPointPosition = 0.0,
            ModifiedAt = now
        };

        return Task.FromResult(initializedState);
    }

    /// <inheritdoc />
    public Task<ConsciousnessState> MergeAsync(
        ConsciousnessState existingState,
        double iPointPosition,
        Cluster[] clusters,
        ProjectedPoint[] highDimPoints,
        ProjectedPoint[] lowDimPoints,
        EmergentEdge[] edges,
        EmergentShape[] shapes,
        SubjectiveState[] subjectiveStates,
        CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        // Calculate influence factors based on I-point position
        // EntropyFactor: how strongly new entropy affects state (higher when entropic)
        // DeterminismFactor: how strongly existing patterns are preserved (higher when deterministic)
        var entropyFactor = CalculateEntropyFactor(iPointPosition);
        var determinismFactor = CalculateDeterminismFactor(iPointPosition);

        // Calculate the decay factor - existing items decay more in entropic mode
        var decayFactor = BaseDecayFactor * (0.5 + 0.5 * determinismFactor);

        // Calculate initial weight for new items - higher in entropic mode
        var newItemWeight = BaseNewItemWeight * entropyFactor;

        // Merge each storage type
        var mergedClusters = MergeClusters(
            existingState.ClusterStorage, clusters, decayFactor, newItemWeight, now);

        var mergedHighDimPoints = MergePoints(
            existingState.HighDimPointStorage, highDimPoints, decayFactor, newItemWeight, now);

        var mergedLowDimPoints = MergePoints(
            existingState.LowDimPointStorage, lowDimPoints, decayFactor, newItemWeight, now);

        var mergedEdges = MergeEdges(
            existingState.EdgeStorage, edges, decayFactor, newItemWeight, now);

        var mergedShapes = MergeShapes(
            existingState.ShapeStorage, shapes, decayFactor, newItemWeight, now);

        var mergedStates = MergeSubjectiveStates(
            existingState.StateStorage, subjectiveStates, decayFactor, newItemWeight, now, determinismFactor);

        var mergedState = existingState with
        {
            ClusterStorage = mergedClusters,
            HighDimPointStorage = mergedHighDimPoints,
            LowDimPointStorage = mergedLowDimPoints,
            EdgeStorage = mergedEdges,
            ShapeStorage = mergedShapes,
            StateStorage = mergedStates,
            LastIPointPosition = iPointPosition,
            ModifiedAt = now
        };

        return Task.FromResult(mergedState.WithUpdate());
    }

    /// <inheritdoc />
    public Task<ConsciousnessState> PruneAsync(ConsciousnessState state, CancellationToken ct = default)
    {
        // Remove items below minimum weight
        var prunedClusters = state.ClusterStorage
            .Where(kv => !kv.Value.ShouldPrune)
            .OrderByDescending(kv => kv.Value.Weight)
            .Take(MaxStorageSize)
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        var prunedHighDimPoints = state.HighDimPointStorage
            .Where(kv => !kv.Value.ShouldPrune)
            .OrderByDescending(kv => kv.Value.Weight)
            .Take(MaxStorageSize)
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        var prunedLowDimPoints = state.LowDimPointStorage
            .Where(kv => !kv.Value.ShouldPrune)
            .OrderByDescending(kv => kv.Value.Weight)
            .Take(MaxStorageSize)
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        var prunedEdges = state.EdgeStorage
            .Where(kv => !kv.Value.ShouldPrune)
            .OrderByDescending(kv => kv.Value.Weight)
            .Take(MaxStorageSize * 2)
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        var prunedShapes = state.ShapeStorage
            .Where(kv => !kv.Value.ShouldPrune)
            .OrderByDescending(kv => kv.Value.Weight)
            .Take(MaxStorageSize)
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        var prunedStates = state.StateStorage
            .Where(kv => !kv.Value.ShouldPrune)
            .OrderByDescending(kv => kv.Value.Weight)
            .Take(MaxStorageSize)
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        var prunedState = state with
        {
            ClusterStorage = prunedClusters,
            HighDimPointStorage = prunedHighDimPoints,
            LowDimPointStorage = prunedLowDimPoints,
            EdgeStorage = prunedEdges,
            ShapeStorage = prunedShapes,
            StateStorage = prunedStates
        };

        return Task.FromResult(prunedState.WithUpdate());
    }

    /// <summary>
    /// Calculates the entropy factor based on I-point position.
    /// Returns value between 0.0 and 1.0.
    /// Higher when I-point is in entropic region (right).
    /// </summary>
    private static double CalculateEntropyFactor(double iPointPosition)
    {
        // Map from [-1, +1] to [0, 1]
        // -1 (deterministic) -> 0.1 (minimal entropy influence)
        // 0 (balanced) -> 0.5
        // +1 (entropic) -> 1.0 (maximum entropy influence)
        return 0.1 + 0.9 * ((iPointPosition + 1.0) / 2.0);
    }

    /// <summary>
    /// Calculates the determinism factor based on I-point position.
    /// Returns value between 0.0 and 1.0.
    /// Higher when I-point is in deterministic region (left).
    /// </summary>
    private static double CalculateDeterminismFactor(double iPointPosition)
    {
        // Map from [-1, +1] to [1, 0]
        // -1 (deterministic) -> 1.0 (maximum pattern preservation)
        // 0 (balanced) -> 0.5
        // +1 (entropic) -> 0.1 (minimal pattern preservation)
        return 0.1 + 0.9 * ((1.0 - iPointPosition) / 2.0);
    }

    private Dictionary<Guid, WeightedCluster> MergeClusters(
        Dictionary<Guid, WeightedCluster> existing,
        Cluster[] incoming,
        double decayFactor,
        double newWeight,
        DateTimeOffset now)
    {
        var result = new Dictionary<Guid, WeightedCluster>();

        // Apply decay to existing clusters
        foreach (var (id, wc) in existing)
        {
            var decayed = wc with { Weight = wc.Weight * decayFactor };
            if (!decayed.ShouldPrune)
            {
                result[id] = decayed;
            }
        }

        // Add or merge incoming clusters
        foreach (var cluster in incoming)
        {
            if (result.TryGetValue(cluster.Id, out var existingCluster))
            {
                // Reinforce existing cluster
                var reinforced = existingCluster.WithWeightDelta(newWeight * 0.5);
                result[cluster.Id] = reinforced;
            }
            else
            {
                // Find similar cluster by centroid distance
                var similar = FindSimilarCluster(result.Values, cluster);
                if (similar != null)
                {
                    // Reinforce similar cluster
                    result[similar.Cluster.Id] = similar.WithWeightDelta(newWeight * 0.3);
                }
                else
                {
                    // Add new cluster
                    result[cluster.Id] = new WeightedCluster(cluster, newWeight, now);
                }
            }
        }

        return result;
    }

    private WeightedCluster? FindSimilarCluster(
        IEnumerable<WeightedCluster> clusters,
        Cluster target)
    {
        return clusters
            .Select(wc => new
            {
                Cluster = wc,
                Similarity = ComputeCosineSimilarity(wc.Cluster.Centroid, target.Centroid)
            })
            .Where(x => x.Similarity >= SimilarityThreshold)
            .OrderByDescending(x => x.Similarity)
            .Select(x => x.Cluster)
            .FirstOrDefault();
    }

    private Dictionary<Guid, WeightedPoint> MergePoints(
        Dictionary<Guid, WeightedPoint> existing,
        ProjectedPoint[] incoming,
        double decayFactor,
        double newWeight,
        DateTimeOffset now)
    {
        var result = new Dictionary<Guid, WeightedPoint>();

        // Apply decay to existing points
        foreach (var (id, wp) in existing)
        {
            var decayed = wp with { Weight = wp.Weight * decayFactor };
            if (!decayed.ShouldPrune)
            {
                result[id] = decayed;
            }
        }

        // Add or merge incoming points
        foreach (var point in incoming)
        {
            if (result.TryGetValue(point.Id, out var existingPoint))
            {
                // Reinforce existing point
                var reinforced = existingPoint.WithWeightDelta(newWeight * 0.5);
                result[point.Id] = reinforced;
            }
            else
            {
                // Find similar point by vector distance
                var similar = FindSimilarPoint(result.Values, point);
                if (similar != null)
                {
                    // Reinforce similar point
                    result[similar.Point.Id] = similar.WithWeightDelta(newWeight * 0.3);
                }
                else
                {
                    // Add new point
                    result[point.Id] = new WeightedPoint(point, newWeight, now);
                }
            }
        }

        return result;
    }

    private WeightedPoint? FindSimilarPoint(
        IEnumerable<WeightedPoint> points,
        ProjectedPoint target)
    {
        return points
            .Where(wp => wp.Point.Vector.Length == target.Vector.Length)
            .Select(wp => new
            {
                Point = wp,
                Similarity = ComputeCosineSimilarity(wp.Point.Vector, target.Vector)
            })
            .Where(x => x.Similarity >= SimilarityThreshold)
            .OrderByDescending(x => x.Similarity)
            .Select(x => x.Point)
            .FirstOrDefault();
    }

    private Dictionary<string, WeightedEdge> MergeEdges(
        Dictionary<string, WeightedEdge> existing,
        EmergentEdge[] incoming,
        double decayFactor,
        double newWeight,
        DateTimeOffset now)
    {
        var result = new Dictionary<string, WeightedEdge>();

        // Apply decay to existing edges
        foreach (var (key, we) in existing)
        {
            var decayed = we with { Weight = we.Weight * decayFactor };
            if (!decayed.ShouldPrune)
            {
                result[key] = decayed;
            }
        }

        // Add or reinforce incoming edges
        foreach (var edge in incoming)
        {
            var key = WeightedEdge.CreateKey(edge.FromPointId, edge.ToPointId);

            if (result.TryGetValue(key, out var existingEdge))
            {
                // Reinforce existing edge
                var reinforced = existingEdge.WithWeightDelta(newWeight * 0.5);
                result[key] = reinforced;
            }
            else
            {
                // Add new edge
                result[key] = new WeightedEdge(edge, newWeight, now);
            }
        }

        return result;
    }

    private Dictionary<Guid, WeightedShape> MergeShapes(
        Dictionary<Guid, WeightedShape> existing,
        EmergentShape[] incoming,
        double decayFactor,
        double newWeight,
        DateTimeOffset now)
    {
        var result = new Dictionary<Guid, WeightedShape>();

        // Apply decay to existing shapes
        foreach (var (id, ws) in existing)
        {
            var decayed = ws with { Weight = ws.Weight * decayFactor };
            if (!decayed.ShouldPrune)
            {
                result[id] = decayed;
            }
        }

        // Add incoming shapes
        foreach (var shape in incoming)
        {
            if (result.TryGetValue(shape.Id, out var existingShape))
            {
                // Reinforce existing shape
                var reinforced = existingShape.WithWeightDelta(newWeight * 0.5);
                result[shape.Id] = reinforced;
            }
            else
            {
                // Add new shape
                result[shape.Id] = new WeightedShape(shape, newWeight, now);
            }
        }

        return result;
    }

    private Dictionary<Guid, WeightedSubjectiveState> MergeSubjectiveStates(
        Dictionary<Guid, WeightedSubjectiveState> existing,
        SubjectiveState[] incoming,
        double decayFactor,
        double newWeight,
        DateTimeOffset now,
        double determinismFactor)
    {
        var result = new Dictionary<Guid, WeightedSubjectiveState>();

        // Apply decay to existing states, but preserve "Thought" patterns more when deterministic
        foreach (var (id, ws) in existing)
        {
            var stateDecay = decayFactor;

            // Thought patterns are preserved more strongly when in deterministic mode
            if (ws.State.Kind == SubjectiveState.StateKinds.Thought)
            {
                stateDecay = decayFactor + (1.0 - decayFactor) * determinismFactor * 0.5;
            }

            var decayed = ws with { Weight = ws.Weight * stateDecay };
            if (!decayed.ShouldPrune)
            {
                result[id] = decayed;
            }
        }

        // Add incoming states
        foreach (var state in incoming)
        {
            if (result.TryGetValue(state.Id, out var existingState))
            {
                // Reinforce existing state
                var reinforced = existingState.WithWeightDelta(newWeight * 0.5);
                result[state.Id] = reinforced;
            }
            else
            {
                // Find similar state by kind and semantic overlap
                var similar = FindSimilarState(result.Values, state);
                if (similar != null)
                {
                    // Reinforce similar state
                    result[similar.State.Id] = similar.WithWeightDelta(newWeight * 0.3);
                }
                else
                {
                    // Add new state
                    result[state.Id] = new WeightedSubjectiveState(state, newWeight, now);
                }
            }
        }

        return result;
    }

    private WeightedSubjectiveState? FindSimilarState(
        IEnumerable<WeightedSubjectiveState> states,
        SubjectiveState target)
    {
        return states
            .Where(ws => ws.State.Kind == target.Kind)
            .Select(ws => new
            {
                State = ws,
                Similarity = ComputeSemanticSimilarity(ws.State, target)
            })
            .Where(x => x.Similarity >= SimilarityThreshold)
            .OrderByDescending(x => x.Similarity)
            .Select(x => x.State)
            .FirstOrDefault();
    }

    private static double ComputeSemanticSimilarity(SubjectiveState a, SubjectiveState b)
    {
        // Compare based on semantic tags overlap and valence/arousal proximity
        var tagOverlap = 0.0;
        if (a.SemanticTags != null && b.SemanticTags != null && a.SemanticTags.Count > 0)
        {
            var intersection = a.SemanticTags.Intersect(b.SemanticTags).Count();
            var union = a.SemanticTags.Union(b.SemanticTags).Count();
            tagOverlap = union > 0 ? (double)intersection / union : 0;
        }

        // Valence and arousal proximity (1.0 when identical, 0.0 when max different)
        var valenceSim = 1.0 - Math.Abs(a.Valence - b.Valence) / 2.0;
        var arousalSim = 1.0 - Math.Abs(a.Arousal - b.Arousal);

        // Weighted combination
        return tagOverlap * 0.4 + valenceSim * 0.3 + arousalSim * 0.3;
    }

    private static double ComputeCosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length) return 0;

        double dotProduct = 0, normA = 0, normB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dotProduct += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        if (normA == 0 || normB == 0) return 0;
        return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }
}
