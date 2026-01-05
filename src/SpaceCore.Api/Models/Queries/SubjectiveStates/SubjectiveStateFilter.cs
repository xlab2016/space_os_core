using Data.Repository;
using SpaceCore.Data.SpaceCoreDb.Entities;

namespace SpaceCore.Models.Queries.SubjectiveStates
{
    /// <summary>
    /// Subjective state
    /// </summary>
    public partial class SubjectiveStateFilter : FilterBase<Data.SpaceCoreDb.Entities.SubjectiveState>
    {
        public FilterOperand<long>? Id { get; set; }
        public FilterOperand<int>? Clock { get; set; }
        public FilterOperand<DateTime>? Time { get; set; }
        public FilterOperand<object>? Data { get; set; }
        public FilterOperand<long?>? SessionId { get; set; }
    }
}
