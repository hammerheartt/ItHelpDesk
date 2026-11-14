using System.ComponentModel.DataAnnotations;

namespace ItHelpDesk.Api.Data.Dtos.Users
{
    public record CreateUserDto(
        [Required, StringLength(100, MinimumLength = 1)] string FullName,
        [Required, StringLength(100, MinimumLength = 1), EmailAddress] string Email,
        [Required, StringLength(50, MinimumLength = 1)] string Password,
        [Required] string Role);
}
