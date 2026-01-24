using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using UserService.Application.Domain;
using UserService.Application.Interfaces;
using UserService.Infraestructure.Persistence;

namespace UserService.Infraestructure
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task<User?> GetAsync(Expression<Func<User, bool>> filter)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(filter);
        }

        public async Task<List<User>> ListAsync(Expression<Func<User, bool>> filter)
        {
            return await _context.Users
             .AsNoTracking()
             .Where(filter)
             .ToListAsync();
        }
    }
}
