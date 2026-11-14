using System.ComponentModel.DataAnnotations;

namespace ItHelpDesk.Api.Data.Dtos.Users
{
    public record SetUserActiveDto([Required] bool? IsActive);
}
