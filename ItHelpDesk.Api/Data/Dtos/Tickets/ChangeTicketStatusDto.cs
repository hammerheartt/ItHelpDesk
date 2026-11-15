using ItHelpDesk.Api.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace ItHelpDesk.Api.Data.Dtos.Tickets
{
    public record ChangeTicketStatusDto(
           [Required] TicketStatus? Status,
           [StringLength(2000)] string? Comment);
}
