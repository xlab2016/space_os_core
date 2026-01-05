using Data.Repository;
using SpaceCore.Data.SpaceCoreDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace SpaceCore.Data.SpaceCoreDb.DatabaseContext
{
    public partial class SpaceCoreDbContext : DbContext
    {
        public DbSet<Clock>? Clocks { get; set; }
        public DbSet<Cluster>? Clusters { get; set; }
        public DbSet<EmergentEdge>? EmergentEdges { get; set; }
        public DbSet<EmergentShape>? EmergentShapes { get; set; }
        public DbSet<NoiseChunk>? NoiseChunks { get; set; }
        public DbSet<ProjectedPoint>? ProjectedPoints { get; set; }
        public DbSet<SubjectiveState>? SubjectiveStates { get; set; }
        public DbSet<Session>? Sessions { get; set; }
        public DbSet<WorkflowLog>? WorkflowLogs { get; set; }

        public SpaceCoreDbContext(DbContextOptions<SpaceCoreDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ClocksConfiguration { IsInMemoryDb = this.IsInMemoryDb() });
            modelBuilder.ApplyConfiguration(new ClustersConfiguration { IsInMemoryDb = this.IsInMemoryDb() });
            modelBuilder.ApplyConfiguration(new EmergentEdgesConfiguration { IsInMemoryDb = this.IsInMemoryDb() });
            modelBuilder.ApplyConfiguration(new EmergentShapesConfiguration { IsInMemoryDb = this.IsInMemoryDb() });
            modelBuilder.ApplyConfiguration(new NoiseChunksConfiguration { IsInMemoryDb = this.IsInMemoryDb() });
            modelBuilder.ApplyConfiguration(new ProjectedPointsConfiguration { IsInMemoryDb = this.IsInMemoryDb() });
            modelBuilder.ApplyConfiguration(new SubjectiveStatesConfiguration { IsInMemoryDb = this.IsInMemoryDb() });
            modelBuilder.ApplyConfiguration(new SessionsConfiguration());
            modelBuilder.ApplyConfiguration(new WorkflowLogsConfiguration());
        }
    }
}
