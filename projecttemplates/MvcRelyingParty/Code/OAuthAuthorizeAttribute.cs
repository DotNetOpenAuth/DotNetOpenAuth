namespace MvcRelyingParty.Code {
	using System.Web.Mvc;
	using RelyingPartyLogic;

	public class OAuthAuthorize : AuthorizeAttribute {
		public override void OnAuthorization(AuthorizationContext filterContext) {
			var authorization = OAuthServiceProvider.ServiceProvider.ReadProtectedResourceAuthorization();
			if (authorization != null) {
				filterContext.HttpContext.User = OAuthServiceProvider.ServiceProvider.CreatePrincipal(authorization);
			} else {
				// Doesn't authorize with OAuth; defer to other schemes
				base.OnAuthorization(filterContext);
			}
		}
	}
}