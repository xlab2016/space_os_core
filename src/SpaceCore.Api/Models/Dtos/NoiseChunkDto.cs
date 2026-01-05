
namespace SpaceCore.Models.Dtos
{
    /// <summary>
    /// Noise chunk
    /// </summary>
    public partial class NoiseChunkDto
    {
        public long Id { get; set; }
        public int Clock { get; set; }
        public DateTime Time { get; set; }
        public object? Data { get; set; }
        public long? SessionId { get; set; }

        public SessionDto? Session { get; set; }
    }
}
