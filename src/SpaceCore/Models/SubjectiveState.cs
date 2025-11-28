using System;
using System.Collections.Generic;

namespace SpaceCore.Models;

/// <summary>
/// Represents a human-like subjective state (mood, feeling, intensity).
/// </summary>
/// <param name="Id">Unique identifier for this state.</param>
/// <param name="Kind">The kind of state (e.g., "mood", "emotion", "thought").</param>
/// <param name="Valence">Emotional valence (-1.0 negative to +1.0 positive).</param>
/// <param name="Arousal">Arousal level (0.0 calm to 1.0 excited).</param>
/// <param name="Intensity">Intensity of the state (0.0 to 1.0).</param>
/// <param name="Narrative">Human-readable description of this state.</param>
/// <param name="Trace">JSON trace containing derivation information.</param>
/// <param name="SemanticTags">Semantic tags associated with this state.</param>
/// <param name="Meta">Additional metadata.</param>
public record SubjectiveState(
    Guid Id,
    string Kind,
    double Valence,
    double Arousal,
    double Intensity,
    string Narrative,
    string Trace,
    IList<string>? SemanticTags = null,
    IDictionary<string, object>? Meta = null)
{
    /// <summary>
    /// State kinds used in the system.
    /// </summary>
    public static class StateKinds
    {
        public const string Mood = "mood";
        public const string Emotion = "emotion";
        public const string Thought = "thought";
        public const string Feeling = "feeling";
        public const string Intention = "intention";
        public const string Curiosity = "curiosity";
        public const string Creativity = "creativity";
    }

    /// <summary>
    /// Calculates an overall "energy" score from valence, arousal, and intensity.
    /// </summary>
    public double Energy => (Arousal + Intensity) / 2;

    /// <summary>
    /// Calculates a positivity score (-1.0 to +1.0).
    /// </summary>
    public double Positivity => Valence;

    /// <summary>
    /// Determines if this is a high-energy state.
    /// </summary>
    public bool IsHighEnergy => Energy > 0.6;

    /// <summary>
    /// Determines if this is a positive state.
    /// </summary>
    public bool IsPositive => Valence > 0;

    /// <summary>
    /// Determines if this is a neutral state (close to zero valence).
    /// </summary>
    public bool IsNeutral => Math.Abs(Valence) < 0.2;
}
