using System.Linq.Expressions;
using BookingService.Application.Domain;

namespace BookingService.Application.Interfaces
{
    public interface IBaseRepository
    {
        Task AddAsync(Booking user);
        Task<Booking?> GetAsync(Expression<Func<Booking, bool>> filter);
        Task<List<Booking>> ListAsync(Expression<Func<Booking, bool>> filter);
    }
}
