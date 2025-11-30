using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SpaceCore.Models;

/// <summary>
/// Represents the complete memorized consciousness state across all pipeline stages.
/// This state persists across percept processing cycles and is modified based on
/// the I-point position (entropy vs determinism influence).
/// </summary>
public record ConsciousnessState
{
    /// <summary>
    /// Unique identifier for this consciousness state.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Session ID this state belongs to.
    /// </summary>
    public Guid SessionId { get; init; }

    /// <summary>
    /// When this state was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// When this state was last modified.
    /// </summary>
    public DateTimeOffset ModifiedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Version counter for optimistic concurrency.
    /// </summary>
    public long Version { get; init; } = 1;

    /// <summary>
    /// Storage for clusters (from ClusterAsync stage).
    /// Maps cluster ID to weighted cluster with influence weight.
    /// </summary>
    public Dictionary<Guid, WeightedCluster> ClusterStorage { get; init; } = new();

    /// <summary>
    /// Storage for high-dimensional projected points (from ProjectBatchAsync stage).
    /// Maps point ID to weighted point with influence weight.
    /// </summary>
    public Dictionary<Guid, WeightedPoint> HighDimPointStorage { get; init; } = new();

    /// <summary>
    /// Storage for low-dimensional projected points (from DownProjectBatchAsync stage).
    /// Maps point ID to weighted point with influence weight.
    /// </summary>
    public Dictionary<Guid, WeightedPoint> LowDimPointStorage { get; init; } = new();

    /// <summary>
    /// Storage for graph edges (from LinkAsync stage).
    /// Uses composite key "fromId:toId" for uniqueness.
    /// </summary>
    public Dictionary<string, WeightedEdge> EdgeStorage { get; init; } = new();

    /// <summary>
    /// Storage for emergent shapes (from ProcessAsync stage).
    /// Maps shape ID to weighted shape with influence weight.
    /// </summary>
    public Dictionary<Guid, WeightedShape> ShapeStorage { get; init; } = new();

    /// <summary>
    /// Storage for subjective states (from ShapeAsync stage).
    /// Maps state ID to weighted subjective state with influence weight.
    /// </summary>
    public Dictionary<Guid, WeightedSubjectiveState> StateStorage { get; init; } = new();

    /// <summary>
    /// The last known I-point position used for state updates.
    /// </summary>
    public double LastIPointPosition { get; init; } = 0.0;

    /// <summary>
    /// Whether this state has been initialized from entropy.
    /// </summary>
    public bool IsInitialized { get; init; } = false;

    /// <summary>
    /// Creates an empty consciousness state for a session.
    /// </summary>
    public static ConsciousnessState Empty(Guid sessionId) => new()
    {
        SessionId = sessionId,
        IsInitialized = false
    };

    /// <summary>
    /// Creates a copy with updated modification timestamp and incremented version.
    /// </summary>
    public ConsciousnessState WithUpdate() => this with
    {
        ModifiedAt = DateTimeOffset.UtcNow,
        Version = Version + 1
    };
}

/// <summary>
/// A cluster with an associated influence weight.
/// </summary>
public record WeightedCluster(
    Cluster Cluster,
    double Weight,
    DateTimeOffset AddedAt)
{
    /// <summary>
    /// Minimum weight below which the cluster should be pruned.
    /// </summary>
    public const double MinWeight = 0.01;

    /// <summary>
    /// Maximum weight a cluster can have.
    /// </summary>
    public const double MaxWeight = 1.0;

    /// <summary>
    /// Whether this cluster should be pruned (weight too low).
    /// </summary>
    [JsonIgnore]
    public bool ShouldPrune => Weight < MinWeight;

    /// <summary>
    /// Creates a new weighted cluster with adjusted weight.
    /// </summary>
    public WeightedCluster WithWeightDelta(double delta) =>
        this with { Weight = Math.Clamp(Weight + delta, 0, MaxWeight) };
}

/// <summary>
/// A projected point with an associated influence weight.
/// </summary>
public record WeightedPoint(
    ProjectedPoint Point,
    double Weight,
    DateTimeOffset AddedAt)
{
    /// <summary>
    /// Minimum weight below which the point should be pruned.
    /// </summary>
    public const double MinWeight = 0.01;

    /// <summary>
    /// Maximum weight a point can have.
    /// </summary>
    public const double MaxWeight = 1.0;

    /// <summary>
    /// Whether this point should be pruned (weight too low).
    /// </summary>
    [JsonIgnore]
    public bool ShouldPrune => Weight < MinWeight;

    /// <summary>
    /// Creates a new weighted point with adjusted weight.
    /// </summary>
    public WeightedPoint WithWeightDelta(double delta) =>
        this with { Weight = Math.Clamp(Weight + delta, 0, MaxWeight) };
}

/// <summary>
/// An edge with an associated influence weight.
/// </summary>
public record WeightedEdge(
    EmergentEdge Edge,
    double Weight,
    DateTimeOffset AddedAt)
{
    /// <summary>
    /// Minimum weight below which the edge should be pruned.
    /// </summary>
    public const double MinWeight = 0.01;

    /// <summary>
    /// Maximum weight an edge can have.
    /// </summary>
    public const double MaxWeight = 1.0;

    /// <summary>
    /// Whether this edge should be pruned (weight too low).
    /// </summary>
    [JsonIgnore]
    public bool ShouldPrune => Weight < MinWeight;

    /// <summary>
    /// Creates a composite key for edge storage.
    /// </summary>
    public static string CreateKey(Guid fromId, Guid toId) => $"{fromId}:{toId}";

    /// <summary>
    /// Creates a new weighted edge with adjusted weight.
    /// </summary>
    public WeightedEdge WithWeightDelta(double delta) =>
        this with { Weight = Math.Clamp(Weight + delta, 0, MaxWeight) };
}

/// <summary>
/// A shape with an associated influence weight.
/// </summary>
public record WeightedShape(
    EmergentShape Shape,
    double Weight,
    DateTimeOffset AddedAt)
{
    /// <summary>
    /// Minimum weight below which the shape should be pruned.
    /// </summary>
    public const double MinWeight = 0.01;

    /// <summary>
    /// Maximum weight a shape can have.
    /// </summary>
    public const double MaxWeight = 1.0;

    /// <summary>
    /// Whether this shape should be pruned (weight too low).
    /// </summary>
    [JsonIgnore]
    public bool ShouldPrune => Weight < MinWeight;

    /// <summary>
    /// Creates a new weighted shape with adjusted weight.
    /// </summary>
    public WeightedShape WithWeightDelta(double delta) =>
        this with { Weight = Math.Clamp(Weight + delta, 0, MaxWeight) };
}

/// <summary>
/// A subjective state with an associated influence weight.
/// </summary>
public record WeightedSubjectiveState(
    SubjectiveState State,
    double Weight,
    DateTimeOffset AddedAt)
{
    /// <summary>
    /// Minimum weight below which the state should be pruned.
    /// </summary>
    public const double MinWeight = 0.01;

    /// <summary>
    /// Maximum weight a state can have.
    /// </summary>
    public const double MaxWeight = 1.0;

    /// <summary>
    /// Whether this state should be pruned (weight too low).
    /// </summary>
    [JsonIgnore]
    public bool ShouldPrune => Weight < MinWeight;

    /// <summary>
    /// Creates a new weighted state with adjusted weight.
    /// </summary>
    public WeightedSubjectiveState WithWeightDelta(double delta) =>
        this with { Weight = Math.Clamp(Weight + delta, 0, MaxWeight) };
}
