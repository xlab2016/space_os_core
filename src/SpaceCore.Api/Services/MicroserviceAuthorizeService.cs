using SpaceCore.Models;
using Api.AspNetCore.Models.Scope;
using Api.AspNetCore.Services;

namespace SpaceCore.Services
{
    public class MicroserviceAuthorizeService : AuthorizeService
    {
        public MicroserviceAuthorizeService(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
        }
    }
}
