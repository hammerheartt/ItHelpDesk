namespace ItHelpDesk.Api.Data.Dtos.Tickets
{
    public record TicketHistoryDto(int Id, string Action, string? OldValue, string? NewValue,
                                   string ChangedById, string ChangedByName, DateTimeOffset ChangedAt);
}