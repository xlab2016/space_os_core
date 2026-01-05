using Api.AspNetCore.Models.Secure;

namespace SpaceCore.Models
{
    public partial class MicroserviceUserToken : JwtToken
    {
        public bool? IsApproved { get; set; }
        public int? FilialId { get; set; }
    }
}
