using System;
using System.Collections.Generic;

namespace SpaceCore.Models;

/// <summary>
/// Represents a perception input - something the system perceives from the environment.
/// </summary>
/// <param name="Source">The source of the perception (e.g., "mic", "keyboard", "sensor").</param>
/// <param name="Type">The type of perception (e.g., "utterance", "text", "image").</param>
/// <param name="Content">The content of the perception.</param>
/// <param name="Timestamp">When this perception was received.</param>
/// <param name="Meta">Additional metadata.</param>
public record Percept(
    string Source,
    string Type,
    string Content,
    DateTimeOffset Timestamp,
    IDictionary<string, object>? Meta = null)
{
    /// <summary>
    /// Percept types used in the system.
    /// </summary>
    public static class PerceptTypes
    {
        public const string Utterance = "utterance";
        public const string Text = "text";
        public const string Image = "image";
        public const string Sensor = "sensor";
        public const string Action = "action";
        public const string Feedback = "feedback";
    }

    /// <summary>
    /// Percept sources used in the system.
    /// </summary>
    public static class PerceptSources
    {
        public const string Microphone = "mic";
        public const string Keyboard = "keyboard";
        public const string Camera = "camera";
        public const string Api = "api";
        public const string Internal = "internal";
    }
}

/// <summary>
/// Represents the contextual state for processing.
/// </summary>
/// <param name="SessionId">Session identifier.</param>
/// <param name="ActiveTags">Currently active semantic tags.</param>
/// <param name="Embedding">The context embedding vector.</param>
/// <param name="RecentPercepts">Recent percepts in this session.</param>
/// <param name="Meta">Additional metadata.</param>
public record ContextState(
    Guid SessionId,
    string[] ActiveTags,
    float[] Embedding,
    IList<Percept>? RecentPercepts = null,
    IDictionary<string, object>? Meta = null)
{
    /// <summary>
    /// Gets the embedding dimensionality.
    /// </summary>
    public int Dimensionality => Embedding.Length;

    /// <summary>
    /// Creates an empty context state.
    /// </summary>
    public static ContextState Empty(Guid sessionId, int embeddingDimension = 128) =>
        new(sessionId, Array.Empty<string>(), new float[embeddingDimension]);
}

/// <summary>
/// Represents a projected state (output of the projector).
/// </summary>
/// <param name="Id">Unique identifier.</param>
/// <param name="Vector">The projected vector.</param>
/// <param name="SemanticTags">Semantic tags.</param>
/// <param name="Narrative">Human-readable narrative.</param>
/// <param name="Entropy">Entropy level of this projection.</param>
/// <param name="CreatedAt">Creation timestamp.</param>
/// <param name="DeterministicKey">Key for reproducibility.</param>
/// <param name="Scores">Score breakdown.</param>
public record ProjectedState(
    string Id,
    float[] Vector,
    string[] SemanticTags,
    string Narrative,
    double Entropy,
    DateTimeOffset CreatedAt,
    string? DeterministicKey = null,
    ProjectedStateScores? Scores = null);

/// <summary>
/// Score breakdown for a projected state.
/// </summary>
/// <param name="Predictability">Predictability score.</param>
/// <param name="Novelty">Novelty score.</param>
/// <param name="Safety">Safety score.</param>
public record ProjectedStateScores(
    double Predictability,
    double Novelty,
    double Safety);
