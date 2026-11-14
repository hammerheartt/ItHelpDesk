using System.ComponentModel.DataAnnotations;

namespace ItHelpDesk.Api.Data.Dtos.Categories
{
    public record CreateCategoryDto(
        [Required, StringLength(100, MinimumLength = 2)] string Name);
}