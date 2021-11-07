using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Data.Entities.Base;

namespace TimeTracker.Data.Extensions
{
    public static class EntityExtensions
    {
        public static bool IsNew(this IEntity self)
        {
            return self.Id == 0;
        }
    }
}
