using HotChocolate;
using HotChocolate.Authorization;
using SpaceCore.Models.Dtos;

namespace SpaceCore.Services.GraphQL
{
    public class ClocksService : RestService2<Clock, long, ClockDto, ClockQuery, ClockMap>
    {
        private readonly SpaceCoreDbContext db;

        public ClocksService(ILogger<RestServiceBase<Clock, long>> logger,
            IDapperDbContext restDapperDb,
            SpaceCoreDbContext restDb,
            ClockMap map)
            : base(logger,
                restDapperDb,
                restDb,
                "Clocks",
                map)
        {
            this.db = restDb;
        }

        public override async Task<PagedList<ClockDto>> SearchAsync(ClockQuery query)
        {
            return await SearchUsingEfAsync(query, _ => _.
                Include(_ => _.Session));
        }
    }
}
