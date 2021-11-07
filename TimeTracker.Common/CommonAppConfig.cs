using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Common
{
    public class CommonAppConfig
    {
        public static string CurrentVersion => "1.0.1";
        public static bool IsDebugMode =>
#if DEBUG
            true;
#else
    false;
#endif
    }
}
