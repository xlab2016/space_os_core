using HotChocolate;
using HotChocolate.Authorization;
using SpaceCore.Models.Dtos;

namespace SpaceCore.Services.GraphQL
{
    public class Query
    {
        [Authorize(Roles=["SuperAdministrator", "Administrator"])]
        public async Task<PagedList<ClockDto>> Clocks(ClockQuery query, [GlobalState("currentUser")] ClaimsPrincipal user, [Service] ClocksService service)
        {
            return await service.SearchAsync(query);
        }

        [Authorize(Roles=["SuperAdministrator", "Administrator"])]
        public async Task<PagedList<ClusterDto>> Clusters(ClusterQuery query, [GlobalState("currentUser")] ClaimsPrincipal user, [Service] ClustersService service)
        {
            return await service.SearchAsync(query);
        }

        [Authorize(Roles=["SuperAdministrator", "Administrator"])]
        public async Task<PagedList<EmergentEdgeDto>> EmergentEdges(EmergentEdgeQuery query, [GlobalState("currentUser")] ClaimsPrincipal user, [Service] EmergentEdgesService service)
        {
            return await service.SearchAsync(query);
        }

        [Authorize(Roles=["SuperAdministrator", "Administrator"])]
        public async Task<PagedList<EmergentShapeDto>> EmergentShapes(EmergentShapeQuery query, [GlobalState("currentUser")] ClaimsPrincipal user, [Service] EmergentShapesService service)
        {
            return await service.SearchAsync(query);
        }

        [Authorize(Roles=["SuperAdministrator", "Administrator"])]
        public async Task<PagedList<NoiseChunkDto>> NoiseChunks(NoiseChunkQuery query, [GlobalState("currentUser")] ClaimsPrincipal user, [Service] NoiseChunksService service)
        {
            return await service.SearchAsync(query);
        }

        [Authorize(Roles=["SuperAdministrator", "Administrator"])]
        public async Task<PagedList<ProjectedPointDto>> ProjectedPoints(ProjectedPointQuery query, [GlobalState("currentUser")] ClaimsPrincipal user, [Service] ProjectedPointsService service)
        {
            return await service.SearchAsync(query);
        }

        [Authorize(Roles=["SuperAdministrator", "Administrator"])]
        public async Task<PagedList<SubjectiveStateDto>> SubjectiveStates(SubjectiveStateQuery query, [GlobalState("currentUser")] ClaimsPrincipal user, [Service] SubjectiveStatesService service)
        {
            return await service.SearchAsync(query);
        }

        [Authorize(Roles=["SuperAdministrator", "Administrator"])]
        public async Task<PagedList<SessionDto>> Sessions(SessionQuery query, [GlobalState("currentUser")] ClaimsPrincipal user, [Service] SessionsService service)
        {
            return await service.SearchAsync(query);
        }

        [Authorize(Roles=["SuperAdministrator", "Administrator"])]
        public async Task<PagedList<WorkflowLogDto>> WorkflowLogs(WorkflowLogQuery query, [GlobalState("currentUser")] ClaimsPrincipal user, [Service] WorkflowLogsService service)
        {
            return await service.SearchAsync(query);
        }
    }
}
