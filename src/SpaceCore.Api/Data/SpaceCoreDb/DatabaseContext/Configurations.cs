using SpaceCore.Data.SpaceCoreDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SpaceCore.Data.SpaceCoreDb.DatabaseContext
{
    public class ClocksConfiguration : IEntityTypeConfiguration<Clock>
    {
        public bool IsInMemoryDb { get; set; }

        public void Configure(EntityTypeBuilder<Clock> builder)
        {
            builder.HasKey(x => x.Id);

            if (!IsInMemoryDb)
            {
                builder.Property(_ => _.Data).HasColumnType("jsonb");
            }
            else
            {
                builder.Ignore(_ => _.Data);
            }
        }
    }

    public class ClustersConfiguration : IEntityTypeConfiguration<Cluster>
    {
        public bool IsInMemoryDb { get; set; }

        public void Configure(EntityTypeBuilder<Cluster> builder)
        {
            builder.HasKey(x => x.Id);

            if (!IsInMemoryDb)
            {
                builder.Property(_ => _.Data).HasColumnType("jsonb");
            }
            else
            {
                builder.Ignore(_ => _.Data);
            }
        }
    }

    public class EmergentEdgesConfiguration : IEntityTypeConfiguration<EmergentEdge>
    {
        public bool IsInMemoryDb { get; set; }

        public void Configure(EntityTypeBuilder<EmergentEdge> builder)
        {
            builder.HasKey(x => x.Id);

            if (!IsInMemoryDb)
            {
                builder.Property(_ => _.Data).HasColumnType("jsonb");
            }
            else
            {
                builder.Ignore(_ => _.Data);
            }
        }
    }

    public class EmergentShapesConfiguration : IEntityTypeConfiguration<EmergentShape>
    {
        public bool IsInMemoryDb { get; set; }

        public void Configure(EntityTypeBuilder<EmergentShape> builder)
        {
            builder.HasKey(x => x.Id);

            if (!IsInMemoryDb)
            {
                builder.Property(_ => _.Data).HasColumnType("jsonb");
            }
            else
            {
                builder.Ignore(_ => _.Data);
            }
        }
    }

    public class NoiseChunksConfiguration : IEntityTypeConfiguration<NoiseChunk>
    {
        public bool IsInMemoryDb { get; set; }

        public void Configure(EntityTypeBuilder<NoiseChunk> builder)
        {
            builder.HasKey(x => x.Id);

            if (!IsInMemoryDb)
            {
                builder.Property(_ => _.Data).HasColumnType("jsonb");
            }
            else
            {
                builder.Ignore(_ => _.Data);
            }
        }
    }

    public class ProjectedPointsConfiguration : IEntityTypeConfiguration<ProjectedPoint>
    {
        public bool IsInMemoryDb { get; set; }

        public void Configure(EntityTypeBuilder<ProjectedPoint> builder)
        {
            builder.HasKey(x => x.Id);

            if (!IsInMemoryDb)
            {
                builder.Property(_ => _.Data).HasColumnType("jsonb");
            }
            else
            {
                builder.Ignore(_ => _.Data);
            }
        }
    }

    public class SubjectiveStatesConfiguration : IEntityTypeConfiguration<SubjectiveState>
    {
        public bool IsInMemoryDb { get; set; }

        public void Configure(EntityTypeBuilder<SubjectiveState> builder)
        {
            builder.HasKey(x => x.Id);

            if (!IsInMemoryDb)
            {
                builder.Property(_ => _.Data).HasColumnType("jsonb");
            }
            else
            {
                builder.Ignore(_ => _.Data);
            }
        }
    }

    public class SessionsConfiguration : IEntityTypeConfiguration<Session>
    {
        public void Configure(EntityTypeBuilder<Session> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }

    public class WorkflowLogsConfiguration : IEntityTypeConfiguration<WorkflowLog>
    {
        public void Configure(EntityTypeBuilder<WorkflowLog> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }
}
