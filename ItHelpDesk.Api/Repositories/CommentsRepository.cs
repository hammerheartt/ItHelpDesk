using ItHelpDesk.Api.Data;
using ItHelpDesk.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ItHelpDesk.Api.Repositories
{
    public interface ICommentsRepository
    {
        Task<List<TicketComment>> GetForTicket(int ticketId, bool includeInternal);
        Task Create(TicketComment comment);
    }

    public class CommentsRepository : ICommentsRepository
    {
        private readonly ItHelpDeskDbContext _db;
        public CommentsRepository(ItHelpDeskDbContext db) { _db = db; }

        public async Task<List<TicketComment>> GetForTicket(int ticketId, bool includeInternal)
        {
            var query = _db.TicketComments
                .Include(c => c.Author)
                .Where(c => c.TicketId == ticketId);

            if (!includeInternal) { query = query.Where(c => !c.IsInternal); }

            return await query.OrderBy(c => c.CreatedAt).ThenBy(c => c.Id).ToListAsync();
        }

        public async Task Create(TicketComment comment)
        {
            _db.TicketComments.Add(comment);
            await _db.SaveChangesAsync();
            await _db.Entry(comment).Reference(c => c.Author).LoadAsync();
        }
    }
}