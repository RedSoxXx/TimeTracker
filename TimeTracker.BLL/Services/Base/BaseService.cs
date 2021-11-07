using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.DAL.Contracts;

namespace TimeTracker.BLL.Services.Base
{
    public class BaseService
    {
        protected readonly IUnitOfWork UnitOfWork;

        public BaseService(IUnitOfWork uow)
        {
            UnitOfWork = uow;
        }
    }
}
