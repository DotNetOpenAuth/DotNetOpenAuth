namespace OAuthAuthorizationServer.Controllers {
	using System;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Web.Mvc;
	using System.Web.Security;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using OAuthAuthorizationServer.Code;
	using OAuthAuthorizationServer.Models;

	[HandleError]
	public class AccountController : Controller {
		// **************************************
		// URL: /Account/LogOn
		// **************************************
		public ActionResult LogOn() {
			return View();
		}

		[HttpPost]
		public async Task<ActionResult> LogOn(LogOnModel model, string returnUrl) {
			if (ModelState.IsValid) {
				var rp = new OpenIdRelyingParty();
				var request = await rp.CreateRequestAsync(model.UserSuppliedIdentifier, Realm.AutoDetect, new Uri(Request.Url, Url.Action("Authenticate")));
				if (request != null) {
					if (returnUrl != null) {
						request.AddCallbackArguments("returnUrl", returnUrl);
					}

					var response = await request.GetRedirectingResponseAsync();
					Response.ContentType = response.Content.Headers.ContentType.ToString();
					return response.AsActionResult();
				} else {
					ModelState.AddModelError(string.Empty, "The identifier you supplied is not recognized as a valid OpenID Identifier.");
				}
			}

			// If we got this far, something failed, redisplay form
			return View(model);
		}

		public async Task<ActionResult> Authenticate(string returnUrl) {
			var rp = new OpenIdRelyingParty();
			var response = await rp.GetResponseAsync(Request);
			if (response != null) {
				switch (response.Status) {
					case AuthenticationStatus.Authenticated:
						// Make sure we have a user account for this guy.
						string identifier = response.ClaimedIdentifier; // convert to string so LinqToSQL expression parsing works.
						if (MvcApplication.DataContext.Users.FirstOrDefault(u => u.OpenIDClaimedIdentifier == identifier) == null) {
							MvcApplication.DataContext.Users.InsertOnSubmit(new User {
								OpenIDFriendlyIdentifier = response.FriendlyIdentifierForDisplay,
								OpenIDClaimedIdentifier = response.ClaimedIdentifier,
							});
						}

						FormsAuthentication.SetAuthCookie(response.ClaimedIdentifier, false);
						return this.Redirect(returnUrl ?? Url.Action("Index", "Home"));
					default:
						ModelState.AddModelError(string.Empty, "An error occurred during login.");
						break;
				}
			}

			return this.View("LogOn");
		}

		// **************************************
		// URL: /Account/LogOff
		// **************************************
		public ActionResult LogOff() {
			FormsAuthentication.SignOut();

			return RedirectToAction("Index", "Home");
		}
	}
}