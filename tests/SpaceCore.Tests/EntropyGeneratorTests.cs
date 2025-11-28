using SpaceCore.Models;
using SpaceCore.Services;

namespace SpaceCore.Tests;

public class EntropyGeneratorTests
{
    private readonly EntropyGenerator _generator = new();

    [Fact]
    public void GenerateChunk_ShouldReturnValidNoiseChunk()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var options = new EntropyOptions(ChunkSize: 256);

        // Act
        var chunk = _generator.GenerateChunk(sessionId, options);

        // Assert
        Assert.NotNull(chunk);
        Assert.Equal(256, chunk.Bytes.Length);
        Assert.NotEmpty(chunk.NoiseHash);
        Assert.True(chunk.Sequence > 0);
    }

    [Fact]
    public void GenerateChunk_WithSeed_ShouldProduceDeterministicOutput()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var seed = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
                               17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32 };
        var options = new EntropyOptions(ChunkSize: 64, UseSeed: true, Seed: seed);

        // Act - generate with same seed twice (using fresh generators to reset sequence)
        var generator1 = new EntropyGenerator();
        var generator2 = new EntropyGenerator();

        var chunk1 = generator1.GenerateChunk(sessionId, options);
        var chunk2 = generator2.GenerateChunk(sessionId, options);

        // Assert - bytes should be identical for same seed and sequence
        Assert.Equal(chunk1.Bytes, chunk2.Bytes);
    }

    [Fact]
    public void GenerateChunks_ShouldReturnRequestedCount()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var options = new EntropyOptions(ChunkSize: 128);

        // Act
        var chunks = _generator.GenerateChunks(sessionId, 5, options);

        // Assert
        Assert.Equal(5, chunks.Length);
        foreach (var chunk in chunks)
        {
            Assert.Equal(128, chunk.Bytes.Length);
        }
    }

    [Fact]
    public void GenerateChunks_ShouldHaveIncreasingSequenceNumbers()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var options = new EntropyOptions(ChunkSize: 64);

        // Act
        var chunks = _generator.GenerateChunks(sessionId, 10, options);

        // Assert
        for (int i = 1; i < chunks.Length; i++)
        {
            Assert.True(chunks[i].Sequence > chunks[i - 1].Sequence);
        }
    }

    [Fact]
    public void EstimateEntropyQuality_ShouldReturnHighQuality()
    {
        // Act
        var quality = _generator.EstimateEntropyQuality();

        // Assert - CSPRNG should have high entropy
        Assert.True(quality > 0.95, $"Entropy quality {quality} is too low");
    }

    [Fact]
    public void NoiseChunk_ComputeHash_ShouldBeDeterministic()
    {
        // Arrange
        var seed = new byte[] { 1, 2, 3, 4 };
        var bytes = new byte[] { 5, 6, 7, 8 };
        var timestamp = DateTimeOffset.Parse("2024-01-01T00:00:00Z");

        // Act
        var hash1 = NoiseChunk.ComputeHash(seed, bytes, timestamp);
        var hash2 = NoiseChunk.ComputeHash(seed, bytes, timestamp);

        // Assert
        Assert.Equal(hash1, hash2);
    }
}
