namespace OAuth2ProtectedWebApi.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Security.Principal;
	using System.Threading.Tasks;
	using System.Web;
	using System.Web.Mvc;

	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2;
	using DotNetOpenAuth.OAuth2.Messages;
	using OAuth2ProtectedWebApi.Code;

	// [Authorize]
	public class UserController : Controller {
		[HttpGet]
		[HttpHeader("x-frame-options", "SAMEORIGIN")] // mitigates clickjacking
		public async Task<ActionResult> Authorize() {
			var authServer = new AuthorizationServer(new AuthorizationServerHost());
			var authRequest = await authServer.ReadAuthorizationRequestAsync(this.Request);
			this.ViewData["scope"] = authRequest.Scope;
			this.ViewData["request"] = this.Request.Url;
			return View();
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<ActionResult> Respond(string request, bool approval) {
			System.Web.HttpContext.Current.User = new GenericPrincipal(new GenericIdentity("Andrew"), new string[0]);
			var authServer = new AuthorizationServer(new AuthorizationServerHost());
			var httpInfo = HttpRequestInfo.Create(HttpMethod.Get.Method, new Uri(request));
			var authRequest = await authServer.ReadAuthorizationRequestAsync(httpInfo);
			IProtocolMessage responseMessage;
			if (approval) {
				responseMessage = authServer.PrepareApproveAuthorizationRequest(
					authRequest, this.User.Identity.Name, authRequest.Scope);
			} else {
				responseMessage = authServer.PrepareRejectAuthorizationRequest(authRequest);
			}

			var response = await authServer.Channel.PrepareResponseAsync(responseMessage);
			return response.AsActionResult();
		}
	}
}
