namespace OpenIdRelyingPartyMvc.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Security;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.RelyingParty;

	public class UserController : Controller {
		private static OpenIdRelyingParty openid = new OpenIdRelyingParty();

		public ActionResult Index() {
			if (!User.Identity.IsAuthenticated) {
				Response.Redirect("~/User/Login?ReturnUrl=Index");
			}

			return View("Index");
		}

		public ActionResult Logout() {
			FormsAuthentication.SignOut();
			return Redirect("~/Home");
		}

		public ActionResult Login() {
			// Stage 1: display login form to user
			return View("Login");
		}

		[ValidateInput(false)]
		public async Task<ActionResult> Authenticate(string returnUrl) {
			var response = await openid.GetResponseAsync(this.Request, this.Response.ClientDisconnectedToken);
			if (response == null) {
				// Stage 2: user submitting Identifier
				Identifier id;
				if (Identifier.TryParse(Request.Form["openid_identifier"], out id)) {
					try {
						var request = await openid.CreateRequestAsync(Request.Form["openid_identifier"]);
						var redirectingResponse = await request.GetRedirectingResponseAsync(this.Response.ClientDisconnectedToken);
						Response.ContentType = redirectingResponse.Content.Headers.ContentType.ToString();
						return redirectingResponse.AsActionResult();
					} catch (ProtocolException ex) {
						ViewData["Message"] = ex.Message;
						return View("Login");
					}
				} else {
					ViewData["Message"] = "Invalid identifier";
					return View("Login");
				}
			} else {
				// Stage 3: OpenID Provider sending assertion response
				switch (response.Status) {
					case AuthenticationStatus.Authenticated:
						Session["FriendlyIdentifier"] = response.FriendlyIdentifierForDisplay;
						var cookie = FormsAuthentication.GetAuthCookie(response.ClaimedIdentifier, false);
						Response.SetCookie(cookie);
						if (!string.IsNullOrEmpty(returnUrl)) {
							return Redirect(returnUrl);
						} else {
							return RedirectToAction("Index", "Home");
						}
					case AuthenticationStatus.Canceled:
						ViewData["Message"] = "Canceled at provider";
						return View("Login");
					case AuthenticationStatus.Failed:
						ViewData["Message"] = response.Exception.Message;
						return View("Login");
				}
			}
			return new EmptyResult();
		}
	}
}
