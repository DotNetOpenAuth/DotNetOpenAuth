using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OpenIdProviderWebForms {
	public partial class WebForm1 : System.Web.UI.Page {
		protected async void Page_Load(object sender, EventArgs e) {
			object oldValue = Session["Hi"];
			Session["Hi"] = new object();
		}
	}
}