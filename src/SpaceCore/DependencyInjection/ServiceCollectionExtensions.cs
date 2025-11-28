using Microsoft.Extensions.DependencyInjection;
using SpaceCore.Interfaces;
using SpaceCore.Services;

namespace SpaceCore.DependencyInjection;

/// <summary>
/// Extension methods for registering SpaceCore services with DI.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all SpaceCore services to the service collection.
    /// </summary>
    public static IServiceCollection AddSpaceCore(this IServiceCollection services)
    {
        // Register core services as singletons (thread-safe implementations)
        services.AddSingleton<IEntropyGenerator, EntropyGenerator>();
        services.AddSingleton<ICrossDimensionProjector, CrossDimensionProjector>();
        services.AddSingleton<IMultiDimensionProjector, MultiDimensionProjector>();
        services.AddSingleton<IMultiDimensionalDowner, MultiDimensionalDowner>();
        services.AddSingleton<IEmergentHook, EmergentHook>();
        services.AddSingleton<IEmergentProcessor, EmergentProcessor>();
        services.AddSingleton<IStateShaper, StateShaper>();
        services.AddSingleton<ISemanticProcessor, SemanticProcessor>();
        services.AddSingleton<IStateSolver, StateSolver>();
        services.AddSingleton<IReflectionImpulser, ReflectionImpulser>();
        services.AddSingleton<ISpiritModel, SpiritModel>();

        // Register orchestrator as singleton
        services.AddSingleton<IConsciousnessAxisOrchestrator, ConsciousnessAxisOrchestrator>();

        return services;
    }

    /// <summary>
    /// Adds SpaceCore services with custom implementations.
    /// </summary>
    public static IServiceCollection AddSpaceCore<TSpirit>(this IServiceCollection services)
        where TSpirit : class, ISpiritModel
    {
        // Register core services
        services.AddSingleton<IEntropyGenerator, EntropyGenerator>();
        services.AddSingleton<ICrossDimensionProjector, CrossDimensionProjector>();
        services.AddSingleton<IMultiDimensionProjector, MultiDimensionProjector>();
        services.AddSingleton<IMultiDimensionalDowner, MultiDimensionalDowner>();
        services.AddSingleton<IEmergentHook, EmergentHook>();
        services.AddSingleton<IEmergentProcessor, EmergentProcessor>();
        services.AddSingleton<IStateShaper, StateShaper>();
        services.AddSingleton<ISemanticProcessor, SemanticProcessor>();
        services.AddSingleton<IStateSolver, StateSolver>();
        services.AddSingleton<IReflectionImpulser, ReflectionImpulser>();

        // Register custom Spirit model
        services.AddSingleton<ISpiritModel, TSpirit>();

        // Register orchestrator
        services.AddSingleton<IConsciousnessAxisOrchestrator, ConsciousnessAxisOrchestrator>();

        return services;
    }
}
