using Data.Mapping;
using Data.Repository.Helpers;
using SpaceCore.Data.SpaceCoreDb.Entities;
using SpaceCore.Models.Dtos;

namespace SpaceCore.Mappings
{
    /// <summary>
    /// Сессия
    /// </summary>
    public partial class SessionMap : MapBase2<Session, SessionDto, MapOptions>
    {
        private readonly DbMapContext mapContext;

        public SessionMap(DbMapContext mapContext)
        {
            this.mapContext = mapContext;
        }

        public override SessionDto MapCore(Session source, MapOptions? options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new SessionDto();
            result.Id = source.Id;
            if (options.MapProperties)
            {
                result.Key = source.Key;
                result.Time = source.Time;
            }
            if (options.MapObjects)
            {
            }
            if (options.MapCollections)
            {
            }

            return result;
        }

        public override Session ReverseMapCore(SessionDto source, MapOptions options = null)
        {
            if (source == null)
                return null;

            options = options ?? new MapOptions();

            var result = new Session();
            result.Id = source.Id;
            if (options.MapProperties)
            {
                result.Key = source.Key;
                result.Time = source.Time.ToUtc();
            }
            if (options.MapObjects)
            {
            }
            if (options.MapCollections)
            {
            }

            return result;
        }

        public override void MapCore(Session source, Session destination, MapOptions options = null)
        {
            if (source == null || destination == null)
                return;

            options = options ?? new MapOptions();

            destination.Id = source.Id;
            if (options.MapProperties)
            {
                destination.Key = source.Key;
                destination.Time = source.Time;
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
