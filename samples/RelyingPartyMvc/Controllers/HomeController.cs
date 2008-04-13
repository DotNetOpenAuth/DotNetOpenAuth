using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RelyingPartyMvc.Controllers {
	public class HomeController : Controller {
		public void Index() {
			Response.AppendHeader("X-XRDS-Location",
				new Uri(Request.Url, Response.ApplyAppPathModifier("~/Home/xrds")).AbsoluteUri);
			RenderView("Index");
		}
		public void Xrds() { RenderView("Xrds"); }
	}
}
