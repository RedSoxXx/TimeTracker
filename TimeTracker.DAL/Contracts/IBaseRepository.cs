using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Data.Entities.Base;

namespace TimeTracker.DAL.Contracts
{
    public interface IBaseRepository<TEntity> where TEntity : IEntity
    {
        Task SaveAsync(TEntity entity);
        Task SaveAsync(IEnumerable<TEntity> entities);
        Task DeleteAsync(TEntity entity, bool isSoftDelete = true);
        Task DeleteAsync(IEnumerable<TEntity> entities, bool isSoftDelete = true);
        Task DeleteAsync(int id, bool isSoftDelete = true);
        Task DeleteAsync(Expression<Func<TEntity, bool>> where, bool isSoftDelete = true);

        Task<TEntity> GetByIdAsync(int id, bool throwExceptionIfNotFound = true);
        Task<TResult> GetByIdAsync<TResult>(int id, Expression<Func<TEntity, TResult>> selector, bool throwExceptionIfNotFound = true);

        IQueryable<TEntity> GetAll(bool needDeleted = false);
        IQueryable<TResult> GetAll<TResult>(Expression<Func<TEntity, TResult>> selector, bool needDeleted = false);
        IQueryable<TEntity> GetMany(Expression<Func<TEntity, bool>> where, bool needDeleted = false);
        IQueryable<TResult> GetMany<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector, bool needDeleted = false);

        Task<TEntity> FirstOrDefaultAsync(bool needDeleted = false);
        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> where, bool needDeleted = false);
        Task<TEntity> FirstAsync(bool needDeleted = false);
        Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> where, bool needDeleted = false);
        Task<TEntity> SingleOrDefaultAsync(bool needDeleted = false);
        Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> where, bool needDeleted = false);
        Task<TEntity> SingleAsync(bool needDeleted = false);
        Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> where, bool needDeleted = false);

        Task<TResult> FirstOrDefaultAsync<TResult>(Expression<Func<TEntity, TResult>> selector, bool needDeleted = false);
        Task<TResult> FirstOrDefaultAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector, bool needDeleted = false);
        Task<TResult> FirstAsync<TResult>(Expression<Func<TEntity, TResult>> selector, bool needDeleted = false);
        Task<TResult> FirstAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector, bool needDeleted = false);
        Task<TResult> SingleOrDefaultAsync<TResult>(Expression<Func<TEntity, TResult>> selector, bool needDeleted = false);
        Task<TResult> SingleOrDefaultAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector, bool needDeleted = false);
        Task<TResult> SingleAsync<TResult>(Expression<Func<TEntity, TResult>> selector, bool needDeleted = false);
        Task<TResult> SingleAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector, bool needDeleted = false);

        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> where, bool needDeleted = false);
        Task<bool> AnyAsync(bool needDeleted = false);
        Task<bool> ExistAsync(int id);
        Task<int> CountAsync(Expression<Func<TEntity, bool>>? where = null, bool needDeleted = false);

        IQueryable<TEntity> AsNoTracking();
    }
}
