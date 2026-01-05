using HotChocolate;
using HotChocolate.Authorization;
using SpaceCore.Models.Dtos;

namespace SpaceCore.Services.GraphQL
{
    public class WorkflowLogsService : RestService2<WorkflowLog, long, WorkflowLogDto, WorkflowLogQuery, WorkflowLogMap>
    {
        private readonly SpaceCoreDbContext db;

        public WorkflowLogsService(ILogger<RestServiceBase<WorkflowLog, long>> logger,
            IDapperDbContext restDapperDb,
            SpaceCoreDbContext restDb,
            WorkflowLogMap map)
            : base(logger,
                restDapperDb,
                restDb,
                "WorkflowLogs",
                map)
        {
            this.db = restDb;
        }

        public override async Task<PagedList<WorkflowLogDto>> SearchAsync(WorkflowLogQuery query)
        {
            return await base.SearchAsync(query);
        }
    }
}
