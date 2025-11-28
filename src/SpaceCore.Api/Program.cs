using SpaceCore.DependencyInjection;
using SpaceCore.Interfaces;
using SpaceCore.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "SpaceCore API", Version = "v1", Description = "SpaceOS Consciousness Engine API" });
});

// Add SpaceCore services
builder.Services.AddSpaceCore();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// === SpaceCore API Endpoints ===

// Health check
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTimeOffset.UtcNow }))
    .WithName("HealthCheck")
    .WithTags("System")
    .WithOpenApi();

// Process a percept through the consciousness pipeline
app.MapPost("/api/spacecore/percepts", async (
    PerceptRequest request,
    IConsciousnessAxisOrchestrator orchestrator,
    CancellationToken ct) =>
{
    var percept = new Percept(
        Source: request.Source ?? Percept.PerceptSources.Api,
        Type: request.Type ?? Percept.PerceptTypes.Text,
        Content: request.Content,
        Timestamp: DateTimeOffset.UtcNow,
        Meta: request.Meta
    );

    var context = new AgentContext(
        SessionId: request.SessionId ?? Guid.NewGuid(),
        UserId: request.UserId,
        Tags: request.Tags
    );

    var result = await orchestrator.ProcessPerceptAsync(percept, context, ct);

    return Results.Ok(new ProcessingResponse(
        SessionId: result.SessionId,
        States: result.States.Select(s => new StateResponse(s.Id, s.Kind, s.Valence, s.Arousal, s.Intensity, s.Narrative)).ToArray(),
        ThoughtCount: result.Thoughts.Length,
        SolutionCount: result.Solutions.Length,
        IPointPosition: result.IPointState.AxisPosition,
        ProcessingTimeMs: result.Trace.ProcessingTimeMs,
        DeterministicKey: result.Trace.DeterministicKey
    ));
})
.WithName("ProcessPercept")
.WithTags("Consciousness")
.WithOpenApi();

// Get current I-point state for a session
app.MapGet("/api/spacecore/state/{sessionId}", (
    Guid sessionId,
    IConsciousnessAxisOrchestrator orchestrator) =>
{
    var iPointState = orchestrator.GetCurrentIPointState(sessionId);
    if (iPointState == null)
    {
        return Results.NotFound(new { error = "Session not found" });
    }

    return Results.Ok(new IPointStateResponse(
        Id: iPointState.Id,
        AxisPosition: iPointState.AxisPosition,
        IsDeterministic: iPointState.IsDeterministic,
        IsEntropic: iPointState.IsEntropic,
        IsBalanced: iPointState.IsBalanced,
        DeterminismFactor: iPointState.DeterminismFactor,
        EntropyFactor: iPointState.EntropyFactor,
        Velocity: iPointState.Velocity,
        Phase: iPointState.Phase,
        PastStatesCount: iPointState.PastStates.Count
    ));
})
.WithName("GetState")
.WithTags("Consciousness")
.WithOpenApi();

// Get context state for a session
app.MapGet("/api/spacecore/context/{sessionId}", async (
    Guid sessionId,
    IConsciousnessAxisOrchestrator orchestrator,
    CancellationToken ct) =>
{
    var contextState = await orchestrator.GetContextStateAsync(sessionId, ct);
    if (contextState == null)
    {
        return Results.NotFound(new { error = "Session not found" });
    }

    return Results.Ok(new ContextStateResponse(
        SessionId: contextState.SessionId,
        ActiveTags: contextState.ActiveTags,
        EmbeddingDimension: contextState.Dimensionality,
        RecentPerceptCount: contextState.RecentPercepts?.Count ?? 0
    ));
})
.WithName("GetContext")
.WithTags("Consciousness")
.WithOpenApi();

// Start continuous consciousness flow
app.MapPost("/api/spacecore/flow/start/{sessionId}", async (
    Guid sessionId,
    FlowRequest? request,
    IConsciousnessAxisOrchestrator orchestrator,
    CancellationToken ct) =>
{
    var context = new AgentContext(
        SessionId: sessionId,
        UserId: request?.UserId,
        Tags: request?.Tags
    );

    await orchestrator.StartFlowAsync(sessionId, context, ct);

    return Results.Ok(new { sessionId, status = "started", message = "Consciousness flow started" });
})
.WithName("StartFlow")
.WithTags("Consciousness")
.WithOpenApi();

// Stop consciousness flow
app.MapPost("/api/spacecore/flow/stop/{sessionId}", async (
    Guid sessionId,
    IConsciousnessAxisOrchestrator orchestrator) =>
{
    await orchestrator.StopFlowAsync(sessionId);
    return Results.Ok(new { sessionId, status = "stopped", message = "Consciousness flow stopped" });
})
.WithName("StopFlow")
.WithTags("Consciousness")
.WithOpenApi();

