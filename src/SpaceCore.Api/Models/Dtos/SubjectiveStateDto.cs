
namespace SpaceCore.Models.Dtos
{
    /// <summary>
    /// Subjective state
    /// </summary>
    public partial class SubjectiveStateDto
    {
        public long Id { get; set; }
        public int Clock { get; set; }
        public DateTime Time { get; set; }
        public object? Data { get; set; }
        public long? SessionId { get; set; }

        public SessionDto? Session { get; set; }
    }
}
