using Data.Repository;
using SpaceCore.Data.SpaceCoreDb.Entities;

namespace SpaceCore.Models.Queries.Clocks
{
    /// <summary>
    /// Clock
    /// </summary>
    public partial class ClockFilter : FilterBase<Clock>
    {
        public FilterOperand<long>? Id { get; set; }
        public FilterOperand<int>? ClockCount { get; set; }
        public FilterOperand<DateTime>? Time { get; set; }
        public FilterOperand<object>? Data { get; set; }
        public FilterOperand<long?>? SessionId { get; set; }
    }
}
