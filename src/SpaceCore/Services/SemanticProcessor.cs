using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SpaceCore.Interfaces;
using SpaceCore.Models;

namespace SpaceCore.Services;

/// <summary>
/// Processes semantic content using memory and (mock) LLM integration.
/// Generates thoughts and manages semantic memory.
/// </summary>
public class SemanticProcessor : ISemanticProcessor
{
    private readonly List<StoredThought> _memoryStore = new();
    private readonly object _memoryLock = new();

    /// <inheritdoc />
    public Task<Thought[]> GenerateThoughtsAsync(
        SubjectiveState[] states,
        SemanticOptions options,
        CancellationToken ct = default)
    {
        var thoughts = new List<Thought>();

        foreach (var state in states)
        {
            // Generate thought based on state
            var thought = GenerateThought(state, options);
            thoughts.Add(thought);

            // Generate additional thoughts from memory associations
            var memoryThoughts = GenerateMemoryAssociatedThoughts(state, options);
            thoughts.AddRange(memoryThoughts);
        }

        // Rank thoughts by relevance
        var rankedThoughts = thoughts
            .OrderByDescending(t => t.Relevance)
            .Take(options.TopK * states.Length)
            .ToArray();

        return Task.FromResult(rankedThoughts);
    }

    /// <inheritdoc />
    public Task<string[]> RetrieveMemoriesAsync(
        SubjectiveState state,
        int topK = 5,
        CancellationToken ct = default)
    {
        lock (_memoryLock)
        {
            if (_memoryStore.Count == 0)
                return Task.FromResult(Array.Empty<string>());

            // Simple keyword-based retrieval for MVP
            var stateWords = GetStateKeywords(state);

            var scored = _memoryStore
                .Select(m => (memory: m, score: CalculateMemoryRelevance(m, stateWords, state)))
                .OrderByDescending(x => x.score)
                .Take(topK)
                .Select(x => x.memory.Content)
                .ToArray();

            return Task.FromResult(scored);
        }
    }

    /// <inheritdoc />
    public Task<float[]> EmbedStateAsync(
        SubjectiveState state,
        CancellationToken ct = default)
    {
        // Simple embedding based on state characteristics (MVP)
        // In production, this would use a proper embedding model
        var embedding = new float[64];

        // Encode valence, arousal, intensity
        embedding[0] = (float)state.Valence;
        embedding[1] = (float)state.Arousal;
        embedding[2] = (float)state.Intensity;

        // Encode kind
        var kindIndex = state.Kind switch
        {
            SubjectiveState.StateKinds.Mood => 3,
            SubjectiveState.StateKinds.Emotion => 4,
            SubjectiveState.StateKinds.Thought => 5,
            SubjectiveState.StateKinds.Feeling => 6,
            SubjectiveState.StateKinds.Intention => 7,
            SubjectiveState.StateKinds.Curiosity => 8,
            SubjectiveState.StateKinds.Creativity => 9,
            _ => 10
        };
        embedding[kindIndex] = 1.0f;

        // Encode semantic tags
        if (state.SemanticTags != null)
        {
            for (int i = 0; i < Math.Min(state.SemanticTags.Count, 20); i++)
            {
                var tagHash = state.SemanticTags[i].GetHashCode();
                embedding[15 + (Math.Abs(tagHash) % 49)] += 0.5f;
            }
        }

        // Hash narrative into remaining dimensions
        if (!string.IsNullOrEmpty(state.Narrative))
        {
            var words = state.Narrative.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words.Take(20))
            {
                var hash = Math.Abs(word.ToLowerInvariant().GetHashCode());
                embedding[15 + (hash % 49)] += 0.1f;
            }
        }

        // Normalize
        var norm = (float)Math.Sqrt(embedding.Sum(x => x * x));
        if (norm > 0)
        {
            for (int i = 0; i < embedding.Length; i++)
                embedding[i] /= norm;
        }

