using HotChocolate;
using HotChocolate.Authorization;
using SpaceCore.Models.Dtos;

namespace SpaceCore.Services.GraphQL
{
    public class NoiseChunksService : RestService2<NoiseChunk, long, NoiseChunkDto, NoiseChunkQuery, NoiseChunkMap>
    {
        private readonly SpaceCoreDbContext db;

        public NoiseChunksService(ILogger<RestServiceBase<NoiseChunk, long>> logger,
            IDapperDbContext restDapperDb,
            SpaceCoreDbContext restDb,
            NoiseChunkMap map)
            : base(logger,
                restDapperDb,
                restDb,
                "NoiseChunks",
                map)
        {
            this.db = restDb;
        }

        public override async Task<PagedList<NoiseChunkDto>> SearchAsync(NoiseChunkQuery query)
        {
            return await SearchUsingEfAsync(query, _ => _.
                Include(_ => _.Session));
        }
    }
}
