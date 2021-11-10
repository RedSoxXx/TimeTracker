using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Common.Extensions;
using TimeTracker.DAL.Contracts;
using TimeTracker.DAL.Exceptions;
using TimeTracker.Data.Entities.Base;

namespace TimeTracker.DAL.Repositories
{
    public class EntityRepository<TEntity> : IBaseRepository<TEntity> where TEntity : BaseEntity
    {
        protected readonly DbContext Context;
        protected readonly DbSet<TEntity> DbSet;

        public EntityRepository(DbContext context)
        {
            Context = context;
            DbSet = Context.Set<TEntity>();
        }

        public virtual async Task SaveAsync(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.IsNew())
                await AddAsync(entity);
            else
                await UpdateAsync(entity);
        }

        public virtual async Task SaveAsync(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                await SaveAsync(entity);
            }
        }

        protected virtual async Task AddAsync(TEntity entity)
        {
            if (entity.CreatedAt.IsEmpty())
                entity.CreatedAt = DateTime.UtcNow;

            await DbSet.AddAsync(entity);
        }

        protected virtual async Task UpdateAsync(TEntity entity)
        {
            await Task.Run(() =>
            {
                var entityState = Context.Entry(entity).State;
                if (entityState == EntityState.Detached)
                {
                    entityState = EntityState.Modified;
                    DbSet.Attach(entity);
                }

                if (entityState == EntityState.Modified)
                    entity.UpdatedAt = DateTime.UtcNow;
            });
        }

        public virtual async Task DeleteAsync(TEntity entity, bool isSoftDelete = true)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (isSoftDelete)
            {
                entity.IsDeleted = true;
                await UpdateAsync(entity);

                return;
            }

            var dbEntityEntry = Context.Entry(entity);
            if (dbEntityEntry.State != EntityState.Deleted)
            {
                dbEntityEntry.State = EntityState.Deleted;
            }
            else
            {
                DbSet.Attach(entity);
                DbSet.Remove(entity);
            }
        }

        public virtual async Task DeleteAsync(IEnumerable<TEntity> entities, bool isSoftDelete = true)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            foreach (var entity in entities)
            {
                await DeleteAsync(entity, isSoftDelete);
            }
        }

        public virtual async Task DeleteAsync(long id, bool isSoftDelete = true)
        {
            var found = await GetByIdAsync(id, isSoftDelete);
            await DeleteAsync(found);
        }

        public virtual async Task DeleteAsync(Expression<Func<TEntity, bool>> where, bool isSoftDelete = true)
        {
            foreach (var entity in GetMany(where))
            {
                await DeleteAsync(entity, isSoftDelete);
            }
        }

        public virtual async Task<TEntity> GetByIdAsync(long id, bool throwExceptionIfNotFound = true)
        {
            var found = await DbSet.FindAsync(id);

            if (found == null && throwExceptionIfNotFound)
                throw new EntityNotFoundException(typeof(TEntity), id);

            return found;
        }

        public virtual async Task<TResult> GetByIdAsync<TResult>(long id, Expression<Func<TEntity, TResult>> selector, bool throwExceptionIfNotFound = true)
        {
            var result = await GetMany(e => e.Id == id, selector).Take(1).ToArrayAsync();
            if (!result.Any() && throwExceptionIfNotFound)
                throw new EntityNotFoundException(typeof(TEntity), id);

            return result.FirstOrDefault();
        }

        public virtual IQueryable<TEntity> GetAll(bool needDeleted = false)
        {
            return DbSet.Where(x => !x.IsDeleted || needDeleted && x.IsDeleted);
        }

        public virtual IQueryable<TResult> GetAll<TResult>(Expression<Func<TEntity, TResult>> selector, bool needDeleted = false)
        {
            return GetAll(needDeleted: needDeleted).Select(selector);
        }

        public virtual IQueryable<TEntity> GetMany(Expression<Func<TEntity, bool>> where, bool needDeleted = false)
        {
            return GetAll(needDeleted: needDeleted).Where(where);
        }

        public virtual IQueryable<TResult> GetMany<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector, bool needDeleted = false)
        {
            return GetAll(needDeleted: needDeleted).Where(where).Select(selector);
        }

        public virtual async Task<TEntity> FirstOrDefaultAsync(bool needDeleted = false)
        {
            return await GetAll(needDeleted: needDeleted).FirstOrDefaultAsync();
        }

        public virtual async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> where, bool needDeleted = false)
        {
            return await GetAll(needDeleted: needDeleted).FirstOrDefaultAsync(where);
        }

        public virtual async Task<TEntity> FirstAsync(bool needDeleted = false)
        {
            return await GetAll(needDeleted: needDeleted).FirstAsync();
        }

        public async Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> where, bool needDeleted = false)
        {
            return await GetAll(needDeleted: needDeleted).FirstAsync(where);
        }

        public async Task<TEntity> SingleOrDefaultAsync(bool needDeleted = false)
        {
            return await GetAll(needDeleted: needDeleted).SingleOrDefaultAsync();
        }

        public async Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> where, bool needDeleted = false)
        {
            return await GetAll(needDeleted: needDeleted).SingleOrDefaultAsync(where);
        }

        public async Task<TEntity> SingleAsync(bool needDeleted = false)
        {
            return await GetAll(needDeleted: needDeleted).SingleAsync();
        }

        public async Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> where, bool needDeleted = false)
        {
            return await GetAll(needDeleted: needDeleted).SingleAsync(where);
        }

        public async Task<TResult> FirstOrDefaultAsync<TResult>(Expression<Func<TEntity, TResult>> selector, bool needDeleted = false)
        {
            return await GetAll(selector, needDeleted: needDeleted).FirstOrDefaultAsync();
        }

        public virtual async Task<TResult> FirstOrDefaultAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector, bool needDeleted = false)
        {
            return await GetMany(where, selector, needDeleted: needDeleted).FirstOrDefaultAsync();
        }

        public async Task<TResult> FirstAsync<TResult>(Expression<Func<TEntity, TResult>> selector, bool needDeleted = false)
        {
            return await GetAll(selector, needDeleted: needDeleted).FirstAsync();
        }

        public virtual async Task<TResult> FirstAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector, bool needDeleted = false)
        {
            return await GetMany(where, selector, needDeleted: needDeleted).FirstAsync();
        }

        public async Task<TResult> SingleOrDefaultAsync<TResult>(Expression<Func<TEntity, TResult>> selector, bool needDeleted = false)
        {
            return await GetAll(selector, needDeleted: needDeleted).SingleOrDefaultAsync();
        }

        public async Task<TResult> SingleOrDefaultAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector, bool needDeleted = false)
        {
            return await GetMany(where, selector, needDeleted: needDeleted).SingleOrDefaultAsync();
        }

        public async Task<TResult> SingleAsync<TResult>(Expression<Func<TEntity, TResult>> selector, bool needDeleted = false)
        {
            return await GetAll(selector, needDeleted: needDeleted).SingleAsync();
        }

        public async Task<TResult> SingleAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector, bool needDeleted = false)
        {
            return await GetMany(where, selector, needDeleted: needDeleted).SingleAsync();
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> where, bool needDeleted = false)
        {
            return await GetAll(where, needDeleted: needDeleted).AnyAsync();
        }

        public virtual async Task<bool> AnyAsync(bool needDeleted = false)
        {
            return await GetAll(needDeleted: needDeleted).AnyAsync();
        }

        public virtual async Task<bool> ExistAsync(int id)
        {
            return await AnyAsync(e => e.Id == id, needDeleted: true);
        }

        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> where = null, bool needDeleted = false)
        {
            return await (where == null ? GetAll(needDeleted: needDeleted).CountAsync() : GetAll(where, needDeleted: needDeleted).CountAsync());
        }

        public virtual IQueryable<TEntity> AsNoTracking()
        {
            return DbSet.AsNoTracking();
        }
    }
}