        return Task.FromResult(embedding);
    }

    /// <inheritdoc />
    public Task StoreThoughtAsync(Thought thought, CancellationToken ct = default)
    {
        lock (_memoryLock)
        {
            _memoryStore.Add(new StoredThought(
                thought.Id,
                thought.Content,
                thought.SourceStateId,
                thought.Relevance,
                DateTimeOffset.UtcNow
            ));

            // Keep memory bounded (simple LRU)
            const int maxMemory = 1000;
            if (_memoryStore.Count > maxMemory)
            {
                _memoryStore.RemoveRange(0, _memoryStore.Count - maxMemory);
            }
        }

        return Task.CompletedTask;
    }

    private Thought GenerateThought(SubjectiveState state, SemanticOptions options)
    {
        // Generate thought content based on state
        var content = GenerateThoughtContent(state);

        // Calculate relevance based on intensity and arousal
        var relevance = (state.Intensity + state.Arousal) / 2;

        return new Thought(
            Id: Guid.NewGuid(),
            Content: content,
            Relevance: relevance,
            SourceStateId: state.Id,
            MemoryHits: null
        );
    }

    private string GenerateThoughtContent(SubjectiveState state)
    {
        // Template-based thought generation for MVP
        var templates = new Dictionary<string, string[]>
        {
            [SubjectiveState.StateKinds.Mood] = new[]
            {
                "The current mood suggests a {valence} disposition.",
                "A {intensity} sense of {description} pervades.",
                "The atmosphere feels {description}."
            },
            [SubjectiveState.StateKinds.Emotion] = new[]
            {
                "An emotional response emerges: {description}.",
                "Feeling a {intensity} {description}.",
                "The emotional landscape shows {description}."
            },
            [SubjectiveState.StateKinds.Thought] = new[]
            {
                "A thought crystallizes: {description}.",
                "Considering the implications of {description}.",
                "Reflecting on {description}."
            },
            [SubjectiveState.StateKinds.Curiosity] = new[]
            {
                "Curiosity arises about {description}.",
                "An interest in exploring {description}.",
                "Questions emerge regarding {description}."
            },
            [SubjectiveState.StateKinds.Creativity] = new[]
            {
                "A creative spark: {description}.",
                "Imagining possibilities for {description}.",
                "A novel idea forms around {description}."
            }
        };

        var kindTemplates = templates.GetValueOrDefault(state.Kind, templates[SubjectiveState.StateKinds.Thought]);
        var template = kindTemplates[new Random().Next(kindTemplates.Length)];

        var valenceWord = state.Valence > 0.3 ? "positive" : state.Valence < -0.3 ? "negative" : "neutral";
        var intensityWord = state.Intensity > 0.6 ? "strong" : state.Intensity > 0.3 ? "moderate" : "subtle";
        var description = state.SemanticTags?.FirstOrDefault() ?? "the current state";

        return template
            .Replace("{valence}", valenceWord)
            .Replace("{intensity}", intensityWord)
            .Replace("{description}", description);
    }

    private List<Thought> GenerateMemoryAssociatedThoughts(SubjectiveState state, SemanticOptions options)
    {
        var thoughts = new List<Thought>();

        // Retrieve related memories
        var memories = RetrieveMemoriesAsync(state, options.TopK).Result;

        foreach (var memory in memories)
        {
            // Generate association thought
            var relevance = options.RelevanceWeight * 0.8; // Slightly lower than direct thoughts

            thoughts.Add(new Thought(
                Id: Guid.NewGuid(),
                Content: $"This connects to previous experience: {memory}",
                Relevance: relevance,
                SourceStateId: state.Id,
                MemoryHits: new[] { memory }
            ));
        }

        return thoughts;
    }

    private HashSet<string> GetStateKeywords(SubjectiveState state)
    {
        var keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Add semantic tags
        if (state.SemanticTags != null)
        {
            foreach (var tag in state.SemanticTags)
                keywords.Add(tag);
        }

        // Add kind
        keywords.Add(state.Kind);

        // Extract words from narrative
        if (!string.IsNullOrEmpty(state.Narrative))
        {
            var words = state.Narrative
                .Split(new[] { ' ', ',', '.', ':', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 3);
            foreach (var word in words)
                keywords.Add(word.ToLowerInvariant());
        }

        return keywords;
    }

    private double CalculateMemoryRelevance(StoredThought memory, HashSet<string> stateKeywords, SubjectiveState state)
    {
        var score = 0.0;

        // Keyword overlap
        var memoryWords = memory.Content
            .Split(new[] { ' ', ',', '.', ':', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(w => w.ToLowerInvariant())
            .ToHashSet();

        var overlap = stateKeywords.Intersect(memoryWords).Count();
        score += overlap * 0.2;

        // Recency bonus
        var age = DateTimeOffset.UtcNow - memory.StoredAt;
        var recencyBonus = Math.Exp(-age.TotalHours / 24);
        score += recencyBonus * 0.3;

        // Base relevance from original thought
        score += memory.OriginalRelevance * 0.3;

        return Math.Min(1.0, score);
    }

    private record StoredThought(
        Guid Id,
        string Content,
        Guid SourceStateId,
        double OriginalRelevance,
        DateTimeOffset StoredAt);
}
