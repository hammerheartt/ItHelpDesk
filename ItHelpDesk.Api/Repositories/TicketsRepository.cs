using ItHelpDesk.Api.Data;
using ItHelpDesk.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ItHelpDesk.Api.Repositories
{
    public interface ITicketsRepository
    {
        Task<int> CountOpenAssigned(string technicianId);
    }

    public class TicketsRepository : ITicketsRepository
    {
        private readonly ItHelpDeskDbContext _db;
        public TicketsRepository(ItHelpDeskDbContext db) { _db = db; }

        public async Task<int> CountOpenAssigned(string technicianId)
        {
            return await _db.Tickets.CountAsync(t =>
                t.AssignedToId == technicianId &&
                (t.Status == TicketStatus.Assigned ||
                 t.Status == TicketStatus.InProgress ||
                 t.Status == TicketStatus.Reopened));
        }
    }
}