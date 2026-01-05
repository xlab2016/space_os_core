using Data.Repository;
using Data.Repository.Dapper;
using SpaceCore.Data.SpaceCoreDb.DatabaseContext;
using SpaceCore.Data.SpaceCoreDb.Entities;
using SpaceCore.Mappings;
using SpaceCore.Models.Dtos;
using SpaceCore.Models.Queries.ProjectedPoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using System.Net.Mime;
using Microsoft.EntityFrameworkCore;

namespace SpaceCore.Controllers
{
    /// <summary>
    /// Points
    /// </summary>
    [Route("/api/v1/projectedPoints")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdministrator,Administrator")]
    public partial class ProjectedPointsController : RestControllerBase2<ProjectedPoint, long, ProjectedPointDto, ProjectedPointQuery, ProjectedPointMap>
    {
        public ProjectedPointsController(ILogger<RestServiceBase<ProjectedPoint, long>> logger,
            IDapperDbContext restDapperDb,
            SpaceCoreDbContext restDb,
            ProjectedPointMap projectedPointMap)
            : base(logger,
                restDapperDb,
                restDb,
                "ProjectedPoints",
                projectedPointMap)
        {
        }

        /// <summary>
        /// Search of ProjectedPoint using given query
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">List of projectedPoints</response>
        /// <response code="400">Validation errors detected, operation denied</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/projectedPoints/search")]
        [HttpPost]
        [ProducesResponseType(typeof(PagedList<ProjectedPointDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<PagedList<ProjectedPointDto>> SearchAsync([FromBody] ProjectedPointQuery query)
        {
            return await SearchUsingEfAsync(query, _ => _.
                Include(_ => _.Session));
        }

        /// <summary>
        /// Get the projectedPoint by id
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">ProjectedPoint data</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/projectedPoints/{key}")]
        [HttpGet]
        [ProducesResponseType(typeof(ProjectedPointDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<ProjectedPointDto> FindAsync([FromRoute] long key)
        {
            return await FindUsingEfAsync(key, _ => _.
                Include(_ => _.Session));
        }

    }
}
