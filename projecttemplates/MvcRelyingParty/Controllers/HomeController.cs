namespace MvcRelyingParty.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Mvc;

	[HandleError]
	public class HomeController : Controller {
		public ActionResult Index() {
			ViewData["Message"] = "Welcome to ASP.NET MVC with OpenID RP + OAuth SP support!";

			return View();
		}

		public ActionResult About() {
			return View();
		}

		public ActionResult PrivacyPolicy() {
			return View();
		}
	}
}
