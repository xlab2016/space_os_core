using Data.Repository;
using SpaceCore.Data.SpaceCoreDb.Entities;

namespace SpaceCore.Models.Queries.Sessions
{
    /// <summary>
    /// Сессия
    /// </summary>
    public partial class SessionSort : SortBase<Session>
    {
        public SortOperand? Id { get; set; }
        public SortOperand? Key { get; set; }
        public SortOperand? Time { get; set; }
    }
}
