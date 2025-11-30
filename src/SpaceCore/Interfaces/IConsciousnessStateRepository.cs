using System;
using System.Threading;
using System.Threading.Tasks;
using SpaceCore.Models;

namespace SpaceCore.Interfaces;

/// <summary>
/// Repository for persisting and retrieving consciousness state.
/// Handles file-based persistence to consciousness_state.json.
/// </summary>
public interface IConsciousnessStateRepository
{
    /// <summary>
    /// Gets the consciousness state for a session.
    /// Returns null if no state exists for the session.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The consciousness state or null.</returns>
    Task<ConsciousnessState?> GetAsync(Guid sessionId, CancellationToken ct = default);

    /// <summary>
    /// Saves the consciousness state.
    /// Creates new or updates existing state.
    /// </summary>
    /// <param name="state">The consciousness state to save.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SaveAsync(ConsciousnessState state, CancellationToken ct = default);

    /// <summary>
    /// Deletes the consciousness state for a session.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="ct">Cancellation token.</param>
    Task DeleteAsync(Guid sessionId, CancellationToken ct = default);

    /// <summary>
    /// Gets all session IDs that have persisted state.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Array of session IDs.</returns>
    Task<Guid[]> GetAllSessionIdsAsync(CancellationToken ct = default);

    /// <summary>
    /// Loads all consciousness states from persistent storage.
    /// Called on server startup to restore state.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task LoadFromStorageAsync(CancellationToken ct = default);

    /// <summary>
    /// Flushes all in-memory state to persistent storage.
    /// Called on server shutdown or periodically for durability.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task FlushToStorageAsync(CancellationToken ct = default);
}
