namespace MvcRelyingParty.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Security.Principal;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Security;
	using System.Web.UI;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using RelyingPartyLogic;
	using DotNetOpenAuth.OpenId;

	[HandleError]
	public class AccountController : Controller {
		internal static OpenIdRelyingParty relyingParty = new OpenIdRelyingParty();

		// This constructor is used by the MVC framework to instantiate the controller using
		// the default forms authentication and membership providers.

		public AccountController()
			: this(null) {
		}

		// This constructor is not used by the MVC framework but is instead provided for ease
		// of unit testing this type. See the comments at the end of this file for more
		// information.
		public AccountController(IFormsAuthentication formsAuth) {
			FormsAuth = formsAuth ?? new FormsAuthenticationService();
		}

		public IFormsAuthentication FormsAuth { get; private set; }

		public Realm Realm {
			get {
				UriBuilder builder = new UriBuilder(Request.Url);
				builder.Path = Request.ApplicationPath;
				return builder.Uri;
			}
		}
		public Uri ReturnTo {
			get { return new Uri(Request.Url, Url.Action("LogOnReturnTo")); }
		}

		public ActionResult LogOn() {
			return View();
		}

		[AcceptVerbs(HttpVerbs.Post), ValidateAntiForgeryToken]
		public ActionResult LogOn(string openid_identifier, bool rememberMe, string returnUrl) {
			try {
				var request = relyingParty.CreateRequest(openid_identifier, this.Realm, this.ReturnTo);
				request.SetUntrustedCallbackArgument("rememberMe", rememberMe ? "1" : "0");

				// This might be signed so the OP can't send the user to a dangerous URL.
				// Of course, if that itself was a danger then the site is vulnerable to XSRF attacks anyway.
				if (!string.IsNullOrEmpty(returnUrl)) {
					request.SetUntrustedCallbackArgument("returnUrl", returnUrl);
				}

				// Ask for the user's email, not because we necessarily need it to do our work,
				// but so we can display something meaningful to the user as their "username"
				// when they log in with a PPID from Google, for example.
				request.AddExtension(new ClaimsRequest {
					Email = DemandLevel.Require,
					FullName = DemandLevel.Request,
				});

				return request.RedirectingResponse.AsActionResult();
			} catch (ProtocolException ex) {
				ModelState.AddModelError("OpenID", ex.Message);
				return View();
			}
		}

		[AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
		public ActionResult LogOnReturnTo() {
			var response = relyingParty.GetResponse();
			if (response != null) {
				switch (response.Status) {
					case AuthenticationStatus.Authenticated:
						bool rememberMe = response.GetUntrustedCallbackArgument("rememberMe") == "1";
						FormsAuth.SignIn(response.ClaimedIdentifier, rememberMe);
						string returnUrl = response.GetCallbackArgument("returnUrl");
						if (!String.IsNullOrEmpty(returnUrl)) {
							return Redirect(returnUrl);
						} else {
							return RedirectToAction("Index", "Home");
						}
						break;
					case AuthenticationStatus.Canceled:
						ModelState.AddModelError("OpenID", "It looks like you canceled login at your OpenID Provider.");
						break;
					case AuthenticationStatus.Failed:
						ModelState.AddModelError("OpenID", response.Exception.Message);
						break;
				}
			}

			return View("LogOn");
		}

		public ActionResult LogOff() {
			FormsAuth.SignOut();
			return RedirectToAction("Index", "Home");
		}
	}

	// The FormsAuthentication type is sealed and contains static members, so it is difficult to
	// unit test code that calls its members. The interface and helper class below demonstrate
	// how to create an abstract wrapper around such a type in order to make the AccountController
	// code unit testable.

	public interface IFormsAuthentication {
		void SignIn(string claimedIdentifier, bool createPersistentCookie);
		void SignOut();
	}

	public class FormsAuthenticationService : IFormsAuthentication {
		public void SignIn(string claimedIdentifier, bool createPersistentCookie) {
			FormsAuthentication.SetAuthCookie(claimedIdentifier, createPersistentCookie);
		}

		public void SignOut() {
			FormsAuthentication.SignOut();
		}
	}
}
