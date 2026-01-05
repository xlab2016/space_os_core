using HotChocolate;
using HotChocolate.Authorization;
using SpaceCore.Models.Dtos;

namespace SpaceCore.Services.GraphQL
{
    public class EmergentEdgesService : RestService2<EmergentEdge, long, EmergentEdgeDto, EmergentEdgeQuery, EmergentEdgeMap>
    {
        private readonly SpaceCoreDbContext db;

        public EmergentEdgesService(ILogger<RestServiceBase<EmergentEdge, long>> logger,
            IDapperDbContext restDapperDb,
            SpaceCoreDbContext restDb,
            EmergentEdgeMap map)
            : base(logger,
                restDapperDb,
                restDb,
                "EmergentEdges",
                map)
        {
            this.db = restDb;
        }

        public override async Task<PagedList<EmergentEdgeDto>> SearchAsync(EmergentEdgeQuery query)
        {
            return await SearchUsingEfAsync(query, _ => _.
                Include(_ => _.Session));
        }
    }
}
