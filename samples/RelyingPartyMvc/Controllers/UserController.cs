using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DotNetOpenId.RelyingParty;
using System.Web.Security;
using DotNetOpenId;

namespace RelyingPartyMvc.Controllers {
	public class UserController : Controller {
		public ActionResult Index() {
			if (!User.Identity.IsAuthenticated) Response.Redirect("/User/Login?ReturnUrl=Index");
			return View("Index");
		}
		public ActionResult Logout() {
			FormsAuthentication.SignOut();
			return Redirect("/Home");
		}

		public ActionResult Login() {
			// Stage 1: display login form to user
			return View("Login");
		}
		public ActionResult Authenticate() {
			var openid = new OpenIdRelyingParty();
			if (openid.Response == null) {
				// Stage 2: user submitting Identifier
				Identifier id;
				if (Identifier.TryParse(Request.Form["openid_identifier"], out id)) {
					openid.CreateRequest(Request.Form["openid_identifier"]).RedirectToProvider();
				} else {
					ViewData["Message"] = "Invalid identifier";
					return View("Login");
				}
			} else {
				// Stage 3: OpenID Provider sending assertion response
				switch (openid.Response.Status) {
					case AuthenticationStatus.Authenticated:
						FormsAuthentication.RedirectFromLoginPage(openid.Response.ClaimedIdentifier, false);
						break;
					case AuthenticationStatus.Canceled:
						ViewData["Message"] = "Canceled at provider";
						return View("Login");
					case AuthenticationStatus.Failed:
						ViewData["Message"] = openid.Response.Exception.Message;
						return View("Login");
				}
			}
			return new EmptyResult();
		}
	}
}
