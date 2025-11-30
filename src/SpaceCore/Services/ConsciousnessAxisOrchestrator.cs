using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SpaceCore.Interfaces;
using SpaceCore.Models;

namespace SpaceCore.Services;

/// <summary>
/// Orchestrates the entire consciousness flow pipeline.
/// Coordinates all components from entropy generation to action execution.
/// Now includes consciousness state memorization across pipeline stages.
/// </summary>
public class ConsciousnessAxisOrchestrator : IConsciousnessAxisOrchestrator
{
    private readonly IEntropyGenerator _entropyGenerator;
    private readonly ICrossDimensionProjector _crossDimensionProjector;
    private readonly IMultiDimensionProjector _multiDimensionProjector;
    private readonly IMultiDimensionalDowner _dimensionalDowner;
    private readonly IEmergentHook _emergentHook;
    private readonly IEmergentProcessor _emergentProcessor;
    private readonly IStateShaper _stateShaper;
    private readonly ISemanticProcessor _semanticProcessor;
    private readonly IStateSolver _stateSolver;
    private readonly IReflectionImpulser _reflectionImpulser;
    private readonly ISpiritModel _spiritModel;
    private readonly IConsciousnessStateRepository _stateRepository;
    private readonly IConsciousnessStateMerger _stateMerger;

    private readonly ConcurrentDictionary<Guid, SessionState> _sessions = new();
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _flowCancellations = new();

    // Default dimensions for pipeline
    private const int InitialDimension = 128;
    private const int FinalDimension = 32;

    // Prune consciousness state every N processing cycles
    private const int PruneCycleInterval = 10;
    private int _processingCycleCount = 0;

    public ConsciousnessAxisOrchestrator(
        IEntropyGenerator entropyGenerator,
        ICrossDimensionProjector crossDimensionProjector,
        IMultiDimensionProjector multiDimensionProjector,
        IMultiDimensionalDowner dimensionalDowner,
        IEmergentHook emergentHook,
        IEmergentProcessor emergentProcessor,
        IStateShaper stateShaper,
        ISemanticProcessor semanticProcessor,
        IStateSolver stateSolver,
        IReflectionImpulser reflectionImpulser,
        ISpiritModel spiritModel,
        IConsciousnessStateRepository stateRepository,
        IConsciousnessStateMerger stateMerger)
    {
        _entropyGenerator = entropyGenerator;
        _crossDimensionProjector = crossDimensionProjector;
        _multiDimensionProjector = multiDimensionProjector;
        _dimensionalDowner = dimensionalDowner;
        _emergentHook = emergentHook;
        _emergentProcessor = emergentProcessor;
        _stateShaper = stateShaper;
        _semanticProcessor = semanticProcessor;
        _stateSolver = stateSolver;
        _reflectionImpulser = reflectionImpulser;
        _spiritModel = spiritModel;
        _stateRepository = stateRepository;
        _stateMerger = stateMerger;
    }

