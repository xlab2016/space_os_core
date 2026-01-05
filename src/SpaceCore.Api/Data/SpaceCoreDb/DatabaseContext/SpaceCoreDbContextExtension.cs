using System.Reflection;
using Data.Repository;

namespace SpaceCore.Data.SpaceCoreDb.DatabaseContext
{
    public static class SpaceCoreDbContextExtension
    {
        public static bool AllMigrationsApplied(this SpaceCoreDbContext context)
        {
            return context.AllMigrationsAppliedCore();
        }

        public static void EnsureSeeded(this SpaceCoreDbContext context)
        {
            context.EnsureSeededCore(_ =>
                {
                    var dbAssembly = Assembly.GetExecutingAssembly();
                    context.SaveChanges();
                });
        }
    }
}
