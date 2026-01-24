using System.Linq.Expressions;
using UserService.Application.Domain;

namespace UserService.Application.Interfaces
{
    public interface IUserRepository
    {
        Task AddAsync(User user);
        Task<User?> GetAsync(Expression<Func<User, bool>> filter);
        Task<List<User>> ListAsync(Expression<Func<User, bool>> filter);
    }
}