// Generate entropy sample
app.MapGet("/api/spacecore/entropy/sample", (
    IEntropyGenerator entropyGenerator,
    int? size,
    bool? useSeed) =>
{
    var options = new EntropyOptions(
        ChunkSize: size ?? 256,
        UseSeed: useSeed ?? false
    );

    var chunk = entropyGenerator.GenerateChunk(Guid.NewGuid(), options);

    return Results.Ok(new EntropyResponse(
        Id: chunk.Id,
        Size: chunk.Bytes.Length,
        NoiseHash: chunk.NoiseHash,
        EntropyBits: chunk.EntropyBits,
        Timestamp: chunk.Timestamp
    ));
})
.WithName("GenerateEntropy")
.WithTags("Entropy")
.WithOpenApi();

// Get entropy quality estimate
app.MapGet("/api/spacecore/entropy/quality", (IEntropyGenerator entropyGenerator) =>
{
    var quality = entropyGenerator.EstimateEntropyQuality();
    return Results.Ok(new { quality, status = quality > 0.95 ? "excellent" : quality > 0.8 ? "good" : "acceptable" });
})
.WithName("EntropyQuality")
.WithTags("Entropy")
.WithOpenApi();

// Validate content against Spirit model
app.MapPost("/api/spacecore/safety/validate", async (
    SafetyValidationRequest request,
    ISpiritModel spiritModel,
    CancellationToken ct) =>
{
    var thought = new Thought(
        Id: Guid.NewGuid(),
        Content: request.Content,
        Relevance: 1.0,
        SourceStateId: Guid.Empty
    );

    var result = await spiritModel.ValidateThoughtAsync(thought, ct);

    return Results.Ok(new SafetyValidationResponse(
        IsValid: result.IsValid,
        SafetyScore: result.SafetyScore,
        ShouldSandbox: result.ShouldSandbox,
        Violations: result.Violations?.ToArray()
    ));
})
.WithName("ValidateSafety")
.WithTags("Safety")
.WithOpenApi();

// Get system info
app.MapGet("/api/spacecore/info", (ISpiritModel spiritModel, IMultiDimensionProjector projector) =>
{
    return Results.Ok(new SystemInfoResponse(
        Version: "1.0.0",
        ProjectorVersion: projector.Version,
        PolicyVersion: spiritModel.PolicyVersion,
        Components: new[]
        {
            "EntropyGenerator", "CrossDimensionProjector", "MultiDimensionProjector",
            "MultiDimensionalDowner", "EmergentHook", "EmergentProcessor",
            "StateShaper", "SemanticProcessor", "StateSolver",
            "ReflectionImpulser", "SpiritModel", "ConsciousnessAxisOrchestrator"
        }
    ));
})
.WithName("SystemInfo")
.WithTags("System")
.WithOpenApi();

app.Run();

// Request/Response DTOs
record PerceptRequest(
    string Content,
    Guid? SessionId = null,
    string? Source = null,
    string? Type = null,
    string? UserId = null,
    IList<string>? Tags = null,
    IDictionary<string, object>? Meta = null
);

record FlowRequest(
    string? UserId = null,
    IList<string>? Tags = null
);

record SafetyValidationRequest(string Content);

record ProcessingResponse(
    Guid SessionId,
    StateResponse[] States,
    int ThoughtCount,
    int SolutionCount,
    double IPointPosition,
    long ProcessingTimeMs,
    string DeterministicKey
);

record StateResponse(
    Guid Id,
    string Kind,
    double Valence,
    double Arousal,
    double Intensity,
    string Narrative
);

record IPointStateResponse(
    Guid Id,
    double AxisPosition,
    bool IsDeterministic,
    bool IsEntropic,
    bool IsBalanced,
    double DeterminismFactor,
    double EntropyFactor,
    double Velocity,
    double Phase,
    int PastStatesCount
);

record ContextStateResponse(
    Guid SessionId,
    string[] ActiveTags,
    int EmbeddingDimension,
    int RecentPerceptCount
);

record EntropyResponse(
    Guid Id,
    int Size,
    string NoiseHash,
    double EntropyBits,
    DateTimeOffset Timestamp
);

record SafetyValidationResponse(
    bool IsValid,
    double SafetyScore,
    bool ShouldSandbox,
    string[]? Violations
);

record SystemInfoResponse(
    string Version,
    string ProjectorVersion,
    string PolicyVersion,
    string[] Components
);
