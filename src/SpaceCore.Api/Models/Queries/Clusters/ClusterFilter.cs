using Data.Repository;
using SpaceCore.Data.SpaceCoreDb.Entities;

namespace SpaceCore.Models.Queries.Clusters
{
    /// <summary>
    /// Cluster
    /// </summary>
    public partial class ClusterFilter : FilterBase<Data.SpaceCoreDb.Entities.Cluster>
    {
        public FilterOperand<long>? Id { get; set; }
        public FilterOperand<int>? Clock { get; set; }
        public FilterOperand<DateTime>? Time { get; set; }
        public FilterOperand<object>? Data { get; set; }
        public FilterOperand<long?>? SessionId { get; set; }
    }
}
