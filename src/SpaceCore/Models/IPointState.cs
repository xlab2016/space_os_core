using System;
using System.Collections.Generic;

namespace SpaceCore.Models;

/// <summary>
/// Represents the state of the "I" point - the consciousness position on the axis.
/// </summary>
/// <param name="Id">Unique identifier for this I-point state.</param>
/// <param name="AxisPosition">Position on the consciousness axis (-1.0 deterministic to +1.0 entropic).</param>
/// <param name="CurrentPointId">The current projected point this I-point is associated with.</param>
/// <param name="PastStates">List of past state IDs (experience history).</param>
/// <param name="Trace">JSON trace for reproducibility.</param>
/// <param name="Velocity">Current velocity of movement along the axis.</param>
/// <param name="Phase">Current phase in the reflection cycle.</param>
public record IPointState(
    Guid Id,
    double AxisPosition,
    Guid? CurrentPointId,
    IList<Guid> PastStates,
    string Trace,
    double Velocity = 0.0,
    double Phase = 0.0)
{
    /// <summary>
    /// Axis position bounds.
    /// </summary>
    public const double MinPosition = -1.0;
    public const double MaxPosition = 1.0;

    /// <summary>
    /// Determines if the I-point is in the deterministic region (left side of axis).
    /// </summary>
    public bool IsDeterministic => AxisPosition < -0.3;

    /// <summary>
    /// Determines if the I-point is in the entropic region (right side of axis).
    /// </summary>
    public bool IsEntropic => AxisPosition > 0.3;

    /// <summary>
    /// Determines if the I-point is in the balanced/central region.
    /// </summary>
    public bool IsBalanced => Math.Abs(AxisPosition) <= 0.3;

    /// <summary>
    /// Gets the determinism factor based on axis position (0.0 to 1.0).
    /// Higher when deterministic, lower when entropic.
    /// </summary>
    public double DeterminismFactor => (1.0 - AxisPosition) / 2.0;

    /// <summary>
    /// Gets the entropy factor based on axis position (0.0 to 1.0).
    /// Higher when entropic, lower when deterministic.
    /// </summary>
    public double EntropyFactor => (1.0 + AxisPosition) / 2.0;

    /// <summary>
    /// Clamps the axis position to valid bounds.
    /// </summary>
    public IPointState ClampPosition() =>
        this with { AxisPosition = Math.Clamp(AxisPosition, MinPosition, MaxPosition) };

    /// <summary>
    /// Creates a new I-point state with updated position.
    /// </summary>
    public IPointState WithNewPosition(double newPosition, Guid? newPointId = null) =>
        this with
        {
            AxisPosition = Math.Clamp(newPosition, MinPosition, MaxPosition),
            CurrentPointId = newPointId ?? CurrentPointId
        };

    /// <summary>
    /// Creates a new I-point state with the current state added to history.
    /// </summary>
    public IPointState WithExperience(Guid experienceStateId) =>
        this with { PastStates = new List<Guid>(PastStates) { experienceStateId } };
}
