using Data.Repository;
using Data.Repository.Dapper;
using SpaceCore.Data.SpaceCoreDb.DatabaseContext;
using SpaceCore.Data.SpaceCoreDb.Entities;
using SpaceCore.Mappings;
using SpaceCore.Models.Dtos;
using SpaceCore.Models.Queries.EmergentShapes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using System.Net.Mime;
using Microsoft.EntityFrameworkCore;

namespace SpaceCore.Controllers
{
    /// <summary>
    /// Emergent shape
    /// </summary>
    [Route("/api/v1/emergentShapes")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdministrator,Administrator")]
    public partial class EmergentShapesController : RestControllerBase2<EmergentShape, long, EmergentShapeDto, EmergentShapeQuery, EmergentShapeMap>
    {
        public EmergentShapesController(ILogger<RestServiceBase<EmergentShape, long>> logger,
            IDapperDbContext restDapperDb,
            SpaceCoreDbContext restDb,
            EmergentShapeMap emergentShapeMap)
            : base(logger,
                restDapperDb,
                restDb,
                "EmergentShapes",
                emergentShapeMap)
        {
        }

        /// <summary>
        /// Search of EmergentShape using given query
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">List of emergentShapes</response>
        /// <response code="400">Validation errors detected, operation denied</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/emergentShapes/search")]
        [HttpPost]
        [ProducesResponseType(typeof(PagedList<EmergentShapeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<PagedList<EmergentShapeDto>> SearchAsync([FromBody] EmergentShapeQuery query)
        {
            return await SearchUsingEfAsync(query, _ => _.
                Include(_ => _.Session));
        }

        /// <summary>
        /// Get the emergentShape by id
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">EmergentShape data</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/emergentShapes/{key}")]
        [HttpGet]
        [ProducesResponseType(typeof(EmergentShapeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<EmergentShapeDto> FindAsync([FromRoute] long key)
        {
            return await FindUsingEfAsync(key, _ => _.
                Include(_ => _.Session));
        }

    }
}
