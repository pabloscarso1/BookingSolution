using System.Linq.Expressions;
using AuthService.Application.Domain;

namespace AuthService.Application.Interfaces
{
    public interface IVehicleRepository
    {
        Task AddAsync(Vehicle user);
        Task<Vehicle?> GetAsync(Expression<Func<Vehicle, bool>> filter);
        Task<List<Vehicle>> ListAsync(Expression<Func<Vehicle, bool>> filter);
    }
}
