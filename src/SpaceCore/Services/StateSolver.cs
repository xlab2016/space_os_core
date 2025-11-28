using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SpaceCore.Interfaces;
using SpaceCore.Models;

namespace SpaceCore.Services;

/// <summary>
/// Solves problems by proposing actions based on subjective states.
/// Uses simple heuristic-based decision making for MVP.
/// </summary>
public class StateSolver : IStateSolver
{
    /// <inheritdoc />
    public Task<Solution[]> SolveAsync(
        SubjectiveState[] states,
        ProblemContext problem,
        SolverOptions options,
        CancellationToken ct = default)
    {
        if (states.Length == 0)
            return Task.FromResult(Array.Empty<Solution>());

        var solutions = new List<Solution>();

        // Generate candidate solutions based on states
        var dominantState = states.OrderByDescending(s => s.Intensity).First();

        // Generate primary solution based on dominant state
        var primarySolution = GenerateSolution(dominantState, problem, isPrimary: true);
        solutions.Add(primarySolution);

        // Generate alternative solutions with exploration
        var explorationCount = Math.Min(options.MaxIterations / 10, 5);
        var random = new Random();

        for (int i = 0; i < explorationCount; i++)
        {
            if (random.NextDouble() < options.ExplorationRate)
            {
                // Explore: pick random state
                var randomState = states[random.Next(states.Length)];
                var explorationSolution = GenerateSolution(randomState, problem, isPrimary: false);
                solutions.Add(explorationSolution);
            }
            else
            {
                // Exploit: use high-intensity states
                var highIntensityStates = states.Where(s => s.Intensity > 0.5).ToArray();
                if (highIntensityStates.Length > 0)
                {
                    var state = highIntensityStates[random.Next(highIntensityStates.Length)];
                    var exploitSolution = GenerateSolution(state, problem, isPrimary: false);
                    solutions.Add(exploitSolution);
                }
            }
        }

        // Apply constraints
        if (problem.Constraints?.Any() == true)
        {
            solutions = solutions
                .Where(s => MeetsConstraints(s, problem.Constraints))
                .ToList();
        }

        // Rank solutions
        var rankedSolutions = solutions
            .OrderByDescending(s => s.Confidence)
            .ToArray();

        return Task.FromResult(rankedSolutions);
    }

    /// <inheritdoc />
    public Task<double> EvaluateAsync(
        Solution solution,
        ProblemContext problem,
        CancellationToken ct = default)
    {
        // Simple evaluation heuristic
        var score = solution.Confidence;

        // Bonus for goal alignment
        if (!string.IsNullOrEmpty(problem.Goal))
        {
            var goalWords = problem.Goal.ToLowerInvariant().Split(' ').ToHashSet();
            var actionWords = solution.Action.ToLowerInvariant().Split(' ').ToHashSet();
            var overlap = goalWords.Intersect(actionWords).Count();
            score += overlap * 0.1;
        }

        // Penalty for constraint violations (already filtered, but double-check)
        if (problem.Constraints?.Any() == true)
        {
            if (!MeetsConstraints(solution, problem.Constraints))
            {
                score *= 0.5;
            }
        }

        return Task.FromResult(Math.Min(1.0, score));
    }

    /// <inheritdoc />
    public Solution SelectBest(Solution[] solutions, ProblemContext problem)
    {
        if (solutions.Length == 0)
            throw new ArgumentException("No solutions to select from");

        if (solutions.Length == 1)
            return solutions[0];

        // Select based on confidence and goal alignment
        return solutions
            .OrderByDescending(s => EvaluateAsync(s, problem).Result)
            .First();
    }

    private Solution GenerateSolution(SubjectiveState state, ProblemContext problem, bool isPrimary)
    {
        // Generate action based on state characteristics
        var action = GenerateAction(state, problem);

        // Calculate confidence
        var confidence = CalculateConfidence(state, problem, isPrimary);

        // Generate reasoning
        var reasoning = GenerateReasoning(state, problem, action);

        return new Solution(
            Id: Guid.NewGuid(),
            Action: action,
            Confidence: confidence,
            Reasoning: reasoning,
            RelatedStateIds: new List<Guid> { state.Id }
        );
    }

    private string GenerateAction(SubjectiveState state, ProblemContext problem)
    {
        // Action templates based on state kind
        var actionTemplates = new Dictionary<string, Func<SubjectiveState, ProblemContext, string>>
        {
            [SubjectiveState.StateKinds.Creativity] = (s, p) =>
                $"Generate creative options for: {p.Goal}",

            [SubjectiveState.StateKinds.Curiosity] = (s, p) =>
                $"Explore and gather more information about: {p.Goal}",

            [SubjectiveState.StateKinds.Thought] = (s, p) =>
                $"Analyze and reason about: {p.Goal}",

            [SubjectiveState.StateKinds.Emotion] = (s, p) =>
                s.Valence > 0
                    ? $"Proceed with enthusiasm towards: {p.Goal}"
                    : $"Approach cautiously: {p.Goal}",

            [SubjectiveState.StateKinds.Intention] = (s, p) =>
                $"Take direct action to achieve: {p.Goal}",

            [SubjectiveState.StateKinds.Mood] = (s, p) =>
                s.Arousal > 0.5
                    ? $"Engage actively with: {p.Goal}"
                    : $"Take time to consider: {p.Goal}",

            [SubjectiveState.StateKinds.Feeling] = (s, p) =>
                $"Respond intuitively to: {p.Goal}"
        };

        var template = actionTemplates.GetValueOrDefault(
            state.Kind,
            (s, p) => $"Consider: {p.Goal}");

        return template(state, problem);
    }

    private double CalculateConfidence(SubjectiveState state, ProblemContext problem, bool isPrimary)
    {
        var baseConfidence = isPrimary ? 0.7 : 0.5;

        // Adjust based on state characteristics
        baseConfidence += state.Intensity * 0.15;
        baseConfidence += Math.Abs(state.Valence) * 0.1;

        // Boost for matching context
        if (problem.Context != null)
        {
            var contextMatch = state.SemanticTags?
                .Any(t => problem.Context.ContainsKey(t)) == true;
            if (contextMatch) baseConfidence += 0.1;
        }

        return Math.Min(1.0, Math.Max(0.1, baseConfidence));
    }

    private string GenerateReasoning(SubjectiveState state, ProblemContext problem, string action)
    {
        var stateDescription = state.Narrative ?? $"a {state.Kind} state";
        var valenceContext = state.Valence > 0 ? "positive" : state.Valence < 0 ? "challenging" : "neutral";

        return $"Based on {stateDescription} with {valenceContext} valence " +
               $"and {(state.Arousal > 0.5 ? "high" : "moderate")} arousal, " +
               $"the recommended approach is: {action}. " +
               $"This aligns with the goal: {problem.Goal}";
    }

    private bool MeetsConstraints(Solution solution, IList<string> constraints)
    {
        var actionLower = solution.Action.ToLowerInvariant();

        foreach (var constraint in constraints)
        {
            var constraintLower = constraint.ToLowerInvariant();

            // Simple constraint checking
            if (constraintLower.StartsWith("not:") || constraintLower.StartsWith("avoid:"))
            {
                var forbidden = constraintLower.Substring(constraintLower.IndexOf(':') + 1).Trim();
                if (actionLower.Contains(forbidden))
                    return false;
            }
            else if (constraintLower.StartsWith("must:") || constraintLower.StartsWith("require:"))
            {
                var required = constraintLower.Substring(constraintLower.IndexOf(':') + 1).Trim();
                if (!actionLower.Contains(required))
                    return false;
            }
        }

        return true;
    }
}
