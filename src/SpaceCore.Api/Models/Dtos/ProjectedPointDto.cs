
namespace SpaceCore.Models.Dtos
{
    /// <summary>
    /// Points
    /// </summary>
    public partial class ProjectedPointDto
    {
        public long Id { get; set; }
        public int Clock { get; set; }
        public DateTime Time { get; set; }
        public int Phase { get; set; }
        public object? Data { get; set; }
        public long? SessionId { get; set; }

        public SessionDto? Session { get; set; }
    }
}
