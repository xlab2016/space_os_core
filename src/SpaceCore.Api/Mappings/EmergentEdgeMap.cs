using Data.Mapping;
using Data.Repository.Helpers;
using SpaceCore.Data.SpaceCoreDb.Entities;
using SpaceCore.Models.Dtos;
using Newtonsoft.Json;
using Data.Repository.Helpers;

namespace SpaceCore.Mappings
{
    /// <summary>
    /// Emergent edge
    /// </summary>
    public partial class EmergentEdgeMap : MapBase2<EmergentEdge, EmergentEdgeDto, MapOptions>
    {
        private readonly DbMapContext mapContext;

        public EmergentEdgeMap(DbMapContext mapContext)
        {
            this.mapContext = mapContext;
        }

        public override EmergentEdgeDto MapCore(EmergentEdge source, MapOptions? options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new EmergentEdgeDto();
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

        public override EmergentEdge ReverseMapCore(EmergentEdgeDto source, MapOptions options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new EmergentEdge();
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

        public override void MapCore(EmergentEdge source, EmergentEdge destination, MapOptions options = null)
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
