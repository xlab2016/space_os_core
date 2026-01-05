using Data.Mapping;
using Data.Repository.Helpers;
using SpaceCore.Data.SpaceCoreDb.Entities;
using SpaceCore.Models.Dtos;
using Newtonsoft.Json;
using Data.Repository.Helpers;

namespace SpaceCore.Mappings
{
    /// <summary>
    /// Points
    /// </summary>
    public partial class ProjectedPointMap : MapBase2<ProjectedPoint, ProjectedPointDto, MapOptions>
    {
        private readonly DbMapContext mapContext;

        public ProjectedPointMap(DbMapContext mapContext)
        {
            this.mapContext = mapContext;
        }

        public override ProjectedPointDto MapCore(ProjectedPoint source, MapOptions? options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new ProjectedPointDto();
            result.Id = source.Id;
            if (options.MapProperties)
            {
                result.Clock = source.Clock;
                result.Time = source.Time;
                result.Phase = source.Phase;
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

        public override ProjectedPoint ReverseMapCore(ProjectedPointDto source, MapOptions options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new ProjectedPoint();
            result.Id = source.Id;
            if (options.MapProperties)
            {
                result.Clock = source.Clock;
                result.Time = source.Time.ToUtc();
                result.Phase = source.Phase;
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

        public override void MapCore(ProjectedPoint source, ProjectedPoint destination, MapOptions options = null)
        {
            if (source == null || destination == null)
                return;

            options = options ?? new MapOptions();

            destination.Id = source.Id;
            if (options.MapProperties)
            {
                destination.Clock = source.Clock;
                destination.Time = source.Time;
                destination.Phase = source.Phase;
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
