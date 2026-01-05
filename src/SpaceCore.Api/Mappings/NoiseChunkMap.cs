using Data.Mapping;
using Data.Repository.Helpers;
using SpaceCore.Data.SpaceCoreDb.Entities;
using SpaceCore.Models.Dtos;
using Newtonsoft.Json;
using Data.Repository.Helpers;

namespace SpaceCore.Mappings
{
    /// <summary>
    /// Noise chunk
    /// </summary>
    public partial class NoiseChunkMap : MapBase2<NoiseChunk, NoiseChunkDto, MapOptions>
    {
        private readonly DbMapContext mapContext;

        public NoiseChunkMap(DbMapContext mapContext)
        {
            this.mapContext = mapContext;
        }

        public override NoiseChunkDto MapCore(NoiseChunk source, MapOptions? options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new NoiseChunkDto();
            result.Id = source.Id;
            if (options.MapProperties)
            {
                result.Clock = source.Clock;
                result.Time = source.Time;
                result.Data = source.Data;
                result.SessionId = source.SessionId;
            }
            if (options.MapObjects)
            {
                result.Session = mapContext.SessionMap.Map(source.Session, options);
            }
            if (options.MapCollections)
            {
            }

            return result;
        }

        public override NoiseChunk ReverseMapCore(NoiseChunkDto source, MapOptions options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new NoiseChunk();
            result.Id = source.Id;
            if (options.MapProperties)
            {
                result.Clock = source.Clock;
                result.Time = source.Time.ToUtc();
                if (source.Data != null)
                    result.Data = JsonConvert.SerializeObject(source.Data);
                result.SessionId = source.SessionId;
            }
            if (options.MapObjects)
            {
                if (source.SessionId == null)
                    result.Session = mapContext.SessionMap.ReverseMap(source.Session, options);
            }
            if (options.MapCollections)
            {
            }

            return result;
        }

        public override void MapCore(NoiseChunk source, NoiseChunk destination, MapOptions options = null)
        {
            if (source == null || destination == null)
                return;

            options = options ?? new MapOptions();

            destination.Id = source.Id;
            if (options.MapProperties)
            {
                destination.Clock = source.Clock;
                destination.Time = source.Time;
                destination.Data = JsonHelper.NormalizeSafe(source.Data);
                destination.SessionId = source.SessionId;
            }
            if (options.MapObjects)
            {
            }
            if (options.MapCollections)
            {
            }

        }
    }
}
