using SpaceCore.Models;
using SpaceCore.Services;

namespace SpaceCore.Tests;

public class CrossDimensionProjectorTests
{
    private readonly CrossDimensionProjector _projector = new();
    private readonly EntropyGenerator _entropyGenerator = new();

    [Fact]
    public async Task ClusterAsync_WithNoiseChunks_ShouldReturnClusters()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var options = new EntropyOptions(ChunkSize: 512);
        var chunks = _entropyGenerator.GenerateChunks(sessionId, 20, options);
        var clusterOptions = new ClusterOptions(Algorithm: "online-kmeans", K: 4);

        // Act
        var clusters = await _projector.ClusterAsync(chunks, clusterOptions);

        // Assert
        Assert.NotEmpty(clusters);
        Assert.True(clusters.Length <= 4);
        foreach (var cluster in clusters)
        {
            Assert.NotNull(cluster.Centroid);
            Assert.True(cluster.Size > 0);
            Assert.NotEmpty(cluster.ClusterHash);
        }
    }

    [Fact]
    public async Task ClusterAsync_WithEmptyInput_ShouldReturnEmptyArray()
    {
        // Arrange
        var chunks = Array.Empty<NoiseChunk>();
        var options = new ClusterOptions();

        // Act
        var clusters = await _projector.ClusterAsync(chunks, options);

        // Assert
        Assert.Empty(clusters);
    }

    [Fact]
    public async Task UpdateClustersAsync_ShouldUpdateExistingClusters()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var options = new EntropyOptions(ChunkSize: 512);
        var initialChunks = _entropyGenerator.GenerateChunks(sessionId, 10, options);
        var clusterOptions = new ClusterOptions(Algorithm: "online-kmeans", K: 3);

        var initialClusters = await _projector.ClusterAsync(initialChunks, clusterOptions);

        var newChunk = _entropyGenerator.GenerateChunk(sessionId, options);

        // Act
        var updatedClusters = await _projector.UpdateClustersAsync(newChunk, initialClusters, clusterOptions);

        // Assert
        Assert.Equal(initialClusters.Length, updatedClusters.Length);
        // At least one cluster should have been updated
        Assert.True(updatedClusters.Any(uc =>
            initialClusters.All(ic => ic.Id != uc.Id || ic.Size != uc.Size)));
    }

    [Fact]
    public void AssignToCluster_ShouldReturnValidIndex()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var options = new EntropyOptions(ChunkSize: 512);
        var chunks = _entropyGenerator.GenerateChunks(sessionId, 20, options);
        var clusterOptions = new ClusterOptions(K: 4);

        var clusters = _projector.ClusterAsync(chunks, clusterOptions).Result;

        var testChunk = _entropyGenerator.GenerateChunk(sessionId, options);

        // Act
        var assignedIndex = _projector.AssignToCluster(testChunk, clusters);

        // Assert
        Assert.True(assignedIndex >= 0);
        Assert.True(assignedIndex < clusters.Length);
    }

    [Fact]
    public void Cluster_HashShouldBeDeterministic()
    {
        // Arrange
        var centroid = new float[] { 0.1f, 0.2f, 0.3f, 0.4f };
        var algorithm = "test-algorithm";
        var timestamp = DateTimeOffset.Parse("2024-01-01T00:00:00Z");

        // Act
        var hash1 = Cluster.ComputeHash(centroid, algorithm, timestamp);
        var hash2 = Cluster.ComputeHash(centroid, algorithm, timestamp);

        // Assert
        Assert.Equal(hash1, hash2);
    }
}
