using Data.Repository;
using SpaceCore.Data.SpaceCoreDb.Entities;

namespace SpaceCore.Models.Queries.NoiseChunks
{
    /// <summary>
    /// Noise chunk
    /// </summary>
    public partial class NoiseChunkFilter : FilterBase<Data.SpaceCoreDb.Entities.NoiseChunk>
    {
        public FilterOperand<long>? Id { get; set; }
        public FilterOperand<int>? Clock { get; set; }
        public FilterOperand<DateTime>? Time { get; set; }
        public FilterOperand<object>? Data { get; set; }
        public FilterOperand<long?>? SessionId { get; set; }
    }
}
