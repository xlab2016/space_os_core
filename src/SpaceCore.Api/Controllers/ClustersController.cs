using Data.Repository;
using Data.Repository.Dapper;
using SpaceCore.Data.SpaceCoreDb.DatabaseContext;
using SpaceCore.Data.SpaceCoreDb.Entities;
using SpaceCore.Mappings;
using SpaceCore.Models.Dtos;
using SpaceCore.Models.Queries.Clusters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using System.Net.Mime;
using Microsoft.EntityFrameworkCore;

namespace SpaceCore.Controllers
{
    /// <summary>
    /// Cluster
    /// </summary>
    [Route("/api/v1/clusters")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdministrator,Administrator")]
    public partial class ClustersController : RestControllerBase2<Cluster, long, ClusterDto, ClusterQuery, ClusterMap>
    {
        public ClustersController(ILogger<RestServiceBase<Cluster, long>> logger,
            IDapperDbContext restDapperDb,
            SpaceCoreDbContext restDb,
            ClusterMap clusterMap)
            : base(logger,
                restDapperDb,
                restDb,
                "Clusters",
                clusterMap)
        {
        }

        /// <summary>
        /// Search of Cluster using given query
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">List of clusters</response>
        /// <response code="400">Validation errors detected, operation denied</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/clusters/search")]
        [HttpPost]
        [ProducesResponseType(typeof(PagedList<ClusterDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<PagedList<ClusterDto>> SearchAsync([FromBody] ClusterQuery query)
        {
            return await SearchUsingEfAsync(query, _ => _.
                Include(_ => _.Session));
        }

        /// <summary>
        /// Get the cluster by id
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">Cluster data</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/clusters/{key}")]
        [HttpGet]
        [ProducesResponseType(typeof(ClusterDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<ClusterDto> FindAsync([FromRoute] long key)
        {
            return await FindUsingEfAsync(key, _ => _.
                Include(_ => _.Session));
        }

    }
}
