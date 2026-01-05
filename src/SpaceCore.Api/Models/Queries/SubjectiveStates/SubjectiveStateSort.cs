using Data.Repository;
using SpaceCore.Data.SpaceCoreDb.Entities;

namespace SpaceCore.Models.Queries.SubjectiveStates
{
    /// <summary>
    /// Subjective state
    /// </summary>
    public partial class SubjectiveStateSort : SortBase<Data.SpaceCoreDb.Entities.SubjectiveState>
    {
        public SortOperand? Id { get; set; }
        public SortOperand? Clock { get; set; }
        public SortOperand? Time { get; set; }
        public SortOperand? Data { get; set; }
        public SortOperand? SessionId { get; set; }
    }
}
