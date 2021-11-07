using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.DAL.Contracts
{
    public interface IUnitOfWork
    {
        Task<int> CommitAsync();
        int Commit();
        bool AutoDetectChanges { get; set; }
    }
}
