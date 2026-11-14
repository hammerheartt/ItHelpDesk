namespace ItHelpDesk.Api.Data.Dtos.Users
{
    public record UserDto(string Id, string FullName, string Email, string Role, bool IsActive);
}
