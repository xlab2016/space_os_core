
namespace SpaceCore.Mappings
{
    public partial class DbMapContext
    {
        public ClockMap ClockMap { get; }
        public ClusterMap ClusterMap { get; }
        public EmergentEdgeMap EmergentEdgeMap { get; }
        public EmergentShapeMap EmergentShapeMap { get; }
        public NoiseChunkMap NoiseChunkMap { get; }
        public ProjectedPointMap ProjectedPointMap { get; }
        public SubjectiveStateMap SubjectiveStateMap { get; }
        public SessionMap SessionMap { get; }
        public WorkflowLogMap WorkflowLogMap { get; }

        public DbMapContext()
        {
            ClockMap = new ClockMap(this);
            ClusterMap = new ClusterMap(this);
            EmergentEdgeMap = new EmergentEdgeMap(this);
            EmergentShapeMap = new EmergentShapeMap(this);
            NoiseChunkMap = new NoiseChunkMap(this);
            ProjectedPointMap = new ProjectedPointMap(this);
            SubjectiveStateMap = new SubjectiveStateMap(this);
            SessionMap = new SessionMap(this);
            WorkflowLogMap = new WorkflowLogMap(this);
        }
    }
}
