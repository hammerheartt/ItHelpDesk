using System.ComponentModel.DataAnnotations;

namespace ItHelpDesk.Api.Data.Dtos.Comments
{
    public record CreateCommentDto(
        [Required, StringLength(4000, MinimumLength = 1)] string Text,
        bool IsInternal = false);
}