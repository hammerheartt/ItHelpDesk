using ItHelpDesk.Api.Data;
using ItHelpDesk.Api.Data.Entities;
using ItHelpDesk.Api.Data.Paging;
using ItHelpDesk.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace ItHelpDesk.Api.Repositories
{
    public interface ITicketsRepository
    {
        Task<int> CountOpenAssigned(string technicianId);
        Task<PagedList<Ticket>> GetFiltered(string? createdById, string? assignedToId, TicketStatus? status,
                                            TicketPriority? priority, int? categoryId, bool? overdue,
                                            SearchParameters searchParameters);
        Task<Ticket?> Get(int ticketId);
        Task Create(Ticket ticket);
        Task<List<TicketHistory>> GetHistory(int ticketId);
        Task<bool> SaveChanges();
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
                 t.Status == TicketStatus.Reopened ||
                 t.Status == TicketStatus.Resolved));
        }

        public async Task<PagedList<Ticket>> GetFiltered(string? createdById, string? assignedToId, TicketStatus? status,
                                                         TicketPriority? priority, int? categoryId, bool? overdue,
                                                         SearchParameters searchParameters)
        {
            var query = WithDetails();

            if (createdById != null) { query = query.Where(t => t.CreatedById == createdById); }
            if (assignedToId != null) { query = query.Where(t => t.AssignedToId == assignedToId); }
            if (status != null) { query = query.Where(t => t.Status == status); }
            if (priority != null) { query = query.Where(t => t.Priority == priority); }
            if (categoryId != null) { query = query.Where(t => t.CategoryId == categoryId); }
            if (overdue == true)
            {
                query = query.Where(TicketWorkflow.IsOverdueQuery(DateTimeOffset.UtcNow));
            }

            query = query.OrderByDescending(t => t.CreatedAt);
            return await PagedList<Ticket>.CreateAsync(query, searchParameters.PageNumber!.Value, searchParameters.PageSize!.Value);
        }

        public async Task<Ticket?> Get(int ticketId)
        {
            return await WithDetails().FirstOrDefaultAsync(t => t.Id == ticketId);
        }

        public async Task Create(Ticket ticket)
        {
            _db.Tickets.Add(ticket);
            await _db.SaveChangesAsync();
        }

        public async Task<List<TicketHistory>> GetHistory(int ticketId)
        {
            return await _db.TicketHistory
                .Include(h => h.ChangedBy)
                .Where(h => h.TicketId == ticketId)
                .OrderBy(h => h.ChangedAt).ThenBy(h => h.Id)
                .ToListAsync();
        }

        public async Task<bool> SaveChanges()
        {
            try
            {
                await _db.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        private IQueryable<Ticket> WithDetails()
        {
            return _db.Tickets
                .Include(t => t.Category)
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo);
        }
    }
}