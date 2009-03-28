namespace OpenIdRelyingPartyMvc.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Mvc;

	public class HomeController : Controller {
		public ActionResult Index() {
			Response.AppendHeader(
				"X-XRDS-Location",
				new Uri(Request.Url, Response.ApplyAppPathModifier("~/Home/xrds")).AbsoluteUri);
			return View("Index");
		}

		public ActionResult Xrds() {
			return View("Xrds");
		}
	}
}
