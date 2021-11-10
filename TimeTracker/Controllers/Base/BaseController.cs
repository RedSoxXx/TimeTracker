using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimeTracker.Common.Extensions;
using TimeTracker.DAL.Contracts;

namespace TimeTracker.Controllers.Base
{
    public class BaseController : Controller
    {
        protected readonly IUnitOfWork UnitOfWork;

        protected long CurrentUserId
        {
            get
            {
                if (User.Identity == null || !User.Identity.IsAuthenticated)
                    return 0;

                return User.FindFirst(ClaimTypes.NameIdentifier)?.Value.AsLong() ?? 0;
            }
        }

        public BaseController(IUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
        }
    }
}
