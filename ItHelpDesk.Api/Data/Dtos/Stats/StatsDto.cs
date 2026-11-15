namespace ItHelpDesk.Api.Data.Dtos.Stats
{
    public record StatsDto(
        int TotalTickets,
        Dictionary<string, int> TicketsByStatus,
        int UnassignedTickets,
        int OpenTickets,
        int OverdueTickets,
        int ReopenedTickets,
        double? AverageResolutionHours,
        List<CategoryCountDto> TicketsByCategory,
        List<TechnicianWorkloadDto> Technicians);

    public record CategoryCountDto(string Category, int Count);

    public record TechnicianWorkloadDto(string Id, string FullName, bool IsActive, int OpenTickets, int ResolvedTickets);
}