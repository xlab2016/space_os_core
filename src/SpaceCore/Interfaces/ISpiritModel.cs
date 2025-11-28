using System.Threading;
using System.Threading.Tasks;
using SpaceCore.Models;

namespace SpaceCore.Interfaces;

/// <summary>
/// Safety model that validates outputs and enforces policy constraints.
/// Rejects forbidden outputs and logs violations.
/// </summary>
public interface ISpiritModel
{
    /// <summary>
    /// Validates a projected state against safety policies.
    /// </summary>
    /// <param name="state">The state to validate.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Validation result.</returns>
    Task<SafetyValidationResult> ValidateAsync(ProjectedState state, CancellationToken ct = default);

    /// <summary>
    /// Validates a subjective state against safety policies.
    /// </summary>
    /// <param name="state">The state to validate.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Validation result.</returns>
    Task<SafetyValidationResult> ValidateSubjectiveAsync(SubjectiveState state, CancellationToken ct = default);

    /// <summary>
    /// Validates a thought against safety policies.
    /// </summary>
    /// <param name="thought">The thought to validate.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Validation result.</returns>
    Task<SafetyValidationResult> ValidateThoughtAsync(Thought thought, CancellationToken ct = default);

    /// <summary>
    /// Filters a batch of states, removing unsafe ones.
    /// </summary>
    /// <param name="states">States to filter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Safe states.</returns>
    Task<ProjectedState[]> FilterStatesAsync(ProjectedState[] states, CancellationToken ct = default);

    /// <summary>
    /// Gets the current policy version.
    /// </summary>
    string PolicyVersion { get; }
}

/// <summary>
/// Result of safety validation.
/// </summary>
/// <param name="IsValid">Whether the input passed validation.</param>
/// <param name="Violations">List of policy violations if any.</param>
/// <param name="SafetyScore">Overall safety score (0.0 to 1.0).</param>
/// <param name="ShouldSandbox">Whether the input should go to sandbox for review.</param>
public record SafetyValidationResult(
    bool IsValid,
    System.Collections.Generic.IList<string>? Violations,
    double SafetyScore,
    bool ShouldSandbox = false);
