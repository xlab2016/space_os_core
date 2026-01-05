using Data.Repository;
using Data.Repository.Dapper;
using SpaceCore.Data.SpaceCoreDb.DatabaseContext;
using SpaceCore.Data.SpaceCoreDb.Entities;
using SpaceCore.Mappings;
using SpaceCore.Models.Dtos;
using SpaceCore.Models.Queries.EmergentEdges;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using System.Net.Mime;
using Microsoft.EntityFrameworkCore;

namespace SpaceCore.Controllers
{
    /// <summary>
    /// Emergent edge
    /// </summary>
    [Route("/api/v1/emergentEdges")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdministrator,Administrator")]
    public partial class EmergentEdgesController : RestControllerBase2<EmergentEdge, long, EmergentEdgeDto, EmergentEdgeQuery, EmergentEdgeMap>
    {
        public EmergentEdgesController(ILogger<RestServiceBase<EmergentEdge, long>> logger,
            IDapperDbContext restDapperDb,
            SpaceCoreDbContext restDb,
            EmergentEdgeMap emergentEdgeMap)
            : base(logger,
                restDapperDb,
                restDb,
                "EmergentEdges",
                emergentEdgeMap)
        {
        }

        /// <summary>
        /// Search of EmergentEdge using given query
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">List of emergentEdges</response>
        /// <response code="400">Validation errors detected, operation denied</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/emergentEdges/search")]
        [HttpPost]
        [ProducesResponseType(typeof(PagedList<EmergentEdgeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<PagedList<EmergentEdgeDto>> SearchAsync([FromBody] EmergentEdgeQuery query)
        {
            return await SearchUsingEfAsync(query, _ => _.
                Include(_ => _.Session));
        }

        /// <summary>
        /// Get the emergentEdge by id
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">EmergentEdge data</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/emergentEdges/{key}")]
        [HttpGet]
        [ProducesResponseType(typeof(EmergentEdgeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<EmergentEdgeDto> FindAsync([FromRoute] long key)
        {
            return await FindUsingEfAsync(key, _ => _.
                Include(_ => _.Session));
        }

    }
}
