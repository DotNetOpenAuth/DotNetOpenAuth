namespace OAuthAuthorizationServer.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Security.Principal;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Routing;
	using System.Web.Security;

	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.RelyingParty;

	using OAuthAuthorizationServer.Models;

	using DotNetOpenAuth.Messaging;

	[HandleError]
	public class AccountController : Controller {

		// **************************************
		// URL: /Account/LogOn
		// **************************************

		public ActionResult LogOn() {
			return View();
		}

		[HttpPost]
		public ActionResult LogOn(LogOnModel model, string returnUrl) {
			if (ModelState.IsValid) {
				var rp = new OpenIdRelyingParty();
				var request = rp.CreateRequest(model.UserSuppliedIdentifier, Realm.AutoDetect, new Uri(Request.Url, Url.Action("Authenticate")));
				if (request != null) {
					request.AddCallbackArguments("returnUrl", returnUrl);
					return request.RedirectingResponse.AsActionResult();
				} else {
					ModelState.AddModelError("", "The identifier you supplied is not recognized as a valid OpenID Identifier.");
				}
			}

			// If we got this far, something failed, redisplay form
			return View(model);
		}

		public ActionResult Authenticate(string returnUrl) {
			var rp = new OpenIdRelyingParty();
			var response = rp.GetResponse();
			if (response != null) {
				switch (response.Status) {
					case AuthenticationStatus.Authenticated:
						FormsAuthentication.SetAuthCookie(response.ClaimedIdentifier, false);
						return this.Redirect(returnUrl);
					default:
						ModelState.AddModelError("", "An error occurred during login.");
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
