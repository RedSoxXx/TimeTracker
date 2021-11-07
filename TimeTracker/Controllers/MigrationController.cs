using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using TimeTracker.App.Attributes;
using TimeTracker.DAL;
using TimeTracker.Data.Identity;

namespace TimeTracker.Controllers
{
    [AuthorizeWithPassword(Order = 0)]
    public class MigrationController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;

        public MigrationController(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public ActionResult Seed()
        {
            var sb = new StringBuilder();


            RunActionAndLogPerformance(() => DbInitializer.Seed(_userManager, _roleManager), out var logResult);
            sb.AppendLine(logResult);

            sb.AppendLine("Migration has been applied successfully.");

            return Content(sb.ToString());
        }

        private void RunActionAndLogPerformance(Expression<Action> expression, out string logResult)
        {
            var methodName = ((MethodCallExpression)expression.Body).Method.Name;
            var func = expression.Compile();

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            func();

            stopWatch.Stop();

            logResult = $"'{methodName}' operation completed. Elapsed time: {stopWatch.ElapsedMilliseconds}ms. <br/>";
        }
    }
}
