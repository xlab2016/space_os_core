using System.Text.Json;
using Data.Repository;
using System.ComponentModel.DataAnnotations.Schema;

namespace SpaceCore.Data.SpaceCoreDb.Entities
{
    /// <summary>
    /// Emergent edge
    /// </summary>
    public partial class EmergentEdge : IEntityKey<long>
    {
        public long Id { get; set; }
        public int Clock { get; set; }
        public DateTime Time { get; set; }
        [Column(TypeName = "jsonb")]
        public string? Data { get; set; }
        public long? SessionId { get; set; }

        public Session? Session { get; set; }
    }
}
