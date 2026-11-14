using ItHelpDesk.Api.Data.Entities;

namespace ItHelpDesk.Api.Data.Dtos.Tickets
{
    public record TicketDto(
        int Id, string Title, string Description,
        TicketStatus Status, TicketPriority? Priority,
        int CategoryId, string CategoryName,
        string CreatedById, string CreatedByName,
        string? AssignedToId, string? AssignedToName,
        DateTimeOffset CreatedAt, DateTimeOffset? DueAt,
        DateTimeOffset? ResolvedAt, DateTimeOffset? ClosedAt,
        int ReopenCount, string? ResolutionNote, bool IsOverdue);
}