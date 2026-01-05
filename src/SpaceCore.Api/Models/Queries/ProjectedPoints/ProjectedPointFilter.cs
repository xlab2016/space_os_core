using Data.Repository;
using SpaceCore.Data.SpaceCoreDb.Entities;

namespace SpaceCore.Models.Queries.ProjectedPoints
{
    /// <summary>
    /// Points
    /// </summary>
    public partial class ProjectedPointFilter : FilterBase<Data.SpaceCoreDb.Entities.ProjectedPoint>
    {
        public FilterOperand<long>? Id { get; set; }
        public FilterOperand<int>? Clock { get; set; }
        public FilterOperand<DateTime>? Time { get; set; }
        public FilterOperand<int>? Phase { get; set; }
        public FilterOperand<object>? Data { get; set; }
        public FilterOperand<long?>? SessionId { get; set; }
    }
}
