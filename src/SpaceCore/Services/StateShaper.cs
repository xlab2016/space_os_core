using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SpaceCore.Interfaces;
using SpaceCore.Models;

namespace SpaceCore.Services;

/// <summary>
/// Maps emergent shapes to human-like subjective states.
/// Computes valence, arousal, intensity and generates narratives.
/// </summary>
public class StateShaper : IStateShaper
{
    private readonly string[] _narrativeTemplates =
    {
        "A sense of {kind} emerges, characterized by {description}.",
        "The emergent pattern suggests {kind} with {description}.",
        "A {intensity} {kind} arises, marked by {description}.",
        "The consciousness perceives {kind}: {description}.",
        "An awareness of {kind} develops, notable for {description}."
    };

    private readonly Random _templateSelector = new();

    /// <inheritdoc />
    public async Task<SubjectiveState[]> ShapeAsync(
        EmergentShape[] shapes,
        ShaperOptions options,
        CancellationToken ct = default)
    {
        var states = new SubjectiveState[shapes.Length];
        for (int i = 0; i < shapes.Length; i++)
        {
            states[i] = await ShapeSingleAsync(shapes[i], options, ct);
        }
        return states;
    }

    /// <inheritdoc />
    public Task<SubjectiveState> ShapeSingleAsync(
        EmergentShape shape,
        ShaperOptions options,
        CancellationToken ct = default)
    {
        // Calculate valence from shape characteristics
        var valence = CalculateValence(shape, options);

        // Calculate arousal from shape density and activity
        var arousal = CalculateArousal(shape, options);

        // Calculate intensity from shape size and edge weights
        var intensity = CalculateIntensity(shape);

        // Determine kind based on characteristics
        var kind = DetermineKind(valence, arousal, intensity);

        // Generate semantic tags
        var semanticTags = GenerateSemanticTags(shape, valence, arousal, intensity);

        // Create trace
        var trace = CreateTrace(shape, options);

        // Generate initial narrative
        var narrative = GenerateSimpleNarrative(kind, valence, arousal, intensity, semanticTags);

        var state = new SubjectiveState(
            Id: Guid.NewGuid(),
            Kind: kind,
            Valence: valence,
            Arousal: arousal,
            Intensity: intensity,
            Narrative: narrative,
            Trace: trace,
            SemanticTags: semanticTags,
            Meta: new Dictionary<string, object>
            {
                ["shapeId"] = shape.Id,
                ["shapeSize"] = shape.Size,
                ["shapeDensity"] = shape.Density
            }
        );

        return Task.FromResult(state);
    }

    /// <inheritdoc />
    public Task<SubjectiveState> GenerateNarrativeAsync(
        SubjectiveState state,
        ContextState? context = null,
        CancellationToken ct = default)
    {
        // Enhanced narrative generation (would use LLM in production)
        var contextInfo = context != null
            ? $" in the context of {string.Join(", ", context.ActiveTags.Take(3))}"
            : "";

        var intensityWord = state.Intensity switch
        {
            >= 0.8 => "profound",
            >= 0.6 => "strong",
            >= 0.4 => "moderate",
            >= 0.2 => "subtle",
            _ => "faint"
        };

        var valenceWord = state.Valence switch
        {
            >= 0.5 => "positive and uplifting",
            >= 0.2 => "mildly positive",
            >= -0.2 => "balanced and neutral",
            >= -0.5 => "mildly negative",
            _ => "challenging"
        };

        var arousalWord = state.Arousal switch
        {
            >= 0.7 => "highly energetic",
            >= 0.4 => "moderately active",
            _ => "calm and settled"
        };

        var enhancedNarrative = $"A {intensityWord} {state.Kind} emerges{contextInfo}. " +
            $"The experience is {valenceWord} with {arousalWord} energy. " +
            (state.SemanticTags?.Any() == true
                ? $"Key themes include: {string.Join(", ", state.SemanticTags.Take(3))}."
                : "");

        return Task.FromResult(state with { Narrative = enhancedNarrative });
    }

    private double CalculateValence(EmergentShape shape, ShaperOptions options)
    {
        // Calculate valence based on centrality distribution and novelty
        var centrality = shape.CalculateCentrality();
        if (centrality.Count == 0) return 0;

        var avgCentrality = centrality.Values.Average();
        var maxCentrality = centrality.Values.Max();

        // High centrality variance suggests more negative valence (tension)
        var variance = centrality.Values.Average(c => Math.Pow(c - avgCentrality, 2));

        // Novelty tends toward positive valence
        var noveltyBoost = shape.NoveltyScore * 0.3;

        var valence = (avgCentrality / (maxCentrality + 1) - variance * 0.5 + noveltyBoost) * options.ValenceWeight * 2 - 0.5;
        return Math.Clamp(valence, -1.0, 1.0);
    }

