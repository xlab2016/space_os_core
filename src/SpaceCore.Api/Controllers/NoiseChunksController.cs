using Data.Repository;
using Data.Repository.Dapper;
using SpaceCore.Data.SpaceCoreDb.DatabaseContext;
using SpaceCore.Data.SpaceCoreDb.Entities;
using SpaceCore.Mappings;
using SpaceCore.Models.Dtos;
using SpaceCore.Models.Queries.NoiseChunks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using System.Net.Mime;
using Microsoft.EntityFrameworkCore;

namespace SpaceCore.Controllers
{
    /// <summary>
    /// Noise chunk
    /// </summary>
    [Route("/api/v1/noiseChunks")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdministrator,Administrator")]
    public partial class NoiseChunksController : RestControllerBase2<NoiseChunk, long, NoiseChunkDto, NoiseChunkQuery, NoiseChunkMap>
    {
        public NoiseChunksController(ILogger<RestServiceBase<NoiseChunk, long>> logger,
            IDapperDbContext restDapperDb,
            SpaceCoreDbContext restDb,
            NoiseChunkMap noiseChunkMap)
            : base(logger,
                restDapperDb,
                restDb,
                "NoiseChunks",
                noiseChunkMap)
        {
        }

        /// <summary>
        /// Search of NoiseChunk using given query
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">List of noiseChunks</response>
        /// <response code="400">Validation errors detected, operation denied</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/noiseChunks/search")]
        [HttpPost]
        [ProducesResponseType(typeof(PagedList<NoiseChunkDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<PagedList<NoiseChunkDto>> SearchAsync([FromBody] NoiseChunkQuery query)
        {
            return await SearchUsingEfAsync(query, _ => _.
                Include(_ => _.Session));
        }

        /// <summary>
        /// Get the noiseChunk by id
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">NoiseChunk data</response>
        /// <response code="401">Unauthorized request</response>
        [Route("/api/v1/noiseChunks/{key}")]
        [HttpGet]
        [ProducesResponseType(typeof(NoiseChunkDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<NoiseChunkDto> FindAsync([FromRoute] long key)
        {
            return await FindUsingEfAsync(key, _ => _.
                Include(_ => _.Session));
        }

    }
}
