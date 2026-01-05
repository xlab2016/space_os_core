# SpaceCore - SpaceOS Consciousness Engine

SpaceCore is the core module of SpaceOS that transforms random noise (entropy) into deterministic, controlled state transitions. It implements a consciousness-like processing pipeline that:

1. **Accepts perceptions and random noise** - Takes input perceptions and high-entropy random values
2. **Transforms noise into controlled states** - Converts chaos into structured, deterministic transitions
3. **Exports transitions to LLM** - Generates prompts and narratives for language model processing
4. **Ensures explainability and safety** - Full traceability, reproducibility, and SpiritModel validation

## Architecture

```
[ Perception Adapters ] -> [ConsciousnessFlow Orchestrator] ->
    -> [Projector] -> [State Engine / Transition Planner] -> [Action Executor]
                                   ↓
                            [KnowledgeBase]
                                   ↓
                       [LLM Adapter / RAG / Response]
```

### Core Components

- **EntropyGenerator** - CSPRNG-based random noise source with deterministic seeding
- **CrossDimensionProjector** - Online k-means clustering for noise → clusters
- **MultiDimensionProjector** - Random projection for clusters → N-dimensional vectors
- **MultiDimensionalDowner** - PCA-based dimension reduction
- **EmergentHook** - Graph edge builder (temporal, semantic, causal links)
- **EmergentProcessor** - Activation propagation and shape detection
- **StateShaper** - Maps emergent shapes to subjective states (valence, arousal, intensity)
- **SemanticProcessor** - Thought generation and memory retrieval
- **StateSolver** - Action/solution proposal based on states
- **ReflectionImpulser** - I-point movement along consciousness axis
- **SpiritModel** - Safety validation and content filtering

### ConsciousnessAxis

The I-point moves along the ConsciousnessAxis:
- **Left (-1.0)** - Deterministic region (predictable, controlled)
- **Center (0.0)** - Balanced region
- **Right (+1.0)** - Entropic region (creative, exploratory)

## Getting Started

### Prerequisites

- .NET 8.0 SDK

### Building

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Running the API

```bash
cd src/SpaceCore.Api
dotnet run
```

The API will be available at `https://localhost:5001` with Swagger UI at `/swagger`.

## API Endpoints

### Consciousness Processing

- `POST /api/spacecore/percepts` - Process a percept through the consciousness pipeline
- `GET /api/spacecore/state/{sessionId}` - Get current I-point state
- `GET /api/spacecore/context/{sessionId}` - Get context state
- `POST /api/spacecore/flow/start/{sessionId}` - Start continuous consciousness flow
- `POST /api/spacecore/flow/stop/{sessionId}` - Stop consciousness flow

### Entropy

- `GET /api/spacecore/entropy/sample` - Generate entropy sample
- `GET /api/spacecore/entropy/quality` - Get entropy quality estimate

### Safety

- `POST /api/spacecore/safety/validate` - Validate content against Spirit model

### System

- `GET /health` - Health check
- `GET /api/spacecore/info` - System information

## Example Usage

```csharp
// Register services
services.AddSpaceCore();

// Process a percept
var percept = new Percept(
    Source: "keyboard",
    Type: "text",
    Content: "I want an idea for a logo",
    Timestamp: DateTimeOffset.UtcNow
);

var context = new AgentContext(
    SessionId: Guid.NewGuid(),
    UserId: "user-123"
);

var result = await orchestrator.ProcessPerceptAsync(percept, context);

// Access results
Console.WriteLine($"States generated: {result.States.Length}");
Console.WriteLine($"Thoughts: {result.Thoughts.Length}");
Console.WriteLine($"Solutions: {result.Solutions.Length}");
Console.WriteLine($"I-Point position: {result.IPointState.AxisPosition}");
Console.WriteLine($"Deterministic key: {result.Trace.DeterministicKey}");
```

## Determinism and Reproducibility

Every processing run generates a `DeterministicKey` that can be used to reproduce the exact same output given:
- Same seed/noise
- Same context
- Same model version

The trace includes:
- `noiseHash` - Hash of entropy source
- `clusterHash` - Hash of clustering result
- `projectorVersion` - Version of projection model
- `contextHash` - Hash of context state

## Safety (SpiritModel)

The SpiritModel validates all outputs against safety policies:
- Content filtering for forbidden patterns
- Sandbox flagging for high-risk content
- Safety scoring (0.0 to 1.0)
- Policy versioning for audit

## Project Structure

```
├── src/
│   ├── SpaceCore/                 # Core library
│   │   ├── Models/                # Data models
│   │   ├── Interfaces/            # Service interfaces
│   │   ├── Services/              # Service implementations
│   │   └── DependencyInjection/   # DI extensions
│   └── SpaceCore.Api/             # Web API
├── tests/
│   └── SpaceCore.Tests/           # Unit and integration tests
└── SpaceCore.sln                  # Solution file
```

## License

[License information]