    /// <inheritdoc />
    public async Task<ConsciousnessProcessingResult> ProcessPerceptAsync(
        Percept percept,
        AgentContext context,
        CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var perceptId = Guid.NewGuid();
        var noiseHashes = new List<string>();

        // Get or create session state
        var session = _sessions.GetOrAdd(context.SessionId, _ => new SessionState
        {
            IPointState = _reflectionImpulser.Reset(context.SessionId),
            ContextState = ContextState.Empty(context.SessionId, InitialDimension)
        });

        // Get or load consciousness state for this session
        var consciousnessState = await _stateRepository.GetAsync(context.SessionId, ct)
            ?? ConsciousnessState.Empty(context.SessionId);

        // Step 1: Generate noise (entropy source)
        var entropyOptions = new EntropyOptions(
            ChunkSize: 512,
            UseSeed: false
        );
        var noiseChunks = _entropyGenerator.GenerateChunks(context.SessionId, 10, entropyOptions);
        noiseHashes.AddRange(noiseChunks.Select(c => c.NoiseHash));

        // Step 2: Cluster noise
        var clusterOptions = new ClusterOptions(Algorithm: "online-kmeans", K: 8);
        var clusters = await _crossDimensionProjector.ClusterAsync(noiseChunks, clusterOptions, ct);

        // Step 3: Project clusters to high-dimensional space
        var projectionOptions = new ProjectionOptions(
            Creativity: 0.5,
            Determinism: 0.9,
            Safety: 1.0,
            Candidates: 3
        );
        var highDimPoints = await _multiDimensionProjector.ProjectBatchAsync(
            clusters, InitialDimension, projectionOptions, ct);

        // Step 4: Reduce dimensionality
        var downerOptions = new DownerOptions(Method: "pca");
        var lowDimPoints = await _dimensionalDowner.DownProjectBatchAsync(
            highDimPoints, FinalDimension, downerOptions, ct);

        // Step 5: Build edges between points
        var linkOptions = new LinkOptions(
            SimilarityThreshold: 0.7,
            TemporalWindow: 5,
            MaxEdgesPerPoint: 5
        );
        var edges = await _emergentHook.LinkAsync(lowDimPoints, linkOptions, ct);

        // Step 6: Process graph to find emergent shapes
        var processorOptions = new ProcessorOptions(Steps: 3, ActivationThreshold: 0.4);
        var shapes = await _emergentProcessor.ProcessAsync(lowDimPoints, edges, processorOptions, ct);

        // Step 7: Shape into subjective states
        var shaperOptions = new ShaperOptions(UseNarrativeGeneration: true);
        var states = await _stateShaper.ShapeAsync(shapes, shaperOptions, ct);

        // Step 8: Validate states through Spirit model
        var validatedStates = new List<SubjectiveState>();
        foreach (var state in states)
        {
            var validation = await _spiritModel.ValidateSubjectiveAsync(state, ct);
            if (validation.IsValid)
            {
                validatedStates.Add(state);
            }
        }

        // === CONSCIOUSNESS STATE MEMORIZATION ===
        // After GenerateChunks, merge pipeline results into consciousness state
        // based on I-point position (entropy vs determinism influence)
        if (!consciousnessState.IsInitialized)
        {
            // Initialize consciousness state from entropy (first run)
            consciousnessState = await _stateMerger.InitializeFromEntropyAsync(
                consciousnessState,
                clusters,
                highDimPoints,
                lowDimPoints,
                edges,
                shapes,
                validatedStates.ToArray(),
                ct);
        }
        else
        {
            // Merge new results into existing state based on I-point position
            // Right (entropic): stronger entropy impact
            // Left (deterministic): weaker entropy but stronger Thought preservation
            consciousnessState = await _stateMerger.MergeAsync(
                consciousnessState,
                session.IPointState.AxisPosition,
                clusters,
                highDimPoints,
                lowDimPoints,
                edges,
                shapes,
                validatedStates.ToArray(),
                ct);
        }

        // Periodically prune low-weight items
        Interlocked.Increment(ref _processingCycleCount);
        if (_processingCycleCount % PruneCycleInterval == 0)
        {
            consciousnessState = await _stateMerger.PruneAsync(consciousnessState, ct);
        }

        // Persist consciousness state to file (consciousness_state.json)
        await _stateRepository.SaveAsync(consciousnessState, ct);

        // Step 9: Generate thoughts via semantic processor
        var semanticOptions = new SemanticOptions(TopK: 5);
        var thoughts = await _semanticProcessor.GenerateThoughtsAsync(
            validatedStates.ToArray(), semanticOptions, ct);

        // Validate thoughts
        var validatedThoughts = new List<Thought>();
        foreach (var thought in thoughts)
        {
            var validation = await _spiritModel.ValidateThoughtAsync(thought, ct);
            if (validation.IsValid)
            {
                validatedThoughts.Add(thought);
                await _semanticProcessor.StoreThoughtAsync(thought, ct);
            }
        }

        // Step 10: Solve for actions
        var problemContext = new ProblemContext(
            SessionId: context.SessionId,
            Goal: percept.Content,
            Context: context.Meta
        );
        var solverOptions = new SolverOptions(MaxIterations: 50, ExplorationRate: 0.2);
        var solutions = await _stateSolver.SolveAsync(
            validatedStates.ToArray(), problemContext, solverOptions, ct);

        // Step 11: Pulse the I-point based on resulting states
        var pulseOptions = new PulseOptions(
            Amplitude: 0.1,
            Frequency: 1.0,
            Waveform: "sine"
        );
        var newIPointState = _reflectionImpulser.Pulse(
            session.IPointState, validatedStates.ToArray(), pulseOptions);

        // Update session state
        session.IPointState = newIPointState;
        session.ContextState = UpdateContextState(session.ContextState, percept, validatedStates);

        // Create deterministic key for reproducibility
        var deterministicKey = ComputeDeterministicKey(noiseHashes, clusters, _multiDimensionProjector.Version);

        stopwatch.Stop();

        var trace = new ConsciousnessTrace(
            PerceptId: perceptId,
            NoiseHashes: noiseHashes.ToArray(),
            ClusterCount: clusters.Length,
            PointCount: lowDimPoints.Length,
            EdgeCount: edges.Length,
            ShapeCount: shapes.Length,
            DeterministicKey: deterministicKey,
            ProcessingTimeMs: stopwatch.ElapsedMilliseconds
        );

        return new ConsciousnessProcessingResult(
            SessionId: context.SessionId,
            States: validatedStates.ToArray(),
            Thoughts: validatedThoughts.ToArray(),
            Solutions: solutions,
            IPointState: newIPointState,
            Trace: trace
        );
    }

