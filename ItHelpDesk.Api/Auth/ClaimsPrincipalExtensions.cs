using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ItHelpDesk.Api.Auth
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserId(this ClaimsPrincipal user) =>
            user.FindFirstValue(JwtRegisteredClaimNames.Sub)!;

        public static string GetRole(this ClaimsPrincipal user) =>
            ItHelpDeskRoles.All.First(user.IsInRole);
    }
}