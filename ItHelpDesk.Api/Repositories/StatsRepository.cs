using ItHelpDesk.Api.Data;
using ItHelpDesk.Api.Data.Dtos.Stats;
using ItHelpDesk.Api.Data.Entities;
using ItHelpDesk.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace ItHelpDesk.Api.Repositories
{
    public record AssigneeCounts(string TechnicianId, int Open, int Resolved);

    public interface IStatsRepository
    {
        Task<Dictionary<TicketStatus, int>> CountByStatus();
        Task<int> CountOverdue(DateTimeOffset now);
        Task<int> CountReopened();
        Task<double?> AverageResolutionHours();
        Task<List<CategoryCountDto>> CountByCategory();
        Task<List<AssigneeCounts>> CountByAssignee();
    }

    public class StatsRepository : IStatsRepository
    {
        private readonly ItHelpDeskDbContext _db;
        public StatsRepository(ItHelpDeskDbContext db) { _db = db; }

        public async Task<Dictionary<TicketStatus, int>> CountByStatus()
        {
            return await _db.Tickets
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);
        }

        public async Task<int> CountOverdue(DateTimeOffset now)
        {
            return await _db.Tickets.CountAsync(TicketWorkflow.IsOverdueQuery(now));
        }

        public async Task<int> CountReopened()
        {
            return await _db.Tickets.CountAsync(t => t.ReopenCount > 0);
        }

        public async Task<double?> AverageResolutionHours()
        {
            var times = await _db.Tickets
                .Where(t => t.ResolvedAt != null)
                .Select(t => new { t.CreatedAt, ResolvedAt = t.ResolvedAt!.Value })
                .ToListAsync();

            if (times.Count == 0) return null;
            return Math.Round(times.Average(x => (x.ResolvedAt - x.CreatedAt).TotalHours), 1);
        }

        public async Task<List<CategoryCountDto>> CountByCategory()
        {
            var counts = await _db.Tickets
                .GroupBy(t => t.Category.Name)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            return counts.Select(x => new CategoryCountDto(x.Category, x.Count)).ToList();
        }

        public async Task<List<AssigneeCounts>> CountByAssignee()
        {
            return await _db.Tickets
                .Where(t => t.AssignedToId != null)
                .GroupBy(t => t.AssignedToId!)
                .Select(g => new AssigneeCounts(
                    g.Key,
                    g.Count(t => t.Status == TicketStatus.Assigned ||
                                 t.Status == TicketStatus.InProgress ||
                                 t.Status == TicketStatus.Reopened),
                    g.Count(t => t.Status == TicketStatus.Resolved ||
                                 t.Status == TicketStatus.Closed)))
                .ToListAsync();
        }
    }
}