using SpaceCore.Mappings;
using SpaceCore.Services;

namespace SpaceCore.Helpers
{
    static class StartupHelper
    {
        public static void AddMapping(this WebApplicationBuilder source)
        {
            var services = source.Services;

            services.AddScoped<DbMapContext>();

            services.AddScoped<ClockMap>();
            services.AddScoped<ClusterMap>();
            services.AddScoped<EmergentEdgeMap>();
            services.AddScoped<EmergentShapeMap>();
            services.AddScoped<NoiseChunkMap>();
            services.AddScoped<ProjectedPointMap>();
            services.AddScoped<SubjectiveStateMap>();
            services.AddScoped<SessionMap>();
            services.AddScoped<WorkflowLogMap>();
        }

        public static void AddServices(this WebApplicationBuilder source)
        {
            var services = source.Services;


        }

        public static void AddProviders(this WebApplicationBuilder source)
        {
            var services = source.Services;

        }
    }
}
