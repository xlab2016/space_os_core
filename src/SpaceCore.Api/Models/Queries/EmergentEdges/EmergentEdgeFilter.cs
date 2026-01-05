using Data.Repository;
using SpaceCore.Data.SpaceCoreDb.Entities;

namespace SpaceCore.Models.Queries.EmergentEdges
{
    /// <summary>
    /// Emergent edge
    /// </summary>
    public partial class EmergentEdgeFilter : FilterBase<Data.SpaceCoreDb.Entities.EmergentEdge>
    {
        public FilterOperand<long>? Id { get; set; }
        public FilterOperand<int>? Clock { get; set; }
        public FilterOperand<DateTime>? Time { get; set; }
        public FilterOperand<object>? Data { get; set; }
        public FilterOperand<long?>? SessionId { get; set; }
    }
}
