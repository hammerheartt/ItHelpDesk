using ItHelpDesk.Api.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace ItHelpDesk.Api.Data.Dtos.Tickets
{
    public record AssignTicketDto(
        [Required] string TechnicianId,
        TicketPriority? Priority);
}
