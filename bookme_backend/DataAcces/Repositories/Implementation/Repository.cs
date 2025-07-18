﻿using Microsoft.EntityFrameworkCore;
using bookme_backend.DataAcces.Repositories.Interfaces;
using System.Linq.Expressions;
using bookme_backend.DataAcces.Models;

namespace bookme_backend.DataAcces.Repositories.Implementation
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly BookmeContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(BookmeContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }
        public virtual async Task<bool> Exist(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        } 
        public virtual async Task<List<T>> GetAllAsync() 
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public virtual void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public virtual async Task<List<T>> GetWhereAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }
        public async Task<List<T>> GetWhereWithIncludesAsync(
        Expression<Func<T, bool>> predicate,
        params Expression<Func<T, object>>[] includes)
        {
            var query = _dbSet.Where(predicate);

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.ToListAsync();
        }
        public async Task<List<T>> GetWhereAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IQueryable<T>>? include = null
        )
        {
            IQueryable<T> query = _dbSet.Where(predicate);

            if (include != null)
            {
                query = include(query);
            }

            return await query.ToListAsync();
        }
        public IQueryable<T> Query()
        {
            return _dbSet.AsQueryable();
        }
    }

}
