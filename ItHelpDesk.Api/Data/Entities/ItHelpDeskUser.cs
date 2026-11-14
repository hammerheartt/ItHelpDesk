using Microsoft.AspNetCore.Identity;

namespace ItHelpDesk.Api.Data.Entities
{
    public class ItHelpDeskUser : IdentityUser
    {
        public required string FullName { get; set; }
        public bool IsAdmin { get; set; } = true;
        public bool ForceRelogin { get; set; }
    }
}
