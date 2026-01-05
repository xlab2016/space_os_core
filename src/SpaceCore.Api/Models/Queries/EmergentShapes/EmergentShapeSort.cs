using Data.Repository;
using SpaceCore.Data.SpaceCoreDb.Entities;

namespace SpaceCore.Models.Queries.EmergentShapes
{
    /// <summary>
    /// Emergent shape
    /// </summary>
    public partial class EmergentShapeSort : SortBase<Data.SpaceCoreDb.Entities.EmergentShape>
    {
        public SortOperand? Id { get; set; }
        public SortOperand? Clock { get; set; }
        public SortOperand? Time { get; set; }
        public SortOperand? Data { get; set; }
        public SortOperand? SessionId { get; set; }
    }
}
