using System.ComponentModel.DataAnnotations;

namespace ItHelpDesk.Api.Data.Dtos.Auth
{
    public record LoginRequestDto(
        [Required, StringLength(100, MinimumLength = 1), EmailAddress] string Email,
        [Required, StringLength(50, MinimumLength = 1)] string Password);
}
