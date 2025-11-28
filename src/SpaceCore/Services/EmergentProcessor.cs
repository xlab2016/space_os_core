using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SpaceCore.Interfaces;
using SpaceCore.Models;

namespace SpaceCore.Services;

/// <summary>
/// Processes graphs of connected points to generate emergent shapes.
/// Implements activation propagation and connected component detection.
/// </summary>
public class EmergentProcessor : IEmergentProcessor
{
    /// <inheritdoc />
    public Task<EmergentShape[]> ProcessAsync(
        ProjectedPoint[] points,
        EmergentEdge[] edges,
        ProcessorOptions options,
        CancellationToken ct = default)
    {
        if (points.Length == 0)
            return Task.FromResult(Array.Empty<EmergentShape>());

        // Initialize activations based on point confidence
        var activations = points.ToDictionary(
            p => p.Id,
            p => p.Confidence
        );

        // Run activation propagation
        for (int step = 0; step < options.Steps; step++)
        {
            activations = PropagateStep(points, edges, activations, options);
        }

        // Find active subgraphs (shapes)
        var shapes = ExtractShapes(points, edges, activations, options);

        return Task.FromResult(shapes);
    }

    /// <inheritdoc />
    public Dictionary<Guid, double> PropagateStep(
        ProjectedPoint[] points,
        EmergentEdge[] edges,
        Dictionary<Guid, double> activations,
        ProcessorOptions options)
    {
        var newActivations = new Dictionary<Guid, double>();

        foreach (var point in points)
        {
            // Start with decayed current activation
            double newActivation = activations.GetValueOrDefault(point.Id, 0) * options.DecayFactor;

            // Add incoming activation from edges
            foreach (var edge in edges.Where(e => e.ToPointId == point.Id))
            {
                var sourceActivation = activations.GetValueOrDefault(edge.FromPointId, 0);
                newActivation += sourceActivation * edge.Weight * (1 - options.DecayFactor);
            }

            // Also consider reverse edges for bidirectional propagation
            foreach (var edge in edges.Where(e => e.FromPointId == point.Id && e.IsBidirectional))
            {
                var sourceActivation = activations.GetValueOrDefault(edge.ToPointId, 0);
                newActivation += sourceActivation * edge.Weight * (1 - options.DecayFactor) * 0.5;
            }

            // Apply activation threshold
            newActivations[point.Id] = Math.Min(1.0, Math.Max(0, newActivation));
        }

        return newActivations;
    }

    /// <inheritdoc />
    public EmergentShape[] DetectComponents(ProjectedPoint[] points, EmergentEdge[] edges)
    {
        if (points.Length == 0)
            return Array.Empty<EmergentShape>();

        var pointIds = points.Select(p => p.Id).ToHashSet();
        var visited = new HashSet<Guid>();
        var components = new List<EmergentShape>();

        // Build adjacency list
        var adjacency = new Dictionary<Guid, List<(Guid neighbor, EmergentEdge edge)>>();
        foreach (var id in pointIds)
        {
            adjacency[id] = new List<(Guid, EmergentEdge)>();
        }

        foreach (var edge in edges)
        {
            if (adjacency.ContainsKey(edge.FromPointId) && adjacency.ContainsKey(edge.ToPointId))
            {
                adjacency[edge.FromPointId].Add((edge.ToPointId, edge));
                if (edge.IsBidirectional)
                {
                    adjacency[edge.ToPointId].Add((edge.FromPointId, edge));
                }
            }
        }

        // BFS to find connected components
        foreach (var startId in pointIds)
        {
            if (visited.Contains(startId))
                continue;

            var componentPoints = new List<Guid>();
            var componentEdges = new List<EmergentEdge>();
            var queue = new Queue<Guid>();

            queue.Enqueue(startId);
            visited.Add(startId);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                componentPoints.Add(current);

                foreach (var (neighbor, edge) in adjacency[current])
                {
                    componentEdges.Add(edge);

                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            // Remove duplicate edges
            componentEdges = componentEdges
                .GroupBy(e => (Math.Min(e.FromPointId.GetHashCode(), e.ToPointId.GetHashCode()),
                               Math.Max(e.FromPointId.GetHashCode(), e.ToPointId.GetHashCode())))
                .Select(g => g.First())
                .ToList();

            components.Add(new EmergentShape(
                Id: Guid.NewGuid(),
                Points: componentPoints,
                Edges: componentEdges,
                Meta: new Dictionary<string, object>
                {
                    ["componentSize"] = componentPoints.Count,
                    ["detectionMethod"] = "bfs"
                }
            ));
        }

        return components.ToArray();
    }

    private EmergentShape[] ExtractShapes(
        ProjectedPoint[] points,
        EmergentEdge[] edges,
        Dictionary<Guid, double> activations,
        ProcessorOptions options)
    {
        // Filter to active points only
        var activePoints = points
            .Where(p => activations.GetValueOrDefault(p.Id, 0) >= options.ActivationThreshold)
            .ToArray();

        if (activePoints.Length == 0)
        {
            // Return single shape with all points if none meet threshold
            return new[]
            {
                new EmergentShape(
                    Id: Guid.NewGuid(),
                    Points: points.Select(p => p.Id).ToList(),
                    Edges: edges.ToList(),
                    Meta: new Dictionary<string, object>
                    {
                        ["activationType"] = "fallback",
                        ["avgActivation"] = activations.Values.DefaultIfEmpty(0).Average()
                    }
                )
            };
        }

        // Filter edges to only include active points
        var activePointIds = activePoints.Select(p => p.Id).ToHashSet();
        var activeEdges = edges
            .Where(e => activePointIds.Contains(e.FromPointId) && activePointIds.Contains(e.ToPointId))
            .ToArray();

        // Detect connected components among active points
        var components = DetectComponents(activePoints, activeEdges);

        // Enrich shapes with activation metadata
        for (int i = 0; i < components.Length; i++)
        {
            var shape = components[i];
            var shapeActivations = shape.Points
                .Select(id => activations.GetValueOrDefault(id, 0))
                .ToList();

            var meta = new Dictionary<string, object>(shape.Meta ?? new Dictionary<string, object>())
            {
                ["avgActivation"] = shapeActivations.Average(),
                ["maxActivation"] = shapeActivations.Max(),
                ["minActivation"] = shapeActivations.Min(),
                ["processingSteps"] = options.Steps
            };

            components[i] = shape with { Meta = meta };
        }

        return components;
    }
}
