using System;
using System.Collections.Generic;
using System.Linq;

namespace SpaceCore.Models;

/// <summary>
/// Represents an emergent shape - a subgraph of connected points that has emerged from processing.
/// </summary>
/// <param name="Id">Unique identifier for this shape.</param>
/// <param name="Points">List of point IDs that form this shape.</param>
/// <param name="Edges">List of edges connecting the points.</param>
/// <param name="Meta">Additional metadata for this shape.</param>
public record EmergentShape(
    Guid Id,
    IList<Guid> Points,
    IList<EmergentEdge> Edges,
    IDictionary<string, object>? Meta = null)
{
    /// <summary>
    /// Gets the number of points in this shape.
    /// </summary>
    public int Size => Points.Count;

    /// <summary>
    /// Gets the density of this shape (ratio of actual edges to possible edges).
    /// </summary>
    public double Density
    {
        get
        {
            if (Points.Count < 2) return 0;
            var possibleEdges = Points.Count * (Points.Count - 1);
            return (double)Edges.Count / possibleEdges;
        }
    }

    /// <summary>
    /// Gets the average edge weight in this shape.
    /// </summary>
    public double AverageEdgeWeight => Edges.Count > 0 ? Edges.Average(e => e.Weight) : 0;

    /// <summary>
    /// Calculates the centrality of each point in this shape.
    /// </summary>
    public Dictionary<Guid, int> CalculateCentrality()
    {
        var centrality = Points.ToDictionary(p => p, _ => 0);
        foreach (var edge in Edges)
        {
            if (centrality.ContainsKey(edge.FromPointId))
                centrality[edge.FromPointId]++;
            if (centrality.ContainsKey(edge.ToPointId))
                centrality[edge.ToPointId]++;
        }
        return centrality;
    }

    /// <summary>
    /// Gets the most central point ID in this shape.
    /// </summary>
    public Guid? MostCentralPoint
    {
        get
        {
            var centrality = CalculateCentrality();
            return centrality.Count > 0
                ? centrality.MaxBy(kv => kv.Value).Key
                : null;
        }
    }

    /// <summary>
    /// Calculates novelty score based on edge types.
    /// </summary>
    public double NoveltyScore
    {
        get
        {
            var noveltyEdges = Edges.Count(e => e.Type == EmergentEdge.EdgeTypes.Novelty);
            return Edges.Count > 0 ? (double)noveltyEdges / Edges.Count : 0;
        }
    }
}
