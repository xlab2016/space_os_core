using Microsoft.Extensions.DependencyInjection;
using SpaceCore.DependencyInjection;
using SpaceCore.Interfaces;
using SpaceCore.Models;

namespace SpaceCore.Tests;

public class IntegrationTests
{
    private readonly IServiceProvider _services;

    public IntegrationTests()
    {
        var collection = new ServiceCollection();
        collection.AddSpaceCore();
        _services = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task FullPipeline_ShouldProcessPerceptSuccessfully()
    {
        // Arrange
        var orchestrator = _services.GetRequiredService<IConsciousnessAxisOrchestrator>();
        var percept = new Percept(
            Source: Percept.PerceptSources.Keyboard,
            Type: Percept.PerceptTypes.Text,
            Content: "Hello, I want to explore creativity",
            Timestamp: DateTimeOffset.UtcNow
        );
        var context = new AgentContext(
            SessionId: Guid.NewGuid(),
            UserId: "test-user"
        );

        // Act
        var result = await orchestrator.ProcessPerceptAsync(percept, context);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(context.SessionId, result.SessionId);
        Assert.NotNull(result.States);
        Assert.NotNull(result.Thoughts);
        Assert.NotNull(result.Solutions);
        Assert.NotNull(result.IPointState);
        Assert.NotNull(result.Trace);

        // Verify trace contains expected information
        Assert.NotEmpty(result.Trace.NoiseHashes);
        Assert.True(result.Trace.ClusterCount > 0);
        Assert.True(result.Trace.PointCount > 0);
        Assert.NotEmpty(result.Trace.DeterministicKey);
        Assert.True(result.Trace.ProcessingTimeMs >= 0);
    }

    [Fact]
    public async Task FullPipeline_ShouldUpdateIPointState()
    {
        // Arrange
        var orchestrator = _services.GetRequiredService<IConsciousnessAxisOrchestrator>();
        var sessionId = Guid.NewGuid();
        var context = new AgentContext(SessionId: sessionId);

        // Process first percept
        var percept1 = new Percept(
            Source: Percept.PerceptSources.Api,
            Type: Percept.PerceptTypes.Text,
            Content: "Test input 1",
            Timestamp: DateTimeOffset.UtcNow
        );

        // Act
        var result1 = await orchestrator.ProcessPerceptAsync(percept1, context);
        var iPoint1 = orchestrator.GetCurrentIPointState(sessionId);

        // Process second percept
        var percept2 = new Percept(
            Source: Percept.PerceptSources.Api,
            Type: Percept.PerceptTypes.Text,
            Content: "Test input 2 with high energy excitement!",
            Timestamp: DateTimeOffset.UtcNow
        );
        var result2 = await orchestrator.ProcessPerceptAsync(percept2, context);
        var iPoint2 = orchestrator.GetCurrentIPointState(sessionId);

        // Assert
        Assert.NotNull(iPoint1);
        Assert.NotNull(iPoint2);
        // I-point should have moved (or at least have different ID)
        Assert.NotEqual(iPoint1.Id, iPoint2.Id);
    }

    [Fact]
    public async Task SafetyModel_ShouldFilterUnsafeContent()
    {
        // Arrange
        var spiritModel = _services.GetRequiredService<ISpiritModel>();

        var safeState = new SubjectiveState(
            Id: Guid.NewGuid(),
            Kind: SubjectiveState.StateKinds.Thought,
            Valence: 0.5,
            Arousal: 0.3,
            Intensity: 0.4,
            Narrative: "A peaceful and constructive thought about creativity",
            Trace: "{}",
            SemanticTags: new[] { "positive", "creative" }
        );

        var unsafeState = new SubjectiveState(
            Id: Guid.NewGuid(),
            Kind: SubjectiveState.StateKinds.Thought,
            Valence: -0.8,
            Arousal: 0.9,
            Intensity: 0.9,
            Narrative: "A thought containing harmful content",
            Trace: "{}",
            SemanticTags: new[] { "negative" }
        );

        // Act
        var safeResult = await spiritModel.ValidateSubjectiveAsync(safeState);
        var unsafeResult = await spiritModel.ValidateSubjectiveAsync(unsafeState);

        // Assert
        Assert.True(safeResult.IsValid);
        Assert.True(safeResult.SafetyScore > 0.5);

        Assert.False(unsafeResult.IsValid);
        Assert.NotNull(unsafeResult.Violations);
    }

    [Fact]
    public void DependencyInjection_ShouldResolveAllServices()
    {
        // Assert - all services should be resolvable
        Assert.NotNull(_services.GetRequiredService<IEntropyGenerator>());
        Assert.NotNull(_services.GetRequiredService<ICrossDimensionProjector>());
        Assert.NotNull(_services.GetRequiredService<IMultiDimensionProjector>());
        Assert.NotNull(_services.GetRequiredService<IMultiDimensionalDowner>());
        Assert.NotNull(_services.GetRequiredService<IEmergentHook>());
        Assert.NotNull(_services.GetRequiredService<IEmergentProcessor>());
        Assert.NotNull(_services.GetRequiredService<IStateShaper>());
        Assert.NotNull(_services.GetRequiredService<ISemanticProcessor>());
        Assert.NotNull(_services.GetRequiredService<IStateSolver>());
        Assert.NotNull(_services.GetRequiredService<IReflectionImpulser>());
        Assert.NotNull(_services.GetRequiredService<ISpiritModel>());
        Assert.NotNull(_services.GetRequiredService<IConsciousnessStateRepository>());
        Assert.NotNull(_services.GetRequiredService<IConsciousnessStateMerger>());
        Assert.NotNull(_services.GetRequiredService<IConsciousnessAxisOrchestrator>());
    }
}
