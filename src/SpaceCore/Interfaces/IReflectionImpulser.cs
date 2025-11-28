using SpaceCore.Models;

namespace SpaceCore.Interfaces;

/// <summary>
/// Generates pulses that move the I-point along the consciousness axis.
/// Controls the balance between determinism and entropy.
/// </summary>
public interface IReflectionImpulser
{
    /// <summary>
    /// Generates a pulse that updates the I-point state.
    /// </summary>
    /// <param name="current">Current I-point state.</param>
    /// <param name="envStates">Environmental subjective states.</param>
    /// <param name="options">Pulse options.</param>
    /// <returns>Updated I-point state.</returns>
    IPointState Pulse(IPointState current, SubjectiveState[] envStates, PulseOptions options);

    /// <summary>
    /// Calculates the pulse direction based on current conditions.
    /// </summary>
    /// <param name="current">Current I-point state.</param>
    /// <param name="envStates">Environmental subjective states.</param>
    /// <returns>Direction value (-1.0 to +1.0).</returns>
    double CalculatePulseDirection(IPointState current, SubjectiveState[] envStates);

    /// <summary>
    /// Calculates the pulse magnitude based on options and state.
    /// </summary>
    /// <param name="current">Current I-point state.</param>
    /// <param name="options">Pulse options.</param>
    /// <param name="time">Current time value for waveform calculation.</param>
    /// <returns>Magnitude value (0.0 to amplitude).</returns>
    double CalculatePulseMagnitude(IPointState current, PulseOptions options, double time);

    /// <summary>
    /// Resets the I-point to a neutral position.
    /// </summary>
    /// <param name="sessionId">Session identifier.</param>
    /// <returns>New I-point state at neutral position.</returns>
    IPointState Reset(System.Guid sessionId);
}
