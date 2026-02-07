using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using BookingService.Application.Domain;
using BookingService.Application.Interfaces;
using BookingService.Infraestructure.Persistence;

namespace BookingService.Infraestructure
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _context;

        public BookingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Booking user)
        {
            await _context.Reservation.AddAsync(user);
        }

        public async Task<Booking?> GetAsync(Expression<Func<Booking, bool>> filter)
        {
            return await _context.Reservation
                .AsNoTracking()
                .FirstOrDefaultAsync(filter);
        }

        public async Task<List<Booking>> ListAsync(Expression<Func<Booking, bool>> filter)
        {
            return await _context.Reservation
             .AsNoTracking()
             .Where(filter)
             .ToListAsync();
        }
    }
}
