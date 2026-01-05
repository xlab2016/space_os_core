using HotChocolate;
using HotChocolate.Authorization;
using SpaceCore.Models.Dtos;

namespace SpaceCore.Services.GraphQL
{
    public class EmergentShapesService : RestService2<EmergentShape, long, EmergentShapeDto, EmergentShapeQuery, EmergentShapeMap>
    {
        private readonly SpaceCoreDbContext db;

        public EmergentShapesService(ILogger<RestServiceBase<EmergentShape, long>> logger,
            IDapperDbContext restDapperDb,
            SpaceCoreDbContext restDb,
            EmergentShapeMap map)
            : base(logger,
                restDapperDb,
                restDb,
                "EmergentShapes",
                map)
        {
            this.db = restDb;
        }

        public override async Task<PagedList<EmergentShapeDto>> SearchAsync(EmergentShapeQuery query)
        {
            return await SearchUsingEfAsync(query, _ => _.
                Include(_ => _.Session));
        }
    }
}
