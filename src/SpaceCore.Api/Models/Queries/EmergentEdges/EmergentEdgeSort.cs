using Data.Repository;
using SpaceCore.Data.SpaceCoreDb.Entities;

namespace SpaceCore.Models.Queries.EmergentEdges
{
    /// <summary>
    /// Emergent edge
    /// </summary>
    public partial class EmergentEdgeSort : SortBase<Data.SpaceCoreDb.Entities.EmergentEdge>
    {
        public SortOperand? Id { get; set; }
        public SortOperand? Clock { get; set; }
        public SortOperand? Time { get; set; }
        public SortOperand? Data { get; set; }
        public SortOperand? SessionId { get; set; }
    }
}
