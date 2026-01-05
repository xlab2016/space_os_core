
namespace SpaceCore.Models.Dtos
{
    /// <summary>
    /// Сессия
    /// </summary>
    public partial class SessionDto
    {
        public long Id { get; set; }
        public string? Key { get; set; }
        public DateTime Time { get; set; }
    }
}
