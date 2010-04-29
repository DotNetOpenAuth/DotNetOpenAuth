namespace MvcRelyingParty.Code {
	using System.Web.Mvc;
	using DotNetOpenAuth.Messaging;
	using RelyingPartyLogic;

	public abstract class OAuthEndpointAttribute : ActionFilterAttribute {
		internal MessageReceivingEndpoint GetRequestAsEndpoint(ActionExecutingContext filterContext) {
			return new MessageReceivingEndpoint(
				filterContext.HttpContext.Request.Url,
				MessagingUtilities.GetHttpDeliveryMethod(filterContext.HttpContext.Request.HttpMethod));
		}
	}

	public class OAuthRequestTokenEndpoint : OAuthEndpointAttribute {
		public override void OnActionExecuting(ActionExecutingContext filterContext) {
			var endpoint = GetRequestAsEndpoint(filterContext);
			OAuthServiceProvider.RequestTokenEndpoint = endpoint;
			base.OnActionExecuting(filterContext);
		}
	}

	public class OAuthAccessTokenEndpoint : OAuthEndpointAttribute {
		public override void OnActionExecuting(ActionExecutingContext filterContext) {
			var endpoint = GetRequestAsEndpoint(filterContext);
			OAuthServiceProvider.AccessTokenEndpoint = endpoint;
			base.OnActionExecuting(filterContext);
		}
	}

	public class OAuthUserAuthorizationEndpoint : OAuthEndpointAttribute {
		public override void OnActionExecuting(ActionExecutingContext filterContext) {
			var endpoint = GetRequestAsEndpoint(filterContext);
			OAuthServiceProvider.UserAuthorizationEndpoint = endpoint;
			base.OnActionExecuting(filterContext);
		}
	}
}