    /// <inheritdoc />
    public async Task StartFlowAsync(Guid sessionId, AgentContext context, CancellationToken ct = default)
    {
        // Create cancellation source for this flow
        var flowCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _flowCancellations[sessionId] = flowCts;

        // Load consciousness state from storage if it exists (server restart recovery)
        await _stateRepository.LoadFromStorageAsync(ct);
        var existingConsciousnessState = await _stateRepository.GetAsync(sessionId, ct);

        // Initialize session, restoring I-point position from consciousness state if available
        var initialIPointState = _reflectionImpulser.Reset(sessionId);
        if (existingConsciousnessState?.IsInitialized == true)
        {
            // Restore I-point position from last known state
            initialIPointState = initialIPointState.WithNewPosition(existingConsciousnessState.LastIPointPosition);
        }

        _sessions[sessionId] = new SessionState
        {
            IPointState = initialIPointState,
            ContextState = ContextState.Empty(sessionId, InitialDimension)
        };

        // Start continuous flow processing
        _ = Task.Run(async () =>
        {
            while (!flowCts.Token.IsCancellationRequested)
            {
                try
                {
                    // Process background consciousness
                    var backgroundPercept = new Percept(
                        Source: Percept.PerceptSources.Internal,
                        Type: Percept.PerceptTypes.Sensor,
                        Content: "background consciousness pulse",
                        Timestamp: DateTimeOffset.UtcNow
                    );

                    await ProcessPerceptAsync(backgroundPercept, context, flowCts.Token);

                    // Pulse at regular intervals
                    await Task.Delay(1000, flowCts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                    // Log error but continue flow
                    await Task.Delay(5000, flowCts.Token);
                }
            }
        }, flowCts.Token);

        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopFlowAsync(Guid sessionId)
    {
        if (_flowCancellations.TryRemove(sessionId, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public IPointState? GetCurrentIPointState(Guid sessionId)
    {
        return _sessions.TryGetValue(sessionId, out var session) ? session.IPointState : null;
    }

    /// <inheritdoc />
    public Task<ContextState?> GetContextStateAsync(Guid sessionId, CancellationToken ct = default)
    {
        var result = _sessions.TryGetValue(sessionId, out var session) ? session.ContextState : null;
        return Task.FromResult(result);
    }

    private ContextState UpdateContextState(ContextState current, Percept percept, IList<SubjectiveState> states)
    {
        // Update active tags from states
        var newTags = states
            .SelectMany(s => s.SemanticTags ?? Enumerable.Empty<string>())
            .Distinct()
            .Take(10)
            .ToArray();

        // Add recent percept
        var recentPercepts = current.RecentPercepts?.ToList() ?? new List<Percept>();
        recentPercepts.Add(percept);
        if (recentPercepts.Count > 10)
        {
            recentPercepts = recentPercepts.Skip(recentPercepts.Count - 10).ToList();
        }

        return current with
        {
            ActiveTags = newTags,
            RecentPercepts = recentPercepts
        };
    }

    private string ComputeDeterministicKey(
        IList<string> noiseHashes,
        Cluster[] clusters,
        string projectorVersion)
    {
        using var sha256 = SHA256.Create();
        var combined = new StringBuilder();

        foreach (var hash in noiseHashes)
            combined.Append(hash);

        foreach (var cluster in clusters)
            combined.Append(cluster.ClusterHash);

        combined.Append(projectorVersion);

        var bytes = Encoding.UTF8.GetBytes(combined.ToString());
        return Convert.ToHexString(sha256.ComputeHash(bytes));
    }

    private class SessionState
    {
        public IPointState IPointState { get; set; } = null!;
        public ContextState ContextState { get; set; } = null!;
    }
}
