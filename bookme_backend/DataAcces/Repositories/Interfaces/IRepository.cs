using System.Linq.Expressions;

namespace bookme_backend.DataAcces.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<List<T>> GetAllAsync();
        Task AddAsync(T entity);
        Task DeleteAsync(T entity);
        void Update(T entity);
        Task<bool> Exist(Expression<Func<T, bool>> predicate);
    }

}
