
namespace SpaceCore.Models.Dtos
{
    /// <summary>
    /// Emergent edge
    /// </summary>
    public partial class EmergentEdgeDto
    {
        public long Id { get; set; }
        public int Clock { get; set; }
        public DateTime Time { get; set; }
        public object? Data { get; set; }
        public long? SessionId { get; set; }

        public SessionDto? Session { get; set; }
    }
}
