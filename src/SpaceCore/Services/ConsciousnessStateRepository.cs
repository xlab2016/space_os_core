using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using SpaceCore.Interfaces;
using SpaceCore.Models;

namespace SpaceCore.Services;

/// <summary>
/// File-based repository for consciousness state persistence.
/// Stores state in consciousness_state.json for recovery across server restarts.
/// </summary>
public class ConsciousnessStateRepository : IConsciousnessStateRepository
{
    private readonly ConcurrentDictionary<Guid, ConsciousnessState> _states = new();
    private readonly string _storagePath;
    private readonly SemaphoreSlim _fileLock = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Default file name for consciousness state storage.
    /// </summary>
    public const string DefaultFileName = "consciousness_state.json";

    /// <summary>
    /// Creates a new repository with the default storage path.
    /// </summary>
    public ConsciousnessStateRepository()
        : this(Path.Combine(AppContext.BaseDirectory, DefaultFileName))
    {
    }

    /// <summary>
    /// Creates a new repository with a custom storage path.
    /// </summary>
    /// <param name="storagePath">Path to the consciousness_state.json file.</param>
    public ConsciousnessStateRepository(string storagePath)
    {
        _storagePath = storagePath;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    /// <inheritdoc />
    public Task<ConsciousnessState?> GetAsync(Guid sessionId, CancellationToken ct = default)
    {
        _states.TryGetValue(sessionId, out var state);
        return Task.FromResult(state);
    }

    /// <inheritdoc />
    public async Task SaveAsync(ConsciousnessState state, CancellationToken ct = default)
    {
        _states[state.SessionId] = state.WithUpdate();

        // Persist to file (debounce in production, but immediate for correctness)
        await FlushToStorageAsync(ct);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid sessionId, CancellationToken ct = default)
    {
        _states.TryRemove(sessionId, out _);
        await FlushToStorageAsync(ct);
    }

    /// <inheritdoc />
    public Task<Guid[]> GetAllSessionIdsAsync(CancellationToken ct = default)
    {
        return Task.FromResult(_states.Keys.ToArray());
    }

    /// <inheritdoc />
    public async Task LoadFromStorageAsync(CancellationToken ct = default)
    {
        await _fileLock.WaitAsync(ct);
        try
        {
            if (!File.Exists(_storagePath))
            {
                return;
            }

            var json = await File.ReadAllTextAsync(_storagePath, ct);
            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            var storage = JsonSerializer.Deserialize<ConsciousnessStateStorage>(json, _jsonOptions);
            if (storage?.States == null)
            {
                return;
            }

            foreach (var state in storage.States)
            {
                _states[state.SessionId] = state;
            }
        }
        catch (JsonException)
        {
            // File is corrupted, start fresh but keep backup
            if (File.Exists(_storagePath))
            {
                var backupPath = $"{_storagePath}.{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.backup";
                File.Move(_storagePath, backupPath);
            }
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task FlushToStorageAsync(CancellationToken ct = default)
    {
        await _fileLock.WaitAsync(ct);
        try
        {
            var storage = new ConsciousnessStateStorage
            {
                States = _states.Values.ToArray(),
                SavedAt = DateTimeOffset.UtcNow,
                Version = "1.0"
            };

            var json = JsonSerializer.Serialize(storage, _jsonOptions);

            // Ensure directory exists
            var directory = Path.GetDirectoryName(_storagePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Write directly to the target file (simpler and more reliable for tests)
            // Use a unique temp file name to avoid conflicts in parallel tests
            var tempPath = $"{_storagePath}.{Guid.NewGuid()}.tmp";
            await File.WriteAllTextAsync(tempPath, json, ct);

            // Atomic move (on most file systems)
            try
            {
                File.Move(tempPath, _storagePath, overwrite: true);
            }
            catch (IOException)
            {
                // Fallback: if move fails, try direct write
                await File.WriteAllTextAsync(_storagePath, json, ct);
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// Internal storage format for JSON serialization.
    /// </summary>
    private record ConsciousnessStateStorage
    {
        public ConsciousnessState[] States { get; init; } = Array.Empty<ConsciousnessState>();
        public DateTimeOffset SavedAt { get; init; }
        public string Version { get; init; } = "1.0";
    }
}
