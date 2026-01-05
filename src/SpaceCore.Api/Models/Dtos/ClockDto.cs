
namespace SpaceCore.Models.Dtos
{
    /// <summary>
    /// Clock
    /// </summary>
    public partial class ClockDto
    {
        public long Id { get; set; }
        public int ClockCount { get; set; }
        public DateTime Time { get; set; }
        public object? Data { get; set; }
        public long? SessionId { get; set; }

        public SessionDto? Session { get; set; }
    }
}
