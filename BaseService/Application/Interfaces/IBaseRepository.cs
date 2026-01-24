using System.Linq.Expressions;
using BaseService.Application.Domain;

namespace BaseService.Application.Interfaces
{
    public interface IBaseRepository
    {
        Task AddAsync(Base user);
        Task<Base?> GetAsync(Expression<Func<Base, bool>> filter);
        Task<List<Base>> ListAsync(Expression<Func<Base, bool>> filter);
    }
}
