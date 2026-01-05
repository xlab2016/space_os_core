using HotChocolate;
using HotChocolate.Authorization;
using SpaceCore.Models.Dtos;

namespace SpaceCore.Services.GraphQL
{
    public class ProjectedPointsService : RestService2<ProjectedPoint, long, ProjectedPointDto, ProjectedPointQuery, ProjectedPointMap>
    {
        private readonly SpaceCoreDbContext db;

        public ProjectedPointsService(ILogger<RestServiceBase<ProjectedPoint, long>> logger,
            IDapperDbContext restDapperDb,
            SpaceCoreDbContext restDb,
            ProjectedPointMap map)
            : base(logger,
                restDapperDb,
                restDb,
                "ProjectedPoints",
                map)
        {
            this.db = restDb;
        }

        public override async Task<PagedList<ProjectedPointDto>> SearchAsync(ProjectedPointQuery query)
        {
            return await SearchUsingEfAsync(query, _ => _.
                Include(_ => _.Session));
        }
    }
}
