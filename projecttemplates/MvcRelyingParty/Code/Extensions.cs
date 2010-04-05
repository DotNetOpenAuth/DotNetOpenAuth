namespace MvcRelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using System.Web.Mvc;

	internal static class Extensions {
		internal static Uri ActionFull(this UrlHelper urlHelper, string actionName) {
			return new Uri(HttpContext.Current.Request.Url, urlHelper.Action(actionName));
		}

		internal static Uri ActionFull(this UrlHelper urlHelper, string actionName, string controllerName) {
			return new Uri(HttpContext.Current.Request.Url, urlHelper.Action(actionName, controllerName));
		}
	}
}