    private double CalculateArousal(EmergentShape shape, ShaperOptions options)
    {
        // Arousal correlates with density and average edge weight
        var densityComponent = shape.Density * options.ArousalWeight;
        var weightComponent = shape.AverageEdgeWeight * (1 - options.ArousalWeight);

        // More edges = higher arousal
        var edgeRatio = Math.Min(1.0, (double)shape.Edges.Count / (shape.Points.Count * 2));

        var arousal = (densityComponent + weightComponent + edgeRatio) / 3;
        return Math.Clamp(arousal, 0, 1.0);
    }

    private double CalculateIntensity(EmergentShape shape)
    {
        // Intensity based on shape size and connectivity
        var sizeComponent = Math.Min(1.0, shape.Size / 10.0);
        var connectivityComponent = shape.Edges.Count > 0
            ? Math.Min(1.0, shape.AverageEdgeWeight * shape.Density * 2)
            : 0;

        // Meta-based intensity if available
        double metaIntensity = 0;
        if (shape.Meta?.TryGetValue("avgActivation", out var activation) == true)
        {
            metaIntensity = Convert.ToDouble(activation);
        }

        var intensity = (sizeComponent + connectivityComponent + metaIntensity) / 3;
        return Math.Clamp(intensity, 0, 1.0);
    }

    private string DetermineKind(double valence, double arousal, double intensity)
    {
        // Map to circumplex model of affect
        if (arousal > 0.6)
        {
            if (valence > 0.3) return SubjectiveState.StateKinds.Creativity;
            if (valence < -0.3) return SubjectiveState.StateKinds.Emotion;
            return SubjectiveState.StateKinds.Curiosity;
        }
        else if (arousal > 0.3)
        {
            if (valence > 0.2) return SubjectiveState.StateKinds.Feeling;
            if (valence < -0.2) return SubjectiveState.StateKinds.Thought;
            return SubjectiveState.StateKinds.Mood;
        }
        else
        {
            if (intensity > 0.5) return SubjectiveState.StateKinds.Intention;
            return SubjectiveState.StateKinds.Mood;
        }
    }

    private IList<string> GenerateSemanticTags(EmergentShape shape, double valence, double arousal, double intensity)
    {
        var tags = new List<string>();

        // Valence-based tags
        if (valence > 0.5) tags.Add("positive");
        else if (valence > 0) tags.Add("mildly-positive");
        else if (valence > -0.5) tags.Add("mildly-negative");
        else tags.Add("negative");

        // Arousal-based tags
        if (arousal > 0.7) tags.Add("high-energy");
        else if (arousal > 0.3) tags.Add("moderate-energy");
        else tags.Add("low-energy");

        // Shape-based tags
        if (shape.Size > 5) tags.Add("complex");
        if (shape.Density > 0.5) tags.Add("dense");
        if (shape.NoveltyScore > 0.3) tags.Add("novel");
        if (shape.AverageEdgeWeight > 0.7) tags.Add("strongly-connected");

        // Edge type tags
        var edgeTypes = shape.Edges.GroupBy(e => e.Type).Select(g => g.Key).ToList();
        if (edgeTypes.Contains(EmergentEdge.EdgeTypes.Causal)) tags.Add("causal-pattern");
        if (edgeTypes.Contains(EmergentEdge.EdgeTypes.Novelty)) tags.Add("exploratory");

        return tags;
    }

    private string CreateTrace(EmergentShape shape, ShaperOptions options)
    {
        var trace = new
        {
            shapeId = shape.Id,
            pointCount = shape.Points.Count,
            edgeCount = shape.Edges.Count,
            density = shape.Density,
            noveltyScore = shape.NoveltyScore,
            options = new
            {
                useNarrativeGeneration = options.UseNarrativeGeneration,
                valenceWeight = options.ValenceWeight,
                arousalWeight = options.ArousalWeight
            },
            timestamp = DateTimeOffset.UtcNow
        };
        return JsonSerializer.Serialize(trace);
    }

    private string GenerateSimpleNarrative(
        string kind,
        double valence,
        double arousal,
        double intensity,
        IList<string> tags)
    {
        var template = _narrativeTemplates[_templateSelector.Next(_narrativeTemplates.Length)];

        var intensityWord = intensity switch
        {
            >= 0.7 => "strong",
            >= 0.4 => "moderate",
            _ => "subtle"
        };

        var description = tags.Count > 0
            ? string.Join(", ", tags.Take(3))
            : $"{(valence > 0 ? "positive" : "neutral")} energy";

        return template
            .Replace("{kind}", kind)
            .Replace("{intensity}", intensityWord)
            .Replace("{description}", description);
    }
}
