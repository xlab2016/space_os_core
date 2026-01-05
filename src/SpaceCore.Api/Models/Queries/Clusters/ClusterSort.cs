using Data.Repository;
using SpaceCore.Data.SpaceCoreDb.Entities;

namespace SpaceCore.Models.Queries.Clusters
{
    /// <summary>
    /// Cluster
    /// </summary>
    public partial class ClusterSort : SortBase<Data.SpaceCoreDb.Entities.Cluster>
    {
        public SortOperand? Id { get; set; }
        public SortOperand? Clock { get; set; }
        public SortOperand? Time { get; set; }
        public SortOperand? Data { get; set; }
        public SortOperand? SessionId { get; set; }
    }
}
