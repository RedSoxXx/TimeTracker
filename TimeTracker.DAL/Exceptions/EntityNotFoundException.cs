using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.DAL.Exceptions
{
    public class EntityNotFoundException : InvalidOperationException
    {
        public EntityNotFoundException(string message) : base(message)
        {
        }

        public EntityNotFoundException(Type entityType, long entityId) : this($"Failed to find entity of type '{entityType}' by id '{entityId}'.")
        {
        }
    }
}
