using System.Threading;
using System.Threading.Tasks;
using SpaceCore.Models;

namespace SpaceCore.Interfaces;

/// <summary>
/// Maps emergent shapes to human-like subjective states.
/// Computes valence, arousal, intensity, and generates narratives.
/// </summary>
public interface IStateShaper
{
    /// <summary>
    /// Shapes emergent shapes into subjective states.
    /// </summary>
    /// <param name="shapes">Emergent shapes to transform.</param>
    /// <param name="options">Shaping options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Array of subjective states.</returns>
    Task<SubjectiveState[]> ShapeAsync(EmergentShape[] shapes, ShaperOptions options, CancellationToken ct = default);

    /// <summary>
    /// Shapes a single emergent shape into a subjective state.
    /// </summary>
    /// <param name="shape">The shape to transform.</param>
    /// <param name="options">Shaping options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A subjective state.</returns>
    Task<SubjectiveState> ShapeSingleAsync(EmergentShape shape, ShaperOptions options, CancellationToken ct = default);

    /// <summary>
    /// Generates a narrative for a subjective state using templates or LLM.
    /// </summary>
    /// <param name="state">The state to narrate.</param>
    /// <param name="context">Additional context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Updated state with narrative.</returns>
    Task<SubjectiveState> GenerateNarrativeAsync(SubjectiveState state, ContextState? context = null, CancellationToken ct = default);
}
