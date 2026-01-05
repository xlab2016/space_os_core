using HotChocolate;
using HotChocolate.Authorization;
using SpaceCore.Models.Dtos;

namespace SpaceCore.Services.GraphQL
{
    public class ClustersService : RestService2<Cluster, long, ClusterDto, ClusterQuery, ClusterMap>
    {
        private readonly SpaceCoreDbContext db;

        public ClustersService(ILogger<RestServiceBase<Cluster, long>> logger,
            IDapperDbContext restDapperDb,
            SpaceCoreDbContext restDb,
            ClusterMap map)
            : base(logger,
                restDapperDb,
                restDb,
                "Clusters",
                map)
        {
            this.db = restDb;
        }

        public override async Task<PagedList<ClusterDto>> SearchAsync(ClusterQuery query)
        {
            return await SearchUsingEfAsync(query, _ => _.
                Include(_ => _.Session));
        }
    }
}
