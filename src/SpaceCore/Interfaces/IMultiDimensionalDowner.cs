using System.Threading;
using System.Threading.Tasks;
using SpaceCore.Models;

namespace SpaceCore.Interfaces;

/// <summary>
/// Reduces dimensionality of projected points between layers.
/// Uses PCA, autoencoders, or other dimension reduction methods.
/// </summary>
public interface IMultiDimensionalDowner
{
    /// <summary>
    /// Reduces the dimensionality of a projected point.
    /// </summary>
    /// <param name="input">The point to reduce.</param>
    /// <param name="targetDim">Target dimensionality.</param>
    /// <param name="options">Reduction options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Reduced projected point.</returns>
    Task<ProjectedPoint> DownProjectAsync(ProjectedPoint input, int targetDim, DownerOptions options, CancellationToken ct = default);

    /// <summary>
    /// Reduces the dimensionality of multiple points.
    /// </summary>
    /// <param name="inputs">The points to reduce.</param>
    /// <param name="targetDim">Target dimensionality.</param>
    /// <param name="options">Reduction options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Array of reduced points.</returns>
    Task<ProjectedPoint[]> DownProjectBatchAsync(ProjectedPoint[] inputs, int targetDim, DownerOptions options, CancellationToken ct = default);

    /// <summary>
    /// Trains the dimension reducer on a dataset.
    /// </summary>
    /// <param name="trainingData">Training data points.</param>
    /// <param name="targetDim">Target dimensionality.</param>
    /// <param name="options">Training options.</param>
    /// <param name="ct">Cancellation token.</param>
    Task TrainAsync(ProjectedPoint[] trainingData, int targetDim, DownerOptions options, CancellationToken ct = default);
}
