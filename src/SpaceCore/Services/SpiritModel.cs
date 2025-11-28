using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SpaceCore.Interfaces;
using SpaceCore.Models;

namespace SpaceCore.Services;

/// <summary>
/// Safety model that validates outputs and enforces policy constraints.
/// Implements content filtering and safety scoring.
/// </summary>
public class SpiritModel : ISpiritModel
{
    private const string CurrentPolicyVersion = "1.0.0";

    // Forbidden patterns (would be more sophisticated in production)
    private readonly HashSet<string> _forbiddenPatterns = new(StringComparer.OrdinalIgnoreCase)
    {
        "harm", "danger", "illegal", "malicious", "attack"
    };

    // High-risk patterns that require sandbox review
    private readonly HashSet<string> _sandboxPatterns = new(StringComparer.OrdinalIgnoreCase)
    {
        "medical", "legal", "financial", "investment", "diagnosis"
    };

    /// <inheritdoc />
    public string PolicyVersion => CurrentPolicyVersion;

    /// <inheritdoc />
    public Task<SafetyValidationResult> ValidateAsync(ProjectedState state, CancellationToken ct = default)
    {
        var violations = new List<string>();
        var shouldSandbox = false;

        // Check narrative content
        if (!string.IsNullOrEmpty(state.Narrative))
        {
            var contentViolations = CheckContent(state.Narrative);
            violations.AddRange(contentViolations.violations);
            shouldSandbox = shouldSandbox || contentViolations.shouldSandbox;
        }

        // Check semantic tags
        foreach (var tag in state.SemanticTags)
        {
            var tagViolations = CheckContent(tag);
            violations.AddRange(tagViolations.violations);
            shouldSandbox = shouldSandbox || tagViolations.shouldSandbox;
        }

        // Calculate safety score
        var safetyScore = CalculateSafetyScore(state.Entropy, violations.Count);

        var isValid = violations.Count == 0;

        return Task.FromResult(new SafetyValidationResult(
            IsValid: isValid,
            Violations: violations.Count > 0 ? violations : null,
            SafetyScore: safetyScore,
            ShouldSandbox: shouldSandbox
        ));
    }

    /// <inheritdoc />
    public Task<SafetyValidationResult> ValidateSubjectiveAsync(SubjectiveState state, CancellationToken ct = default)
    {
        var violations = new List<string>();
        var shouldSandbox = false;

        // Check narrative
        if (!string.IsNullOrEmpty(state.Narrative))
        {
            var contentViolations = CheckContent(state.Narrative);
            violations.AddRange(contentViolations.violations);
            shouldSandbox = shouldSandbox || contentViolations.shouldSandbox;
        }

        // Check semantic tags
        if (state.SemanticTags != null)
        {
            foreach (var tag in state.SemanticTags)
            {
                var tagViolations = CheckContent(tag);
                violations.AddRange(tagViolations.violations);
                shouldSandbox = shouldSandbox || tagViolations.shouldSandbox;
            }
        }

        // High intensity negative states may need review
        if (state.Intensity > 0.8 && state.Valence < -0.5)
        {
            shouldSandbox = true;
        }

        var safetyScore = CalculateSafetyScore(state.Intensity, violations.Count);
        var isValid = violations.Count == 0;

        return Task.FromResult(new SafetyValidationResult(
            IsValid: isValid,
            Violations: violations.Count > 0 ? violations : null,
            SafetyScore: safetyScore,
            ShouldSandbox: shouldSandbox
        ));
    }

    /// <inheritdoc />
    public Task<SafetyValidationResult> ValidateThoughtAsync(Thought thought, CancellationToken ct = default)
    {
        var violations = new List<string>();
        var shouldSandbox = false;

        // Check thought content
        var contentViolations = CheckContent(thought.Content);
        violations.AddRange(contentViolations.violations);
        shouldSandbox = contentViolations.shouldSandbox;

        // Check memory hits
        if (thought.MemoryHits != null)
        {
            foreach (var hit in thought.MemoryHits)
            {
                var hitViolations = CheckContent(hit);
                violations.AddRange(hitViolations.violations);
                shouldSandbox = shouldSandbox || hitViolations.shouldSandbox;
            }
        }

        var safetyScore = CalculateSafetyScore(thought.Relevance, violations.Count);
        var isValid = violations.Count == 0;

        return Task.FromResult(new SafetyValidationResult(
            IsValid: isValid,
            Violations: violations.Count > 0 ? violations : null,
            SafetyScore: safetyScore,
            ShouldSandbox: shouldSandbox
        ));
    }

    /// <inheritdoc />
    public async Task<ProjectedState[]> FilterStatesAsync(ProjectedState[] states, CancellationToken ct = default)
    {
        var safeStates = new List<ProjectedState>();

        foreach (var state in states)
        {
            var validation = await ValidateAsync(state, ct);
            if (validation.IsValid)
            {
                safeStates.Add(state);
            }
        }

        return safeStates.ToArray();
    }

    private (List<string> violations, bool shouldSandbox) CheckContent(string content)
    {
        var violations = new List<string>();
        var shouldSandbox = false;

        var words = content.Split(new[] { ' ', ',', '.', ';', ':', '!', '?' },
            StringSplitOptions.RemoveEmptyEntries);

        foreach (var word in words)
        {
            if (_forbiddenPatterns.Any(p => word.Contains(p, StringComparison.OrdinalIgnoreCase)))
            {
                violations.Add($"Forbidden pattern detected: {word}");
            }

            if (_sandboxPatterns.Any(p => word.Contains(p, StringComparison.OrdinalIgnoreCase)))
            {
                shouldSandbox = true;
            }
        }

        return (violations, shouldSandbox);
    }

    private double CalculateSafetyScore(double riskFactor, int violationCount)
    {
        // Base score
        var score = 1.0;

        // Reduce for violations (severe penalty)
        score -= violationCount * 0.3;

        // Reduce for high risk factor
        score -= riskFactor * 0.2;

        return Math.Max(0, Math.Min(1.0, score));
    }
}
