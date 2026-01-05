using HotChocolate;
using HotChocolate.Authorization;
using SpaceCore.Models.Dtos;

namespace SpaceCore.Services.GraphQL
{
    public class SessionsService : RestService2<Session, long, SessionDto, SessionQuery, SessionMap>
    {
        private readonly SpaceCoreDbContext db;

        public SessionsService(ILogger<RestServiceBase<Session, long>> logger,
            IDapperDbContext restDapperDb,
            SpaceCoreDbContext restDb,
            SessionMap map)
            : base(logger,
                restDapperDb,
                restDb,
                "Sessions",
                map)
        {
            this.db = restDb;
        }

        public override async Task<PagedList<SessionDto>> SearchAsync(SessionQuery query)
        {
            return await base.SearchAsync(query);
        }
    }
}
