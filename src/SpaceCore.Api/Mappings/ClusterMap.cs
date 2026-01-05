using Data.Mapping;
using Data.Repository.Helpers;
using SpaceCore.Data.SpaceCoreDb.Entities;
using SpaceCore.Models.Dtos;
using Newtonsoft.Json;
using Data.Repository.Helpers;

namespace SpaceCore.Mappings
{
    /// <summary>
    /// Cluster
    /// </summary>
    public partial class ClusterMap : MapBase2<Cluster, ClusterDto, MapOptions>
    {
        private readonly DbMapContext mapContext;

        public ClusterMap(DbMapContext mapContext)
        {
            this.mapContext = mapContext;
        }

        public override ClusterDto MapCore(Cluster source, MapOptions? options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new ClusterDto();
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

        public override Cluster ReverseMapCore(ClusterDto source, MapOptions options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new Cluster();
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

        public override void MapCore(Cluster source, Cluster destination, MapOptions options = null)
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
