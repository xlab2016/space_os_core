using HotChocolate;
using HotChocolate.Authorization;
using SpaceCore.Models.Dtos;
using Data.Repository;
using Data.Repository.Dapper;

namespace SpaceCore.Services.GraphQL
{
    public class Mutation
    {
        [Authorize(Roles=["SuperAdministrator", "Administrator"])]
        public async Task<int> CreateClock(ClockDto clockDto, [Service] ClocksService service)
        {
            var result = await service.AddAsync(clockDto);
            return (int)result;
        }

        public async Task<bool> UpdateClock(ClockDto clockDto, [Service] ClocksService service)
        {
            var result = await service.UpdateAsync(clockDto);
            return (bool)result;
        }

        public async Task<RemoveOperationResult> DeleteClock(int id, [Service] ClocksService service)
        {
            var result = await service.RemoveAsync(id);
            return (RemoveOperationResult)result;
        }

        [Authorize(Roles=["SuperAdministrator", "Administrator"])]
        public async Task<int> CreateCluster(ClusterDto clusterDto, [Service] ClustersService service)
        {
            var result = await service.AddAsync(clusterDto);
            return (int)result;
        }

        public async Task<bool> UpdateCluster(ClusterDto clusterDto, [Service] ClustersService service)
        {
            var result = await service.UpdateAsync(clusterDto);
            return (bool)result;
        }

        public async Task<RemoveOperationResult> DeleteCluster(int id, [Service] ClustersService service)
        {
            var result = await service.RemoveAsync(id);
            return (RemoveOperationResult)result;
        }

        [Authorize(Roles=["SuperAdministrator", "Administrator"])]
        public async Task<int> CreateEmergentEdge(EmergentEdgeDto emergentEdgeDto, [Service] EmergentEdgesService service)
        {
            var result = await service.AddAsync(emergentEdgeDto);
            return (int)result;
        }

        public async Task<bool> UpdateEmergentEdge(EmergentEdgeDto emergentEdgeDto, [Service] EmergentEdgesService service)
        {
            var result = await service.UpdateAsync(emergentEdgeDto);
            return (bool)result;
        }

        public async Task<RemoveOperationResult> DeleteEmergentEdge(int id, [Service] EmergentEdgesService service)
        {
            var result = await service.RemoveAsync(id);
            return (RemoveOperationResult)result;
        }

        [Authorize(Roles=["SuperAdministrator", "Administrator"])]
        public async Task<int> CreateEmergentShape(EmergentShapeDto emergentShapeDto, [Service] EmergentShapesService service)
        {
            var result = await service.AddAsync(emergentShapeDto);
            return (int)result;
        }

        public async Task<bool> UpdateEmergentShape(EmergentShapeDto emergentShapeDto, [Service] EmergentShapesService service)
        {
            var result = await service.UpdateAsync(emergentShapeDto);
            return (bool)result;
        }

        public async Task<RemoveOperationResult> DeleteEmergentShape(int id, [Service] EmergentShapesService service)
        {
            var result = await service.RemoveAsync(id);
            return (RemoveOperationResult)result;
        }

        [Authorize(Roles=["SuperAdministrator", "Administrator"])]
        public async Task<int> CreateNoiseChunk(NoiseChunkDto noiseChunkDto, [Service] NoiseChunksService service)
        {
            var result = await service.AddAsync(noiseChunkDto);
            return (int)result;
        }

        public async Task<bool> UpdateNoiseChunk(NoiseChunkDto noiseChunkDto, [Service] NoiseChunksService service)
        {
            var result = await service.UpdateAsync(noiseChunkDto);
            return (bool)result;
        }

        public async Task<RemoveOperationResult> DeleteNoiseChunk(int id, [Service] NoiseChunksService service)
        {
            var result = await service.RemoveAsync(id);
            return (RemoveOperationResult)result;
        }

        [Authorize(Roles=["SuperAdministrator", "Administrator"])]
        public async Task<int> CreateProjectedPoint(ProjectedPointDto projectedPointDto, [Service] ProjectedPointsService service)
        {
            var result = await service.AddAsync(projectedPointDto);
            return (int)result;
        }

        public async Task<bool> UpdateProjectedPoint(ProjectedPointDto projectedPointDto, [Service] ProjectedPointsService service)
        {
            var result = await service.UpdateAsync(projectedPointDto);
            return (bool)result;
        }

        public async Task<RemoveOperationResult> DeleteProjectedPoint(int id, [Service] ProjectedPointsService service)
        {
            var result = await service.RemoveAsync(id);
            return (RemoveOperationResult)result;
        }

        [Authorize(Roles=["SuperAdministrator", "Administrator"])]
        public async Task<int> CreateSubjectiveState(SubjectiveStateDto subjectiveStateDto, [Service] SubjectiveStatesService service)
        {
            var result = await service.AddAsync(subjectiveStateDto);
            return (int)result;
        }

        public async Task<bool> UpdateSubjectiveState(SubjectiveStateDto subjectiveStateDto, [Service] SubjectiveStatesService service)
        {
            var result = await service.UpdateAsync(subjectiveStateDto);
            return (bool)result;
        }

        public async Task<RemoveOperationResult> DeleteSubjectiveState(int id, [Service] SubjectiveStatesService service)
        {
            var result = await service.RemoveAsync(id);
            return (RemoveOperationResult)result;
        }

        [Authorize(Roles=["SuperAdministrator", "Administrator"])]
        public async Task<int> CreateSession(SessionDto sessionDto, [Service] SessionsService service)
        {
            var result = await service.AddAsync(sessionDto);
            return (int)result;
        }

        public async Task<bool> UpdateSession(SessionDto sessionDto, [Service] SessionsService service)
        {
            var result = await service.UpdateAsync(sessionDto);
            return (bool)result;
        }

        public async Task<RemoveOperationResult> DeleteSession(int id, [Service] SessionsService service)
        {
            var result = await service.RemoveAsync(id);
            return (RemoveOperationResult)result;
        }

        [Authorize(Roles=["SuperAdministrator", "Administrator"])]
        public async Task<int> CreateWorkflowLog(WorkflowLogDto workflowLogDto, [Service] WorkflowLogsService service)
        {
            var result = await service.AddAsync(workflowLogDto);
            return (int)result;
        }

        public async Task<bool> UpdateWorkflowLog(WorkflowLogDto workflowLogDto, [Service] WorkflowLogsService service)
        {
            var result = await service.UpdateAsync(workflowLogDto);
            return (bool)result;
        }

        public async Task<RemoveOperationResult> DeleteWorkflowLog(int id, [Service] WorkflowLogsService service)
        {
            var result = await service.RemoveAsync(id);
            return (RemoveOperationResult)result;
        }
    }
}
