namespace OpenIdProviderMvc.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Mvc;
	using System.Web.Mvc.Ajax;

	public class UserController : Controller {
		/// <summary>
		/// Identities the specified id.
		/// </summary>
		/// <param name="id">The username or anonymous identifier.</param>
		/// <param name="anon">if set to <c>true</c> then <paramref name="id"/> represents an anonymous identifier rather than a username.</param>
		/// <returns>The view to display.</returns>
		public ActionResult Identity(string id, bool anon) {
			if (!anon) {
				var redirect = this.RedirectIfNotNormalizedRequestUri(id);
				if (redirect != null) {
					return redirect;
				}
			}

			if (Request.AcceptTypes != null && Request.AcceptTypes.Contains("application/xrds+xml")) {
				return View("Xrds");
			}

			if (!anon) {
				this.ViewData["username"] = id;
			}

			return View();
		}

		public ActionResult Xrds(string id) {
			return View();
		}

		private ActionResult RedirectIfNotNormalizedRequestUri(string user) {
			Uri normalized = Models.User.GetClaimedIdentifierForUser(user);
			if (Request.Url != normalized) {
				return Redirect(normalized.AbsoluteUri);
			}

			return null;
		}
	}
}
