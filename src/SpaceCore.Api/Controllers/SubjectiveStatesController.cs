using Data.Repository;
using Data.Repository.Dapper;
using SpaceCore.Data.SpaceCoreDb.DatabaseContext;
using SpaceCore.Data.SpaceCoreDb.Entities;
using SpaceCore.Mappings;
using SpaceCore.Models.Dtos;
using SpaceCore.Models.Queries.SubjectiveStates;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using System.Net.Mime;
using Microsoft.EntityFrameworkCore;

namespace SpaceCore.Controllers
{
    /// <summary>
    /// Subjective state
    /// </summary>
    [Route("/api/v1/subjectiveStates")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdministrator,Administrator")]
    public partial class SubjectiveStatesController : RestControllerBase2<SubjectiveState, long, SubjectiveStateDto, SubjectiveStateQuery, SubjectiveStateMap>
    {
        public SubjectiveStatesController(ILogger<RestServiceBase<SubjectiveState, long>> logger,
            IDapperDbContext restDapperDb,
            SpaceCoreDbContext restDb,
            SubjectiveStateMap subjectiveStateMap)
            : base(logger,
                restDapperDb,
                restDb,
                "SubjectiveStates",
                subjectiveStateMap)
        {
        }

        /// <summary>
        /// Search of SubjectiveState using given query
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">List of subjectiveStates</response>
        /// <response code="400">Validation errors detected, operation denied</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/subjectiveStates/search")]
        [HttpPost]
        [ProducesResponseType(typeof(PagedList<SubjectiveStateDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<PagedList<SubjectiveStateDto>> SearchAsync([FromBody] SubjectiveStateQuery query)
        {
            return await SearchUsingEfAsync(query, _ => _.
                Include(_ => _.Session));
        }

        /// <summary>
        /// Get the subjectiveState by id
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">SubjectiveState data</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/subjectiveStates/{key}")]
        [HttpGet]
        [ProducesResponseType(typeof(SubjectiveStateDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<SubjectiveStateDto> FindAsync([FromRoute] long key)
        {
            return await FindUsingEfAsync(key, _ => _.
                Include(_ => _.Session));
        }

    }
}
