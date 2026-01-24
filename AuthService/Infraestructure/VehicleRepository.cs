using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using AuthService.Application.Domain;
using AuthService.Application.Interfaces;
using AuthService.Infraestructure.Persistence;

namespace AuthService.Infraestructure
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly AppDbContext _context;

        public VehicleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Vehicle user)
        {
            await _context.Vechiles.AddAsync(user);
        }

        public async Task<Vehicle?> GetAsync(Expression<Func<Vehicle, bool>> filter)
        {
            return await _context.Vechiles
                .AsNoTracking()
                .FirstOrDefaultAsync(filter);
        }

        public async Task<List<Vehicle>> ListAsync(Expression<Func<Vehicle, bool>> filter)
        {
            return await _context.Vechiles
             .AsNoTracking()
             .Where(filter)
             .ToListAsync();
        }
    }
}
