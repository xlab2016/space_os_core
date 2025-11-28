using System.Threading;
using System.Threading.Tasks;
using SpaceCore.Models;

namespace SpaceCore.Interfaces;

/// <summary>
/// Solves problems by proposing actions based on subjective states and context.
/// Uses optimization or policy-based decision making.
/// </summary>
public interface IStateSolver
{
    /// <summary>
    /// Solves a problem given the current states.
    /// </summary>
    /// <param name="states">Current subjective states.</param>
    /// <param name="problem">The problem context.</param>
    /// <param name="options">Solver options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Array of proposed solutions.</returns>
    Task<Solution[]> SolveAsync(SubjectiveState[] states, ProblemContext problem, SolverOptions options, CancellationToken ct = default);

    /// <summary>
    /// Evaluates a solution's expected outcome.
    /// </summary>
    /// <param name="solution">The solution to evaluate.</param>
    /// <param name="problem">The problem context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Evaluation score (0.0 to 1.0).</returns>
    Task<double> EvaluateAsync(Solution solution, ProblemContext problem, CancellationToken ct = default);

    /// <summary>
    /// Selects the best solution from candidates.
    /// </summary>
    /// <param name="solutions">Candidate solutions.</param>
    /// <param name="problem">The problem context.</param>
    /// <returns>The selected solution.</returns>
    Solution SelectBest(Solution[] solutions, ProblemContext problem);
}
