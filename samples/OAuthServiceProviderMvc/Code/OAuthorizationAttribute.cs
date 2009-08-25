using System.Collections.Generic;
using System.Net;
using System.Net.Mime;
using System.Web.Routing;
using System.Web.Mvc;
using System.Web;

namespace oAuthMVC
{
    /// <summary>
    /// Summary description for OAuthorizationAttribute
    /// </summary>
    public class OAuthorizationAttribute : ActionFilterAttribute, IExceptionFilter
    {
        public OAuthorizationAttribute()
            : base()
        {

        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!OAuthAuthorizationManager.CheckAccess())
                throw new HttpException((int)HttpStatusCode.Forbidden, "You must provide a valid oAuth token to access this resource.");

            base.OnActionExecuting(filterContext);
        }

        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled)
            {
                return;
            }

            throw new HttpException((int)HttpStatusCode.Forbidden, "This resource requires authorization.");
        }
    }
}