namespace ItHelpDesk.Api.Data.Entities
{
    public class TicketComment
    {
        public int Id { get; set; }
        public required string Text { get; set; }
        public bool IsInternal { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        public required string AuthorId { get; set; }
        public ItHelpDeskUser Author { get; set; } = null!;
    }
}
