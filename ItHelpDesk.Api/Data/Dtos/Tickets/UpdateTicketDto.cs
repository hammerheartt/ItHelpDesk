using System.ComponentModel.DataAnnotations;

namespace ItHelpDesk.Api.Data.Dtos.Tickets
{
    public record UpdateTicketDto(
        [Required, StringLength(200, MinimumLength = 5)] string Title,
        [Required, StringLength(4000, MinimumLength = 10)] string Description,
        [Required] int? CategoryId);
}
