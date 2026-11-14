namespace ItHelpDesk.Api.Data.Entities
{
    public class TicketHistory
    {
        public int Id { get; set; }
        public required string Action { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public DateTimeOffset ChangedAt { get; set; }

        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        public required string ChangedById { get; set; }
        public ItHelpDeskUser ChangedBy { get; set; } = null!;
    }
}
