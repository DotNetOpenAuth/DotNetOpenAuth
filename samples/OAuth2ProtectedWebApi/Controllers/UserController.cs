namespace OAuth2ProtectedWebApi.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Security.Principal;
	using System.Threading.Tasks;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Security;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2;
	using DotNetOpenAuth.OAuth2.Messages;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using OAuth2ProtectedWebApi.Code;

	public class UserController : Controller {
		[Authorize]
		[HttpGet]
		[HttpHeader("x-frame-options", "SAMEORIGIN")] // mitigates clickjacking
		public async Task<ActionResult> Authorize() {
			var authServer = new AuthorizationServer(new AuthorizationServerHost());
			var authRequest = await authServer.ReadAuthorizationRequestAsync(this.Request);
			this.ViewData["scope"] = authRequest.Scope;
			this.ViewData["request"] = this.Request.Url;
			return View();
		}

		[Authorize]
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> Respond(string request, bool approval) {
			var authServer = new AuthorizationServer(new AuthorizationServerHost());
			var authRequest = await authServer.ReadAuthorizationRequestAsync(new Uri(request));
			IProtocolMessage responseMessage;
			if (approval) {
				var grantedResponse = authServer.PrepareApproveAuthorizationRequest(
					authRequest, this.User.Identity.Name, authRequest.Scope);
				responseMessage = grantedResponse;
			} else {
				var rejectionResponse = authServer.PrepareRejectAuthorizationRequest(authRequest);
				rejectionResponse.Error = Protocol.EndUserAuthorizationRequestErrorCodes.AccessDenied;
				responseMessage = rejectionResponse;
			}

			var response = await authServer.Channel.PrepareResponseAsync(responseMessage);
			Response.ContentType = response.Content.Headers.ContentType.ToString();
			return response.AsActionResult();
		}

		public async Task<ActionResult> Login(string returnUrl) {
			var rp = new OpenIdRelyingParty(null);
			Realm officialWebSiteHome = Realm.AutoDetect;
			Uri returnTo = new Uri(this.Request.Url, this.Url.Action("Authenticate"));
			var request = await rp.CreateRequestAsync(WellKnownProviders.Google, officialWebSiteHome, returnTo);
			if (returnUrl != null) {
				request.SetUntrustedCallbackArgument("returnUrl", returnUrl);
			}

			var redirectingResponse = await request.GetRedirectingResponseAsync();
			Response.ContentType = redirectingResponse.Content.Headers.ContentType.ToString();
			return redirectingResponse.AsActionResult();
		}

		public async Task<ActionResult> Authenticate() {
			var rp = new OpenIdRelyingParty(null);
			var response = await rp.GetResponseAsync(this.Request);
			if (response != null) {
				if (response.Status == AuthenticationStatus.Authenticated) {
					FormsAuthentication.SetAuthCookie(response.ClaimedIdentifier, false);
					return this.Redirect(FormsAuthentication.GetRedirectUrl(response.ClaimedIdentifier, false));
				}
			}

			return this.RedirectToAction("Index", "Home");
		}
	}
}