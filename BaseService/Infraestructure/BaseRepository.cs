using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using BaseService.Application.Domain;
using BaseService.Application.Interfaces;
using BaseService.Infraestructure.Persistence;

namespace BaseService.Infraestructure
{
    public class BaseRepository : IBaseRepository
    {
        private readonly AppDbContext _context;

        public BaseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Base user)
        {
            await _context.Bases.AddAsync(user);
        }

        public async Task<Base?> GetAsync(Expression<Func<Base, bool>> filter)
        {
            return await _context.Bases
                .AsNoTracking()
                .FirstOrDefaultAsync(filter);
        }

        public async Task<List<Base>> ListAsync(Expression<Func<Base, bool>> filter)
        {
            return await _context.Bases
             .AsNoTracking()
             .Where(filter)
             .ToListAsync();
        }
    }
}
