using Data.Mapping;
using Data.Repository.Helpers;
using SpaceCore.Data.SpaceCoreDb.Entities;
using SpaceCore.Models.Dtos;
using Newtonsoft.Json;
using Data.Repository.Helpers;

namespace SpaceCore.Mappings
{
    /// <summary>
    /// Clock
    /// </summary>
    public partial class ClockMap : MapBase2<Clock, ClockDto, MapOptions>
    {
        private readonly DbMapContext mapContext;

        public ClockMap(DbMapContext mapContext)
        {
            this.mapContext = mapContext;
        }

        public override ClockDto MapCore(Clock source, MapOptions? options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new ClockDto();
            result.Id = source.Id;
            if (options.MapProperties)
            {
                result.ClockCount = source.ClockCount;
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

        public override Clock ReverseMapCore(ClockDto source, MapOptions options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new Clock();
            result.Id = source.Id;
            if (options.MapProperties)
            {
                result.ClockCount = source.ClockCount;
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

        public override void MapCore(Clock source, Clock destination, MapOptions options = null)
        {
            if (source == null || destination == null)
                return;

            options = options ?? new MapOptions();

            destination.Id = source.Id;
            if (options.MapProperties)
            {
                destination.ClockCount = source.ClockCount;
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
