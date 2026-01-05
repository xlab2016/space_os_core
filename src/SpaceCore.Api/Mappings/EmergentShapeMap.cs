using Data.Mapping;
using Data.Repository.Helpers;
using SpaceCore.Data.SpaceCoreDb.Entities;
using SpaceCore.Models.Dtos;
using Newtonsoft.Json;
using Data.Repository.Helpers;

namespace SpaceCore.Mappings
{
    /// <summary>
    /// Emergent shape
    /// </summary>
    public partial class EmergentShapeMap : MapBase2<EmergentShape, EmergentShapeDto, MapOptions>
    {
        private readonly DbMapContext mapContext;

        public EmergentShapeMap(DbMapContext mapContext)
        {
            this.mapContext = mapContext;
        }

        public override EmergentShapeDto MapCore(EmergentShape source, MapOptions? options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new EmergentShapeDto();
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

        public override EmergentShape ReverseMapCore(EmergentShapeDto source, MapOptions options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new EmergentShape();
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

        public override void MapCore(EmergentShape source, EmergentShape destination, MapOptions options = null)
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
