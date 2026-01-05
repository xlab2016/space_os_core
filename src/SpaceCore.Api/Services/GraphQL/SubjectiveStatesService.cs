using HotChocolate;
using HotChocolate.Authorization;
using SpaceCore.Models.Dtos;

namespace SpaceCore.Services.GraphQL
{
    public class SubjectiveStatesService : RestService2<SubjectiveState, long, SubjectiveStateDto, SubjectiveStateQuery, SubjectiveStateMap>
    {
        private readonly SpaceCoreDbContext db;

        public SubjectiveStatesService(ILogger<RestServiceBase<SubjectiveState, long>> logger,
            IDapperDbContext restDapperDb,
            SpaceCoreDbContext restDb,
            SubjectiveStateMap map)
            : base(logger,
                restDapperDb,
                restDb,
                "SubjectiveStates",
                map)
        {
            this.db = restDb;
        }

        public override async Task<PagedList<SubjectiveStateDto>> SearchAsync(SubjectiveStateQuery query)
        {
            return await SearchUsingEfAsync(query, _ => _.
                Include(_ => _.Session));
        }
    }
}
