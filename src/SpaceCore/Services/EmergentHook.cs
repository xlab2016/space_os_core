using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SpaceCore.Interfaces;
using SpaceCore.Models;

namespace SpaceCore.Services;

/// <summary>
/// Builds edges (connections) between projected points.
/// Creates temporal, semantic, and similarity-based links.
/// </summary>
public class EmergentHook : IEmergentHook
{
    /// <inheritdoc />
    public Task<EmergentEdge[]> LinkAsync(
        ProjectedPoint[] points,
        LinkOptions options,
        CancellationToken ct = default)
    {
        if (points.Length < 2)
            return Task.FromResult(Array.Empty<EmergentEdge>());

        var edges = new List<EmergentEdge>();

        // Build temporal edges (sequential adjacency)
        for (int i = 0; i < points.Length - 1; i++)
        {
            var windowEnd = Math.Min(i + options.TemporalWindow, points.Length);
            for (int j = i + 1; j < windowEnd; j++)
            {
                var weight = 1.0 - ((double)(j - i) / options.TemporalWindow);
                edges.Add(new EmergentEdge(
                    FromPointId: points[i].Id,
                    ToPointId: points[j].Id,
                    Weight: weight,
                    Type: EmergentEdge.EdgeTypes.Temporal,
                    Meta: new Dictionary<string, object> { ["distance"] = j - i }
                ));
            }
        }

        // Build semantic edges (cosine similarity)
        for (int i = 0; i < points.Length; i++)
        {
            var similarities = new List<(int idx, double sim)>();

            for (int j = i + 1; j < points.Length; j++)
            {
                var similarity = points[i].CosineSimilarity(points[j]);
                if (similarity >= options.SimilarityThreshold)
                {
                    similarities.Add((j, similarity));
                }
            }

            // Keep top edges per point
            var topEdges = similarities
                .OrderByDescending(x => x.sim)
                .Take(options.MaxEdgesPerPoint)
                .ToList();

            foreach (var (idx, sim) in topEdges)
            {
                edges.Add(new EmergentEdge(
                    FromPointId: points[i].Id,
                    ToPointId: points[idx].Id,
                    Weight: sim,
                    Type: EmergentEdge.EdgeTypes.Semantic,
                    Meta: new Dictionary<string, object>
                    {
                        ["similarity"] = sim,
                        ["bidirectional"] = true
                    }
                ));
            }
        }

        // Add novelty edges (connect distant but interesting points)
        var noveltyEdges = CreateNoveltyEdges(points, edges, options);
        edges.AddRange(noveltyEdges);

        return Task.FromResult(edges.ToArray());
    }

    /// <inheritdoc />
    public Task<EmergentEdge[]> LinkNewPointAsync(
        ProjectedPoint newPoint,
        ProjectedPoint[] existingPoints,
        LinkOptions options,
        CancellationToken ct = default)
    {
        if (existingPoints.Length == 0)
            return Task.FromResult(Array.Empty<EmergentEdge>());

        var edges = new List<EmergentEdge>();

        // Temporal edge to the most recent point
        var mostRecent = existingPoints[^1];
        edges.Add(new EmergentEdge(
            FromPointId: mostRecent.Id,
            ToPointId: newPoint.Id,
            Weight: 1.0,
            Type: EmergentEdge.EdgeTypes.Temporal,
            Meta: new Dictionary<string, object> { ["distance"] = 1 }
        ));

        // Semantic edges to similar points
        var similarities = existingPoints
            .Select((p, i) => (point: p, idx: i, sim: newPoint.CosineSimilarity(p)))
            .Where(x => x.sim >= options.SimilarityThreshold)
            .OrderByDescending(x => x.sim)
            .Take(options.MaxEdgesPerPoint)
            .ToList();

        foreach (var (point, _, sim) in similarities)
        {
            edges.Add(new EmergentEdge(
                FromPointId: newPoint.Id,
                ToPointId: point.Id,
                Weight: sim,
                Type: EmergentEdge.EdgeTypes.Semantic,
                Meta: new Dictionary<string, object>
                {
                    ["similarity"] = sim,
                    ["bidirectional"] = true
                }
            ));
        }

        return Task.FromResult(edges.ToArray());
    }

    /// <inheritdoc />
    public EmergentEdge[] PruneEdges(EmergentEdge[] edges, double minWeight = 0.3)
    {
        return edges.Where(e => e.Weight >= minWeight).ToArray();
    }

    private List<EmergentEdge> CreateNoveltyEdges(
        ProjectedPoint[] points,
        List<EmergentEdge> existingEdges,
        LinkOptions options)
    {
        var noveltyEdges = new List<EmergentEdge>();

        // Find points that are not well connected (low degree)
        var degrees = points.ToDictionary(p => p.Id, _ => 0);
        foreach (var edge in existingEdges)
        {
            if (degrees.ContainsKey(edge.FromPointId))
                degrees[edge.FromPointId]++;
            if (degrees.ContainsKey(edge.ToPointId))
                degrees[edge.ToPointId]++;
        }

        var isolatedPoints = points
            .Where(p => degrees[p.Id] < 2)
            .ToList();

        // Connect isolated points to create exploration paths
        for (int i = 0; i < isolatedPoints.Count; i++)
        {
            for (int j = i + 1; j < isolatedPoints.Count && j < i + 3; j++)
            {
                var distance = isolatedPoints[i].DistanceTo(isolatedPoints[j]);
                var weight = Math.Exp(-distance); // Exponential decay

                if (weight > 0.1)
                {
                    noveltyEdges.Add(new EmergentEdge(
                        FromPointId: isolatedPoints[i].Id,
                        ToPointId: isolatedPoints[j].Id,
                        Weight: weight,
                        Type: EmergentEdge.EdgeTypes.Novelty,
                        Meta: new Dictionary<string, object>
                        {
                            ["distance"] = distance,
                            ["exploratory"] = true
                        }
                    ));
                }
            }
        }

        return noveltyEdges;
    }
}
