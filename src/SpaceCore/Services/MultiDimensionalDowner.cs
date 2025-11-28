using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SpaceCore.Interfaces;
using SpaceCore.Models;

namespace SpaceCore.Services;

/// <summary>
/// Reduces dimensionality of projected points using PCA.
/// Implements simple PCA-like dimension reduction for MVP.
/// </summary>
public class MultiDimensionalDowner : IMultiDimensionalDowner
{
    private float[,]? _projectionMatrix;
    private float[]? _mean;
    private int _targetDim;

    /// <inheritdoc />
    public Task<ProjectedPoint> DownProjectAsync(
        ProjectedPoint input,
        int targetDim,
        DownerOptions options,
        CancellationToken ct = default)
    {
        if (input.Vector.Length <= targetDim)
        {
            return Task.FromResult(input);
        }

        float[] reducedVector;

        if (_projectionMatrix != null && _mean != null && _targetDim == targetDim)
        {
            // Use trained projection
            reducedVector = ApplyProjection(input.Vector);
        }
        else
        {
            // Simple truncation/averaging for untrained case
            reducedVector = SimpleDownProject(input.Vector, targetDim);
        }

        var result = new ProjectedPoint(
            Id: Guid.NewGuid(),
            ClusterId: input.ClusterId,
            Dimensionality: targetDim,
            Vector: reducedVector,
            Confidence: input.Confidence * 0.95, // Slight confidence reduction
            SemanticTag: input.SemanticTag,
            Trace: input.Trace
        );

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public async Task<ProjectedPoint[]> DownProjectBatchAsync(
        ProjectedPoint[] inputs,
        int targetDim,
        DownerOptions options,
        CancellationToken ct = default)
    {
        var results = new ProjectedPoint[inputs.Length];
        for (int i = 0; i < inputs.Length; i++)
        {
            results[i] = await DownProjectAsync(inputs[i], targetDim, options, ct);
        }
        return results;
    }

    /// <inheritdoc />
    public Task TrainAsync(
        ProjectedPoint[] trainingData,
        int targetDim,
        DownerOptions options,
        CancellationToken ct = default)
    {
        if (trainingData.Length == 0)
            return Task.CompletedTask;

        var inputDim = trainingData[0].Vector.Length;
        if (inputDim <= targetDim)
            return Task.CompletedTask;

        _targetDim = targetDim;

        // Compute mean
        _mean = new float[inputDim];
        foreach (var point in trainingData)
        {
            for (int i = 0; i < inputDim; i++)
            {
                _mean[i] += point.Vector[i] / trainingData.Length;
            }
        }

        // Compute covariance matrix (simplified)
        var covariance = ComputeCovariance(trainingData, _mean);

        // Perform power iteration to find top k eigenvectors (simplified PCA)
        _projectionMatrix = ComputeProjectionMatrix(covariance, targetDim);

        return Task.CompletedTask;
    }

    private float[] ApplyProjection(float[] input)
    {
        if (_projectionMatrix == null || _mean == null)
            throw new InvalidOperationException("Model not trained");

        var centered = new float[input.Length];
        for (int i = 0; i < input.Length; i++)
        {
            centered[i] = input[i] - _mean[i];
        }

        var result = new float[_targetDim];
        for (int j = 0; j < _targetDim; j++)
        {
            float sum = 0;
            for (int i = 0; i < centered.Length; i++)
            {
                sum += centered[i] * _projectionMatrix[i, j];
            }
            result[j] = sum;
        }

        return result;
    }

    private float[] SimpleDownProject(float[] input, int targetDim)
    {
        // Simple dimension reduction by grouping and averaging
        var result = new float[targetDim];
        int groupSize = input.Length / targetDim;
        int remainder = input.Length % targetDim;

        int inputIdx = 0;
        for (int i = 0; i < targetDim; i++)
        {
            int thisGroupSize = groupSize + (i < remainder ? 1 : 0);
            float sum = 0;
            for (int j = 0; j < thisGroupSize && inputIdx < input.Length; j++)
            {
                sum += input[inputIdx++];
            }
            result[i] = thisGroupSize > 0 ? sum / thisGroupSize : 0;
        }

        return result;
    }

    private float[,] ComputeCovariance(ProjectedPoint[] data, float[] mean)
    {
        int n = data[0].Vector.Length;
        var cov = new float[n, n];

        foreach (var point in data)
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = i; j < n; j++)
                {
                    var vi = point.Vector[i] - mean[i];
                    var vj = point.Vector[j] - mean[j];
                    cov[i, j] += vi * vj / data.Length;
                    if (i != j) cov[j, i] = cov[i, j];
                }
            }
        }

        return cov;
    }

    private float[,] ComputeProjectionMatrix(float[,] covariance, int k)
    {
        int n = covariance.GetLength(0);
        var projection = new float[n, k];

        // Simplified: use power iteration to find top k eigenvectors
        var random = new Random(42); // Deterministic for reproducibility

        for (int comp = 0; comp < k; comp++)
        {
            // Initialize random vector
            var v = new float[n];
            for (int i = 0; i < n; i++)
            {
                v[i] = (float)random.NextDouble();
            }
            Normalize(v);

            // Power iteration
            for (int iter = 0; iter < 50; iter++)
            {
                var newV = MultiplyMatrixVector(covariance, v);

                // Orthogonalize against previous components
                for (int prev = 0; prev < comp; prev++)
                {
                    var prevV = new float[n];
                    for (int i = 0; i < n; i++)
                        prevV[i] = projection[i, prev];

                    float dot = DotProduct(newV, prevV);
                    for (int i = 0; i < n; i++)
                        newV[i] -= dot * prevV[i];
                }

                Normalize(newV);
                v = newV;
            }

            for (int i = 0; i < n; i++)
            {
                projection[i, comp] = v[i];
            }
        }

        return projection;
    }

    private float[] MultiplyMatrixVector(float[,] matrix, float[] vector)
    {
        int n = matrix.GetLength(0);
        var result = new float[n];
        for (int i = 0; i < n; i++)
        {
            float sum = 0;
            for (int j = 0; j < n; j++)
            {
                sum += matrix[i, j] * vector[j];
            }
            result[i] = sum;
        }
        return result;
    }

    private void Normalize(float[] v)
    {
        double norm = Math.Sqrt(v.Sum(x => x * x));
        if (norm > 0)
        {
            for (int i = 0; i < v.Length; i++)
            {
                v[i] /= (float)norm;
            }
        }
    }

    private float DotProduct(float[] a, float[] b)
    {
        float sum = 0;
        for (int i = 0; i < a.Length; i++)
        {
            sum += a[i] * b[i];
        }
        return sum;
    }
}
