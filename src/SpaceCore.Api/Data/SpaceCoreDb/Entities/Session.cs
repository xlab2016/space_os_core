using Data.Repository;

namespace SpaceCore.Data.SpaceCoreDb.Entities
{
    /// <summary>
    /// Сессия
    /// </summary>
    public partial class Session : IEntityKey<long>
    {
        public long Id { get; set; }
        public string? Key { get; set; }
        public DateTime Time { get; set; }
    }
}
