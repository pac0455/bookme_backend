using System.Linq.Expressions;

namespace bookme_backend.DataAcces.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<List<T>> GetAllAsync();
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        Task DeleteAsync(T entity);
        void Update(T entity);
        Task<bool> Exist(Expression<Func<T, bool>> predicate);
        Task SaveChangesAsync();
        Task<List<T>> GetWhereAsync(Expression<Func<T, bool>> predicate);

        Task<List<T>> GetWhereWithIncludesAsync( Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
        Task<List<T>> GetWhereAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IQueryable<T>>? include = null
        );



    }

}
