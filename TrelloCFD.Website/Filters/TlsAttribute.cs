using System;
using System.Web.Mvc;

namespace TrelloCFD.Website.Filters
{
    public class TlsAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var request = filterContext.HttpContext.Request;

            if (request.IsSecureConnection)
            {
                filterContext.HttpContext.Response.AddHeader("Strict-Transport-Security", "max-age=15552000");
            }
            else if (!request.IsLocal && request.Headers["Upgrade-Insecure-Requests"] == "1")
            {
                var url = new Uri("https://" + request.Url.GetComponents(UriComponents.Host | UriComponents.PathAndQuery, UriFormat.Unescaped), UriKind.Absolute);
                filterContext.Result = new RedirectResult(url.AbsoluteUri);
            }
        }
    }
}