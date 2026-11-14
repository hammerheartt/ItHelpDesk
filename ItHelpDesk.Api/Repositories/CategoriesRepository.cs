using ItHelpDesk.Api.Data;
using ItHelpDesk.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ItHelpDesk.Api.Repositories
{
    public interface ICategoriesRepository
    {
        Task<List<Category>> GetAll(bool includeInactive);
        Task<Category?> Get(int categoryId);
        Task<bool> NameExists(string name, int? excludeCategoryId = null);
        Task<bool> HasTickets(int categoryId);
        Task<Category> Create(Category category);
        Task<Category> Update(Category category);
        Task Delete(Category category);
    }

    public class CategoriesRepository : ICategoriesRepository
    {
        private readonly ItHelpDeskDbContext _db;
        public CategoriesRepository(ItHelpDeskDbContext db) { _db = db; }

        public async Task<List<Category>> GetAll(bool includeInactive)
        {
            var query = _db.Categories.AsQueryable();
            if (!includeInactive) { query = query.Where(c => c.IsActive); }
            return await query.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<Category?> Get(int categoryId)
        {
            return await _db.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
        }

        public async Task<bool> NameExists(string name, int? excludeCategoryId = null)
        {
            return await _db.Categories.AnyAsync(c =>
                c.Name == name && (excludeCategoryId == null || c.Id != excludeCategoryId));
        }

        public async Task<bool> HasTickets(int categoryId)
        {
            return await _db.Tickets.AnyAsync(t => t.CategoryId == categoryId);
        }

        public async Task<Category> Create(Category category)
        {
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();
            return category;
        }

        public async Task<Category> Update(Category category)
        {
            _db.Categories.Update(category);
            await _db.SaveChangesAsync();
            return category;
        }

        public async Task Delete(Category category)
        {
            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
        }
    }
}