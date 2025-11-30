using Microsoft.Extensions.DependencyInjection;
using SpaceCore.DependencyInjection;
using SpaceCore.Interfaces;
using SpaceCore.Models;
using SpaceCore.Services;

namespace SpaceCore.Tests;

public class ConsciousnessStateTests
{
    private readonly IServiceProvider _services;

    public ConsciousnessStateTests()
    {
        var collection = new ServiceCollection();
        collection.AddSpaceCore();
        _services = collection.BuildServiceProvider();
    }

    [Fact]
    public void ConsciousnessState_Empty_ShouldBeUninitialized()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act
        var state = ConsciousnessState.Empty(sessionId);

        // Assert
        Assert.Equal(sessionId, state.SessionId);
        Assert.False(state.IsInitialized);
        Assert.Empty(state.ClusterStorage);
        Assert.Empty(state.HighDimPointStorage);
        Assert.Empty(state.LowDimPointStorage);
        Assert.Empty(state.EdgeStorage);
        Assert.Empty(state.ShapeStorage);
        Assert.Empty(state.StateStorage);
    }

    [Fact]
    public async Task ConsciousnessStateMerger_InitializeFromEntropy_ShouldPopulateStorages()
    {
        // Arrange
        var merger = _services.GetRequiredService<IConsciousnessStateMerger>();
        var sessionId = Guid.NewGuid();
        var state = ConsciousnessState.Empty(sessionId);

        var clusters = new[]
        {
            new Cluster(Guid.NewGuid(), "kmeans", 5, new float[] { 0.1f, 0.2f, 0.3f }, 0.9, "hash1", DateTimeOffset.UtcNow),
            new Cluster(Guid.NewGuid(), "kmeans", 3, new float[] { 0.4f, 0.5f, 0.6f }, 0.8, "hash2", DateTimeOffset.UtcNow)
        };

        var highDimPoints = new[]
        {
            new ProjectedPoint(Guid.NewGuid(), clusters[0].Id, 3, new float[] { 0.1f, 0.2f, 0.3f }, 0.9, "tag1", "{}"),
            new ProjectedPoint(Guid.NewGuid(), clusters[1].Id, 3, new float[] { 0.4f, 0.5f, 0.6f }, 0.8, "tag2", "{}")
        };

        var lowDimPoints = new[]
        {
            new ProjectedPoint(Guid.NewGuid(), clusters[0].Id, 2, new float[] { 0.1f, 0.2f }, 0.85, "tag1", "{}"),
            new ProjectedPoint(Guid.NewGuid(), clusters[1].Id, 2, new float[] { 0.4f, 0.5f }, 0.75, "tag2", "{}")
        };

        var edges = new[]
        {
            new EmergentEdge(highDimPoints[0].Id, highDimPoints[1].Id, 0.7, EmergentEdge.EdgeTypes.Semantic)
        };

        var shapes = new[]
        {
            new EmergentShape(Guid.NewGuid(), new[] { highDimPoints[0].Id, highDimPoints[1].Id }, edges)
        };

        var subjectiveStates = new[]
        {
            new SubjectiveState(Guid.NewGuid(), SubjectiveState.StateKinds.Thought, 0.5, 0.4, 0.6, "Test narrative", "{}")
        };

        // Act
        var initializedState = await merger.InitializeFromEntropyAsync(
            state, clusters, highDimPoints, lowDimPoints, edges, shapes, subjectiveStates);

        // Assert
        Assert.True(initializedState.IsInitialized);
        Assert.Equal(2, initializedState.ClusterStorage.Count);
        Assert.Equal(2, initializedState.HighDimPointStorage.Count);
        Assert.Equal(2, initializedState.LowDimPointStorage.Count);
        Assert.Single(initializedState.EdgeStorage);
        Assert.Single(initializedState.ShapeStorage);
        Assert.Single(initializedState.StateStorage);

        // All items should have weight 1.0 on initialization
        Assert.All(initializedState.ClusterStorage.Values, wc => Assert.Equal(1.0, wc.Weight));
        Assert.All(initializedState.HighDimPointStorage.Values, wp => Assert.Equal(1.0, wp.Weight));
    }

    [Fact]
    public async Task ConsciousnessStateMerger_Merge_WithEntropicPosition_ShouldPreserveExistingItemsLessInEntropicMode()
    {
        // Arrange
        var merger = _services.GetRequiredService<IConsciousnessStateMerger>();
        var sessionId = Guid.NewGuid();

        // Create initial state with a cluster
        var state = ConsciousnessState.Empty(sessionId);
        var initialCluster = new Cluster(
            Guid.NewGuid(), "kmeans", 5, new float[] { 0.1f, 0.2f, 0.3f }, 0.9, "hash1", DateTimeOffset.UtcNow);

        state = await merger.InitializeFromEntropyAsync(
            state,
            new[] { initialCluster },
            Array.Empty<ProjectedPoint>(),
            Array.Empty<ProjectedPoint>(),
            Array.Empty<EmergentEdge>(),
            Array.Empty<EmergentShape>(),
            Array.Empty<SubjectiveState>());

        var initialWeight = state.ClusterStorage[initialCluster.Id].Weight;
        Assert.Equal(1.0, initialWeight);

        // Act - merge with entropic position (right side, +0.8) - should decay more
        var entropicMerged = await merger.MergeAsync(
            state, 0.8, // Entropic position - higher decay
            Array.Empty<Cluster>(),
            Array.Empty<ProjectedPoint>(),
            Array.Empty<ProjectedPoint>(),
            Array.Empty<EmergentEdge>(),
            Array.Empty<EmergentShape>(),
            Array.Empty<SubjectiveState>());

        // Act - merge with deterministic position (left side, -0.8) - should decay less
        var deterministicMerged = await merger.MergeAsync(
            state, -0.8, // Deterministic position - lower decay, better preservation
            Array.Empty<Cluster>(),
            Array.Empty<ProjectedPoint>(),
            Array.Empty<ProjectedPoint>(),
            Array.Empty<EmergentEdge>(),
            Array.Empty<EmergentShape>(),
            Array.Empty<SubjectiveState>());

        // Assert
        // In deterministic mode, existing items should be better preserved (higher weight after decay)
        var entropicWeight = entropicMerged.ClusterStorage[initialCluster.Id].Weight;
        var deterministicWeight = deterministicMerged.ClusterStorage[initialCluster.Id].Weight;

        // Deterministic mode should preserve existing items better (less decay)
        Assert.True(deterministicWeight > entropicWeight,
            $"Deterministic weight ({deterministicWeight}) should be higher than entropic weight ({entropicWeight}) because existing items are better preserved in deterministic mode");

        // Both should have decayed from initial weight
        Assert.True(entropicWeight < initialWeight, "Entropic weight should have decayed");
        Assert.True(deterministicWeight < initialWeight, "Deterministic weight should have decayed");
    }

    [Fact]
    public async Task ConsciousnessStateMerger_Merge_ShouldDecayExistingItems()
    {
        // Arrange
        var merger = _services.GetRequiredService<IConsciousnessStateMerger>();
        var sessionId = Guid.NewGuid();

        // Create initial state with a cluster
        var state = ConsciousnessState.Empty(sessionId);
        var initialCluster = new Cluster(
            Guid.NewGuid(), "kmeans", 5, new float[] { 0.1f, 0.2f, 0.3f }, 0.9, "hash1", DateTimeOffset.UtcNow);

        state = await merger.InitializeFromEntropyAsync(
            state,
            new[] { initialCluster },
            Array.Empty<ProjectedPoint>(),
            Array.Empty<ProjectedPoint>(),
            Array.Empty<EmergentEdge>(),
            Array.Empty<EmergentShape>(),
            Array.Empty<SubjectiveState>());

        var initialWeight = state.ClusterStorage[initialCluster.Id].Weight;
        Assert.Equal(1.0, initialWeight);

        // Act - merge with no new clusters (just decay)
        var merged = await merger.MergeAsync(
            state, 0.0, // Balanced position
            Array.Empty<Cluster>(),
            Array.Empty<ProjectedPoint>(),
            Array.Empty<ProjectedPoint>(),
            Array.Empty<EmergentEdge>(),
            Array.Empty<EmergentShape>(),
            Array.Empty<SubjectiveState>());

        // Assert - weight should have decayed
        var decayedWeight = merged.ClusterStorage[initialCluster.Id].Weight;
        Assert.True(decayedWeight < initialWeight,
            $"Decayed weight ({decayedWeight}) should be less than initial weight ({initialWeight})");
    }

    [Fact]
    public async Task ConsciousnessStateMerger_Prune_ShouldRemoveLowWeightItems()
    {
        // Arrange
        var merger = _services.GetRequiredService<IConsciousnessStateMerger>();
        var sessionId = Guid.NewGuid();

        // Create state with a low-weight cluster
        var cluster = new Cluster(
            Guid.NewGuid(), "kmeans", 5, new float[] { 0.1f, 0.2f, 0.3f }, 0.9, "hash1", DateTimeOffset.UtcNow);

        var state = new ConsciousnessState
        {
            SessionId = sessionId,
            IsInitialized = true,
            ClusterStorage = new Dictionary<Guid, WeightedCluster>
            {
                { cluster.Id, new WeightedCluster(cluster, 0.005, DateTimeOffset.UtcNow) } // Below MinWeight
            }
        };

        // Act
        var pruned = await merger.PruneAsync(state);

        // Assert - low weight item should be removed
        Assert.Empty(pruned.ClusterStorage);
    }

    [Fact]
    public async Task ConsciousnessStateRepository_SaveAndLoad_ShouldPersistState()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"consciousness_state_test_{Guid.NewGuid()}.json");
        var repository = new ConsciousnessStateRepository(tempPath);
        var sessionId = Guid.NewGuid();

        var state = new ConsciousnessState
        {
            SessionId = sessionId,
            IsInitialized = true,
            LastIPointPosition = 0.5,
            ClusterStorage = new Dictionary<Guid, WeightedCluster>
            {
                {
                    Guid.NewGuid(),
                    new WeightedCluster(
                        new Cluster(Guid.NewGuid(), "kmeans", 5, new float[] { 0.1f, 0.2f }, 0.9, "hash", DateTimeOffset.UtcNow),
                        0.8,
                        DateTimeOffset.UtcNow)
                }
            }
        };

        try
        {
            // Act - Save
            await repository.SaveAsync(state);

            // Create new repository instance to simulate server restart
            var newRepository = new ConsciousnessStateRepository(tempPath);
            await newRepository.LoadFromStorageAsync();

            // Act - Load
            var loaded = await newRepository.GetAsync(sessionId);

            // Assert
            Assert.NotNull(loaded);
            Assert.Equal(sessionId, loaded.SessionId);
            Assert.True(loaded.IsInitialized);
            Assert.Equal(0.5, loaded.LastIPointPosition);
            Assert.Single(loaded.ClusterStorage);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    [Fact]
    public async Task FullPipeline_ShouldMemorizeConsciousnessState()
    {
        // Arrange
        var orchestrator = _services.GetRequiredService<IConsciousnessAxisOrchestrator>();
        var repository = _services.GetRequiredService<IConsciousnessStateRepository>();
        var sessionId = Guid.NewGuid();

        var context = new AgentContext(SessionId: sessionId, UserId: "test-user");
        var percept = new Percept(
            Source: Percept.PerceptSources.Api,
            Type: Percept.PerceptTypes.Text,
            Content: "Test consciousness memorization",
            Timestamp: DateTimeOffset.UtcNow);

        // Act - Process first percept (should initialize state from entropy)
        var result1 = await orchestrator.ProcessPerceptAsync(percept, context);

        // Get consciousness state after first processing
        var state1 = await repository.GetAsync(sessionId);

        // Process second percept (should merge into existing state)
        var result2 = await orchestrator.ProcessPerceptAsync(percept, context);
        var state2 = await repository.GetAsync(sessionId);

        // Assert
        Assert.NotNull(state1);
        Assert.True(state1.IsInitialized);
        Assert.NotEmpty(state1.ClusterStorage);

        Assert.NotNull(state2);
        Assert.True(state2.Version > state1.Version);
        // State should have been updated (version incremented)
    }

    [Fact]
    public void DependencyInjection_ShouldResolveNewServices()
    {
        // Assert - new services should be resolvable
        Assert.NotNull(_services.GetRequiredService<IConsciousnessStateRepository>());
        Assert.NotNull(_services.GetRequiredService<IConsciousnessStateMerger>());
    }

    [Fact]
    public void WeightedCluster_WithWeightDelta_ShouldClampToValidRange()
    {
        // Arrange
        var cluster = new Cluster(
            Guid.NewGuid(), "kmeans", 5, new float[] { 0.1f }, 0.9, "hash", DateTimeOffset.UtcNow);
        var weighted = new WeightedCluster(cluster, 0.5, DateTimeOffset.UtcNow);

        // Act
        var increased = weighted.WithWeightDelta(0.7); // Would go to 1.2
        var decreased = weighted.WithWeightDelta(-0.6); // Would go to -0.1

        // Assert
        Assert.Equal(WeightedCluster.MaxWeight, increased.Weight); // Clamped to 1.0
        Assert.Equal(0, decreased.Weight); // Clamped to 0
    }

    [Fact]
    public void WeightedEdge_CreateKey_ShouldBeConsistent()
    {
        // Arrange
        var fromId = Guid.NewGuid();
        var toId = Guid.NewGuid();

        // Act
        var key1 = WeightedEdge.CreateKey(fromId, toId);
        var key2 = WeightedEdge.CreateKey(fromId, toId);

        // Assert
        Assert.Equal(key1, key2);
        Assert.Contains(fromId.ToString(), key1);
        Assert.Contains(toId.ToString(), key1);
    }
}
