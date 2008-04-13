using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DotNetOpenId.RelyingParty;
using System.Web.Security;

namespace RelyingPartyMvc.Controllers {
	public class UserController : Controller {
		public void Index() {
			if (!User.Identity.IsAuthenticated) Response.Redirect("/User/Login?ReturnUrl=Index");
			RenderView("Index");
		}
		public void Logout() {
			FormsAuthentication.SignOut();
			Response.Redirect("/Home");
		}

		public void Login() {
			// Stage 1: display login form to user
			RenderView("Login");
		}
		public void Authenticate() {
			var openid = new OpenIdRelyingParty();
			if (openid.Response == null) {
				// Stage 2: user submitting Identifier
				openid.CreateRequest(Request.Form["openid_identifier"]).RedirectToProvider();
			} else {
				// Stage 3: OpenID Provider sending assertion response
				switch (openid.Response.Status) {
					case AuthenticationStatus.Authenticated:
						FormsAuthentication.RedirectFromLoginPage(openid.Response.ClaimedIdentifier, false);
						break;
					case AuthenticationStatus.Canceled:
						ViewData["Message"] = "Canceled at provider";
						RenderView("Login");
						break;
					case AuthenticationStatus.Failed:
						ViewData["Message"] = openid.Response.Exception.Message;
						RenderView("Login");
						break;
				}
			}
		}
	}
}
