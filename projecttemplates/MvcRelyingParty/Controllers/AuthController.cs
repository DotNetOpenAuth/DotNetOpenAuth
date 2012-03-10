//-----------------------------------------------------------------------
// <copyright file="AuthController.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MvcRelyingParty.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Web;
	using System.Web.Mvc;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using RelyingPartyLogic;

	public class AuthController : Controller {
		/// <summary>
		/// Initializes a new instance of the <see cref="AuthController"/> class.
		/// </summary>
		/// <remarks>
		/// This constructor is used by the MVC framework to instantiate the controller using
		/// the default forms authentication and OpenID services.
		/// </remarks>
		public AuthController()
			: this(null, null) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthController"/> class.
		/// </summary>
		/// <param name="formsAuth">The forms auth.</param>
		/// <param name="relyingParty">The relying party.</param>
		/// <remarks>
		/// This constructor is not used by the MVC framework but is instead provided for ease
		/// of unit testing this type. 
		/// </remarks>
		public AuthController(IFormsAuthentication formsAuth, IOpenIdRelyingParty relyingParty) {
			this.FormsAuth = formsAuth ?? new FormsAuthenticationService();
			this.RelyingParty = relyingParty ?? new OpenIdRelyingPartyService();
		}

		/// <summary>
		/// Gets the forms authentication module to use.
		/// </summary>
		public IFormsAuthentication FormsAuth { get; private set; }

		/// <summary>
		/// Gets the OpenID relying party to use for logging users in.
		/// </summary>
		public IOpenIdRelyingParty RelyingParty { get; private set; }

		private Uri PrivacyPolicyUrl {
			get {
				return Url.ActionFull("PrivacyPolicy", "Home");
			}
		}

		/// <summary>
		/// Performs discovery on a given identifier.
		/// </summary>
		/// <param name="identifier">The identifier on which to perform discovery.</param>
		/// <returns>The JSON result of discovery.</returns>
		public ActionResult Discover(string identifier) {
			if (!this.Request.IsAjaxRequest()) {
				throw new InvalidOperationException();
			}

			return this.RelyingParty.AjaxDiscovery(
				identifier,
				Realm.AutoDetect,
				Url.ActionFull("PopUpReturnTo"),
				this.PrivacyPolicyUrl);
		}

		/// <summary>
		/// Prepares a web page to help the user supply his login information.
		/// </summary>
		/// <returns>The action result.</returns>
		public ActionResult LogOn() {
			this.PreloadDiscoveryResults();
			return View();
		}

		/// <summary>
		/// Prepares a web page to help the user supply his login information.
		/// </summary>
		/// <returns>The action result.</returns>
		public ActionResult LogOnPopUp() {
			this.PreloadDiscoveryResults();
			return View();
		}

		/// <summary>
		/// Handles the positive assertion that comes from Providers to Javascript running in the browser.
		/// </summary>
		/// <returns>The action result.</returns>
		/// <remarks>
		/// This method instructs ASP.NET MVC to <i>not</i> validate input
		/// because some OpenID positive assertions messages otherwise look like
		/// hack attempts and result in errors when validation is turned on.
		/// </remarks>
		[AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post), ValidateInput(false)]
		public ActionResult PopUpReturnTo() {
			return this.RelyingParty.ProcessAjaxOpenIdResponse();
		}

		/// <summary>
		/// Handles the positive assertion that comes from Providers.
		/// </summary>
		/// <param name="openid_openidAuthData">The positive assertion obtained via AJAX.</param>
		/// <returns>The action result.</returns>
		/// <remarks>
		/// This method instructs ASP.NET MVC to <i>not</i> validate input
		/// because some OpenID positive assertions messages otherwise look like
		/// hack attempts and result in errors when validation is turned on.
		/// </remarks>
		[AcceptVerbs(HttpVerbs.Post), ValidateInput(false)]
		public ActionResult LogOnPostAssertion(string openid_openidAuthData) {
			IAuthenticationResponse response;
			if (!string.IsNullOrEmpty(openid_openidAuthData)) {
				// Always say it's a GET since the payload is all in the URL, even the large ones.
				var auth = new Uri(openid_openidAuthData);
				HttpRequestBase clientResponseInfo = HttpRequestInfo.Create("GET", auth, headers: Request.Headers);
				response = this.RelyingParty.GetResponse(clientResponseInfo);
			} else {
				response = this.RelyingParty.GetResponse();
			}
			if (response != null) {
				switch (response.Status) {
					case AuthenticationStatus.Authenticated:
						var token = RelyingPartyLogic.User.ProcessUserLogin(response);
						this.FormsAuth.SignIn(token.ClaimedIdentifier, false);
						string returnUrl = Request.Form["returnUrl"];
						if (!string.IsNullOrEmpty(returnUrl)) {
							return Redirect(returnUrl);
						} else {
							return RedirectToAction("Index", "Home");
						}
					case AuthenticationStatus.Canceled:
						ModelState.AddModelError("OpenID", "It looks like you canceled login at your OpenID Provider.");
						break;
					case AuthenticationStatus.Failed:
						ModelState.AddModelError("OpenID", response.Exception.Message);
						break;
				}
			}

			// If we're to this point, login didn't complete successfully.
			// Show the LogOn view again to show the user any errors and
			// give another chance to complete login.
			return View("LogOn");
		}

		[Authorize, AcceptVerbs(HttpVerbs.Post), ValidateAntiForgeryToken, ValidateInput(false)]
		public ActionResult AddAuthenticationToken(string openid_openidAuthData) {
			IAuthenticationResponse response;
			if (!string.IsNullOrEmpty(openid_openidAuthData)) {
				var auth = new Uri(openid_openidAuthData);
				var headers = new WebHeaderCollection();
				foreach (string header in Request.Headers) {
					headers[header] = Request.Headers[header];
				}

				// Always say it's a GET since the payload is all in the URL, even the large ones.
				HttpRequestBase clientResponseInfo = HttpRequestInfo.Create("GET", auth, headers: headers);
				response = this.RelyingParty.GetResponse(clientResponseInfo);
			} else {
				response = this.RelyingParty.GetResponse();
			}
			if (response != null) {
				switch (response.Status) {
					case AuthenticationStatus.Authenticated:
						string identifierString = response.ClaimedIdentifier;
						var existing = Database.DataContext.AuthenticationTokens.Include("User").FirstOrDefault(token => token.ClaimedIdentifier == identifierString);
						if (existing == null) {
							Database.LoggedInUser.AuthenticationTokens.Add(new AuthenticationToken {
								ClaimedIdentifier = response.ClaimedIdentifier,
								FriendlyIdentifier = response.FriendlyIdentifierForDisplay,
							});
							Database.DataContext.SaveChanges();
						} else {
							if (existing.User != Database.LoggedInUser) {
								// The supplied token is already bound to a different user account.
								// TODO: communicate the problem to the user.
							}
						}
						break;
					default:
						break;
				}
			}

			return RedirectToAction("Edit", "Account");
		}

		/// <summary>
		/// Logs the user out of the site and redirects the browser to our home page.
		/// </summary>
		/// <returns>The action result.</returns>
		public ActionResult LogOff() {
			this.FormsAuth.SignOut();
			return RedirectToAction("Index", "Home");
		}

		/// <summary>
		/// Preloads discovery results for the OP buttons we display on the selector in the ViewData.
		/// </summary>
		private void PreloadDiscoveryResults() {
			this.ViewData["PreloadedDiscoveryResults"] = this.RelyingParty.PreloadDiscoveryResults(
				Realm.AutoDetect,
				Url.ActionFull("PopUpReturnTo"),
				this.PrivacyPolicyUrl,
				"https://me.yahoo.com/",
				"https://www.google.com/accounts/o8/id");
		}
	}
}
