using Data.Repository;
using SpaceCore.Data.SpaceCoreDb.Entities;

namespace SpaceCore.Models.Queries.Clusters
{
    public partial class ClusterQuery : QueryBase<Data.SpaceCoreDb.Entities.Cluster, ClusterFilter, ClusterSort>
    {
    }
}
