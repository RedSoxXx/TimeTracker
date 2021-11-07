using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Common.Extensions
{
    public static class ExceptionExtensions
    {
        public static string GetFinalMessage(this Exception self)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));

            return self.GetFinalException().Message;
        }

        public static Exception GetFinalException(this Exception self)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));

            if (self.InnerException == null)
                return self;

            return self.GetAllInnerExceptions().Last();
        }

        public static IEnumerable<Exception> GetAllInnerExceptions(this Exception self)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));

            var result = new List<Exception>();

            var last = self;
            while (last.InnerException != null)
            {
                last = last.InnerException;
                result.Add(last);
            }

            return result.ToArray();
        }
    }
}
