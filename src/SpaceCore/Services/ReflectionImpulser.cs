using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using SpaceCore.Interfaces;
using SpaceCore.Models;

namespace SpaceCore.Services;

/// <summary>
/// Generates pulses that move the I-point along the consciousness axis.
/// Implements waveform-based pulsing with state-dependent modulation.
/// </summary>
public class ReflectionImpulser : IReflectionImpulser
{
    private double _time;
    private readonly object _timeLock = new();

    /// <inheritdoc />
    public IPointState Pulse(IPointState current, SubjectiveState[] envStates, PulseOptions options)
    {
        // Calculate pulse direction based on environmental states
        var direction = CalculatePulseDirection(current, envStates);

        // Calculate pulse magnitude based on waveform
        var time = GetAndIncrementTime();
        var magnitude = CalculatePulseMagnitude(current, options, time);

        // Apply pulse
        var delta = direction * magnitude;
        var newPosition = current.AxisPosition + delta;

        // Calculate new velocity (momentum)
        var newVelocity = current.Velocity * 0.9 + delta * 0.1; // Smoothed momentum

        // Update phase
        var newPhase = (current.Phase + options.Frequency * 0.01) % (2 * Math.PI);

        // Create trace
        var trace = CreateTrace(current, direction, magnitude, options);

        var newState = new IPointState(
            Id: Guid.NewGuid(),
            AxisPosition: Math.Clamp(newPosition, IPointState.MinPosition, IPointState.MaxPosition),
            CurrentPointId: current.CurrentPointId,
            PastStates: current.PastStates,
            Trace: trace,
            Velocity: newVelocity,
            Phase: newPhase
        );

        return newState;
    }

    /// <inheritdoc />
    public double CalculatePulseDirection(IPointState current, SubjectiveState[] envStates)
    {
        if (envStates.Length == 0)
            return 0;

        // Calculate weighted direction based on states
        double totalWeight = 0;
        double weightedDirection = 0;

        foreach (var state in envStates)
        {
            var weight = state.Intensity;
            totalWeight += weight;

            // Positive valence + high arousal -> toward entropy (right)
            // Negative valence + low arousal -> toward determinism (left)
            var stateDirection = (state.Valence * 0.5 + state.Arousal * 0.5);
            weightedDirection += stateDirection * weight;
        }

        if (totalWeight > 0)
        {
            weightedDirection /= totalWeight;
        }

        // Add momentum influence
        weightedDirection += current.Velocity * 0.3;

        // Add position-based restoring force (tendency to return to center)
        var centeringForce = -current.AxisPosition * 0.1;
        weightedDirection += centeringForce;

        return Math.Clamp(weightedDirection, -1.0, 1.0);
    }

    /// <inheritdoc />
    public double CalculatePulseMagnitude(IPointState current, PulseOptions options, double time)
    {
        var baseAmplitude = options.Amplitude;

        // Apply waveform
        double waveformValue = options.Waveform switch
        {
            "sine" => Math.Sin(time * options.Frequency * 2 * Math.PI),
            "exponential" => Math.Exp(-((time % 1) - 0.5) * ((time % 1) - 0.5) * 8) * 2 - 1,
            "sawtooth" => ((time * options.Frequency) % 1) * 2 - 1,
            "square" => Math.Sin(time * options.Frequency * 2 * Math.PI) > 0 ? 1 : -1,
            _ => Math.Sin(time * options.Frequency * 2 * Math.PI)
        };

        // Modulate amplitude based on position (softer near edges)
        var edgeDistance = 1.0 - Math.Abs(current.AxisPosition);
        var edgeModulation = Math.Sqrt(edgeDistance);

        var magnitude = baseAmplitude * Math.Abs(waveformValue) * edgeModulation;

        return magnitude;
    }

    /// <inheritdoc />
    public IPointState Reset(Guid sessionId)
    {
        var trace = JsonSerializer.Serialize(new
        {
            type = "reset",
            sessionId,
            timestamp = DateTimeOffset.UtcNow
        });

        return new IPointState(
            Id: Guid.NewGuid(),
            AxisPosition: 0, // Neutral center position
            CurrentPointId: null,
            PastStates: new List<Guid>(),
            Trace: trace,
            Velocity: 0,
            Phase: 0
        );
    }

    private double GetAndIncrementTime()
    {
        lock (_timeLock)
        {
            _time += 0.01;
            return _time;
        }
    }

    private string CreateTrace(IPointState current, double direction, double magnitude, PulseOptions options)
    {
        var trace = new
        {
            previousPosition = current.AxisPosition,
            direction,
            magnitude,
            velocity = current.Velocity,
            phase = current.Phase,
            options = new
            {
                amplitude = options.Amplitude,
                frequency = options.Frequency,
                waveform = options.Waveform
            },
            timestamp = DateTimeOffset.UtcNow
        };
        return JsonSerializer.Serialize(trace);
    }
}
