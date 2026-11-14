namespace ItHelpDesk.Api.Data.Entities
{
    public class Ticket
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }

        public TicketStatus Status { get; set; } = TicketStatus.New;
        public TicketPriority? Priority { get; set; }
        public int ReopenCount { get; set; }
        public string? ResolutionNote { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? DueAt { get; set; }
        public DateTimeOffset? ResolvedAt { get; set; }
        public DateTimeOffset? ClosedAt { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public required string CreatedById { get; set; }
        public ItHelpDeskUser CreatedBy { get; set; } = null!;

        public string? AssignedToId { get; set; }
        public ItHelpDeskUser? AssignedTo { get; set; }

        public List<TicketComment> Comments { get; set; } = new();
        public List<TicketHistory> History { get; set; } = new();
    }
}
