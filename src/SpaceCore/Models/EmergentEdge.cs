using System;
using System.Collections.Generic;

namespace SpaceCore.Models;

/// <summary>
/// Represents an edge (connection/transition) between two projected points.
/// </summary>
/// <param name="FromPointId">The source point ID.</param>
/// <param name="ToPointId">The target point ID.</param>
/// <param name="Weight">The weight/strength of this edge (0.0 to 1.0).</param>
/// <param name="Type">The type of edge: "temporal", "semantic", "causal", etc.</param>
/// <param name="Meta">Additional metadata for this edge.</param>
public record EmergentEdge(
    Guid FromPointId,
    Guid ToPointId,
    double Weight,
    string Type,
    IDictionary<string, object>? Meta = null)
{
    /// <summary>
    /// Edge types used in the system.
    /// </summary>
    public static class EdgeTypes
    {
        public const string Temporal = "temporal";
        public const string Semantic = "semantic";
        public const string Causal = "causal";
        public const string Similarity = "similarity";
        public const string Novelty = "novelty";
    }

    /// <summary>
    /// Checks if this edge is bidirectional.
    /// </summary>
    public bool IsBidirectional => Meta?.ContainsKey("bidirectional") == true && (bool)Meta["bidirectional"];

    /// <summary>
    /// Creates a reverse edge (swapping from and to).
    /// </summary>
    public EmergentEdge Reverse() => this with { FromPointId = ToPointId, ToPointId = FromPointId };
}
