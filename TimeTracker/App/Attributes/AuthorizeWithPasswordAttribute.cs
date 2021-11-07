using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TimeTracker.Common;

namespace TimeTracker.App.Attributes
{
    public class AuthorizeWithPasswordAttribute : ActionFilterAttribute
    {
        public string Password { get; set; } = CommonAppConfig.IsDebugMode ? "123" : "97dcf9ec52814379-8dfdff840a77fa50";

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            if (!request.Query.TryGetValue("pwd", out var pwd) || pwd != Password)
                filterContext.Result = new ContentResult { Content = "Invalid password!" };
        }
    }
}
