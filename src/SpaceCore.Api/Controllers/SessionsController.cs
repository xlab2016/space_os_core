using Data.Repository;
using Data.Repository.Dapper;
using SpaceCore.Data.SpaceCoreDb.DatabaseContext;
using SpaceCore.Data.SpaceCoreDb.Entities;
using SpaceCore.Mappings;
using SpaceCore.Models.Dtos;
using SpaceCore.Models.Queries.Sessions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using System.Net.Mime;

namespace SpaceCore.Controllers
{
    /// <summary>
    /// Сессия
    /// </summary>
    [Route("/api/v1/sessions")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdministrator,Administrator")]
    public partial class SessionsController : RestControllerBase2<Session, long, SessionDto, SessionQuery, SessionMap>
    {
        public SessionsController(ILogger<RestServiceBase<Session, long>> logger,
            IDapperDbContext restDapperDb,
            SpaceCoreDbContext restDb,
            SessionMap sessionMap)
            : base(logger,
                restDapperDb,
                restDb,
                "Sessions",
                sessionMap)
        {
        }

        /// <summary>
        /// Search of Session using given query
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">List of sessions</response>
        /// <response code="400">Validation errors detected, operation denied</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/sessions/search")]
        [HttpPost]
        [ProducesResponseType(typeof(PagedList<SessionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<PagedList<SessionDto>> SearchAsync([FromBody] SessionQuery query)
        {
            return await base.SearchAsync(query);
        }

        /// <summary>
        /// Get the session by id
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">Session data</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/sessions/{key}")]
        [HttpGet]
        [ProducesResponseType(typeof(SessionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<SessionDto> FindAsync([FromRoute] long key)
        {
            return await base.FindAsync(key);
        }

    }
}
