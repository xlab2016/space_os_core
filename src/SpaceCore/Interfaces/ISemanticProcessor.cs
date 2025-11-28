using System.Threading;
using System.Threading.Tasks;
using SpaceCore.Models;

namespace SpaceCore.Interfaces;

/// <summary>
/// Processes semantic content using memory and LLM integration.
/// Generates thoughts and retrieves relevant memory hits.
/// </summary>
public interface ISemanticProcessor
{
    /// <summary>
    /// Generates thoughts based on subjective states.
    /// </summary>
    /// <param name="states">Input subjective states.</param>
    /// <param name="options">Processing options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Array of generated thoughts.</returns>
    Task<Thought[]> GenerateThoughtsAsync(SubjectiveState[] states, SemanticOptions options, CancellationToken ct = default);

    /// <summary>
    /// Retrieves memory hits relevant to a state.
    /// </summary>
    /// <param name="state">The state to find memories for.</param>
    /// <param name="topK">Number of top hits to return.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Array of memory hit strings.</returns>
    Task<string[]> RetrieveMemoriesAsync(SubjectiveState state, int topK = 5, CancellationToken ct = default);

    /// <summary>
    /// Embeds a state for semantic search.
    /// </summary>
    /// <param name="state">The state to embed.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Embedding vector.</returns>
    Task<float[]> EmbedStateAsync(SubjectiveState state, CancellationToken ct = default);

    /// <summary>
    /// Stores a thought in the semantic memory.
    /// </summary>
    /// <param name="thought">The thought to store.</param>
    /// <param name="ct">Cancellation token.</param>
    Task StoreThoughtAsync(Thought thought, CancellationToken ct = default);
}
