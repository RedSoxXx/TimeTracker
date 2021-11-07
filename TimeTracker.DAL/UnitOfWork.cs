using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.DAL.Contracts;
using TimeTracker.Data.Entities.Base;

namespace TimeTracker.DAL
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly TimeTrackerDbContext _context;
        private readonly List<object> _repositories = new List<object>();

        public UnitOfWork(TimeTrackerDbContext context)
        {
            _context = context;
        }

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public int Commit()
        {
            return _context.SaveChanges();
        }

        public bool AutoDetectChanges
        {
            get => _context.ChangeTracker.AutoDetectChangesEnabled;
            set => _context.ChangeTracker.AutoDetectChangesEnabled = value;
        }

        private T GetOrFetchRepository<T, TE>(Func<T> creationFunc) where T : IBaseRepository<TE> where TE : BaseEntity
        {
            var repository = _repositories.OfType<T>().SingleOrDefault();

            if (repository == null)
            {
                repository = creationFunc();
                _repositories.Add(repository);
            }

            return repository;
        }
        #region IDisposable

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context?.Dispose();
                }
            }
            _disposed = true;
        }

        #endregion
    }
}
