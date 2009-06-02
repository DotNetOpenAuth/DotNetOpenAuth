namespace OpenIdProviderMvc.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Mvc.Ajax;

	public class UserController : Controller {
		public ActionResult PpidIdentity() {
			if (Request.AcceptTypes.Contains("application/xrds+xml")) {
				return View("Xrds");
			}

			return View();
		}

		public ActionResult Identity(string id) {
			var redirect = this.RedirectIfNotNormalizedRequestUri();
			if (redirect != null) {
				return redirect;
			}

			if (Request.AcceptTypes.Contains("application/xrds+xml")) {
				return View("Xrds");
			}

			this.ViewData["username"] = id;
			return View();
		}

		public ActionResult Xrds(string id) {
			return View();
		}

		private ActionResult RedirectIfNotNormalizedRequestUri() {
			Uri normalized = Models.User.GetNormalizedClaimedIdentifier(Request.Url);
			if (Request.Url != normalized) {
				return Redirect(normalized.AbsoluteUri);
			}

			return null;
		}
	}
}
