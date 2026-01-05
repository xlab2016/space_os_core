using Data.Repository;
using SpaceCore.Data.SpaceCoreDb.Entities;

namespace SpaceCore.Models.Queries.NoiseChunks
{
    /// <summary>
    /// Noise chunk
    /// </summary>
    public partial class NoiseChunkSort : SortBase<Data.SpaceCoreDb.Entities.NoiseChunk>
    {
        public SortOperand? Id { get; set; }
        public SortOperand? Clock { get; set; }
        public SortOperand? Time { get; set; }
        public SortOperand? Data { get; set; }
        public SortOperand? SessionId { get; set; }
    }
}
