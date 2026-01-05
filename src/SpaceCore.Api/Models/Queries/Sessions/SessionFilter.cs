using Data.Repository;
using SpaceCore.Data.SpaceCoreDb.Entities;

namespace SpaceCore.Models.Queries.Sessions
{
    /// <summary>
    /// Сессия
    /// </summary>
    public partial class SessionFilter : FilterBase<Session>
    {
        public FilterOperand<long>? Id { get; set; }
        public FilterOperand<string>? Key { get; set; }
        public FilterOperand<DateTime>? Time { get; set; }
    }
}
