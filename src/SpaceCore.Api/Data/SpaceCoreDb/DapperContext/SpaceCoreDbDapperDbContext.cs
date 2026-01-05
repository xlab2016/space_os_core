using Data.Repository.Dapper;
using Microsoft.Extensions.Configuration;

namespace SpaceCore.Data.SpaceCoreDb.DapperContext
{
    public partial class SpaceCoreDbDapperDbContext : DapperDbContext
    {
        public SpaceCoreDbDapperDbContext(IConfiguration configuration)
            : base(configuration)
        {
        }
    }
}
