
namespace SpaceCore.Models.Dtos
{
    /// <summary>
    /// Emergent shape
    /// </summary>
    public partial class EmergentShapeDto
    {
        public long Id { get; set; }
        public int Clock { get; set; }
        public DateTime Time { get; set; }
        public object? Data { get; set; }
        public long? SessionId { get; set; }

        public SessionDto? Session { get; set; }
    }
}
