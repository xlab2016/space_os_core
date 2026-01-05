using Data.Repository;
using SpaceCore.Data.SpaceCoreDb.Entities;

namespace SpaceCore.Models.Queries.ProjectedPoints
{
    /// <summary>
    /// Points
    /// </summary>
    public partial class ProjectedPointSort : SortBase<Data.SpaceCoreDb.Entities.ProjectedPoint>
    {
        public SortOperand? Id { get; set; }
        public SortOperand? Clock { get; set; }
        public SortOperand? Time { get; set; }
        public SortOperand? Phase { get; set; }
        public SortOperand? Data { get; set; }
        public SortOperand? SessionId { get; set; }
    }
}
