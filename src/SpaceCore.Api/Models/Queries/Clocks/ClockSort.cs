using Data.Repository;
using SpaceCore.Data.SpaceCoreDb.Entities;

namespace SpaceCore.Models.Queries.Clocks
{
    /// <summary>
    /// Clock
    /// </summary>
    public partial class ClockSort : SortBase<Clock>
    {
        public SortOperand? Id { get; set; }
        public SortOperand? ClockCount { get; set; }
        public SortOperand? Time { get; set; }
        public SortOperand? Data { get; set; }
        public SortOperand? SessionId { get; set; }
    }
}
