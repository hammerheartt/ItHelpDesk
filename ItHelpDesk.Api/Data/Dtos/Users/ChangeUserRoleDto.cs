using System.ComponentModel.DataAnnotations;

namespace ItHelpDesk.Api.Data.Dtos.Users
{
    public record ChangeUserRoleDto([Required] string Role);
}
