using Data.Repository;
using Data.Repository.Dapper;
using SpaceCore.Data.SpaceCoreDb.DatabaseContext;
using SpaceCore.Data.SpaceCoreDb.Entities;
using SpaceCore.Mappings;
using SpaceCore.Models.Dtos;
using SpaceCore.Models.Queries.Clocks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using System.Net.Mime;
using Microsoft.EntityFrameworkCore;

namespace SpaceCore.Controllers
{
    /// <summary>
    /// Clock
    /// </summary>
    [Route("/api/v1/clocks")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdministrator,Administrator")]
    public partial class ClocksController : RestControllerBase2<Clock, long, ClockDto, ClockQuery, ClockMap>
    {
        public ClocksController(ILogger<RestServiceBase<Clock, long>> logger,
            IDapperDbContext restDapperDb,
            SpaceCoreDbContext restDb,
            ClockMap clockMap)
            : base(logger,
                restDapperDb,
                restDb,
                "Clocks",
                clockMap)
        {
        }

        /// <summary>
        /// Search of Clock using given query
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">List of clocks</response>
        /// <response code="400">Validation errors detected, operation denied</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/clocks/search")]
        [HttpPost]
        [ProducesResponseType(typeof(PagedList<ClockDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<PagedList<ClockDto>> SearchAsync([FromBody] ClockQuery query)
        {
            return await SearchUsingEfAsync(query, _ => _.
                Include(_ => _.Session));
        }

        /// <summary>
        /// Get the clock by id
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">Clock data</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/clocks/{key}")]
        [HttpGet]
        [ProducesResponseType(typeof(ClockDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<ClockDto> FindAsync([FromRoute] long key)
        {
            return await FindUsingEfAsync(key, _ => _.
                Include(_ => _.Session));
        }

    }
}
