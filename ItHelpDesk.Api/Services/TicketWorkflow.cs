using ItHelpDesk.Api.Auth;
using ItHelpDesk.Api.Data.Entities;

namespace ItHelpDesk.Api.Services
{
    public static class TicketWorkflow
    {
        private static readonly Dictionary<(TicketStatus From, TicketStatus To), string[]> Transitions = new()
        {
            [(TicketStatus.New, TicketStatus.Assigned)] = new[] { ItHelpDeskRoles.Admin },
            [(TicketStatus.New, TicketStatus.Cancelled)] = new[] { ItHelpDeskRoles.Employee },
            [(TicketStatus.Assigned, TicketStatus.InProgress)] = new[] { ItHelpDeskRoles.Technician },
            [(TicketStatus.InProgress, TicketStatus.Resolved)] = new[] { ItHelpDeskRoles.Technician },
            [(TicketStatus.Resolved, TicketStatus.Closed)] = new[] { ItHelpDeskRoles.Employee, ItHelpDeskRoles.Admin },
            [(TicketStatus.Resolved, TicketStatus.Reopened)] = new[] { ItHelpDeskRoles.Employee },
            [(TicketStatus.Reopened, TicketStatus.InProgress)] = new[] { ItHelpDeskRoles.Technician },
        };

        public static readonly TicketStatus[] OpenStatuses =
            { TicketStatus.Assigned, TicketStatus.InProgress, TicketStatus.Reopened };

        public static readonly TimeSpan ReopenWindow = TimeSpan.FromDays(7);

        public static bool IsTransitionAllowed(TicketStatus from, TicketStatus to, string role) =>
            Transitions.TryGetValue((from, to), out var roles) && roles.Contains(role);

        public static bool IsValidTransition(TicketStatus from, TicketStatus to) =>
            Transitions.ContainsKey((from, to));

        public static TimeSpan DeadlineFor(TicketPriority priority) => priority switch
        {
            TicketPriority.Critical => TimeSpan.FromHours(4),
            TicketPriority.High => TimeSpan.FromDays(1),
            TicketPriority.Medium => TimeSpan.FromDays(3),
            TicketPriority.Low => TimeSpan.FromDays(5),
            _ => throw new ArgumentOutOfRangeException(nameof(priority))
        };

        public static bool IsOverdue(Ticket ticket, DateTimeOffset now) =>
            ticket.DueAt != null && ticket.DueAt < now && OpenStatuses.Contains(ticket.Status);
    }
}