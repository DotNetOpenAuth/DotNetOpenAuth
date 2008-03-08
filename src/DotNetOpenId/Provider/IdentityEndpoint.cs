using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DotNetOpenId.Provider {
	[DefaultProperty("ServerUrl")]
	[ToolboxData("<{0}:IdentityEndpoint runat=server></{0}:IdentityEndpoint>")]
	public class IdentityEndpoint : WebControl {

		const string serverUrlViewStateKey = "ServerUrl";
		[Bindable(true)]
		[Category("Behavior")]
		[Localizable(true)]
		public string ServerUrl {
			get { return (string)ViewState[serverUrlViewStateKey]; }
			set {
				RelyingParty.OpenIdTextBox.ValidateResolvableUrl(Page, DesignMode, value);
				ViewState[serverUrlViewStateKey] = value;
			}
		}

		const string delegateUrlViewStateKey = "DelegateUrl";
		[Bindable(true)]
		[Category("Behavior")]
		[Localizable(true)]
		public string DelegateUrl {
			get { return (string)ViewState[delegateUrlViewStateKey]; }
			set {
				RelyingParty.OpenIdTextBox.ValidateResolvableUrl(Page, DesignMode, value);
				ViewState[delegateUrlViewStateKey] = value;
			}
		}

		protected override void Render(HtmlTextWriter writer) {
			if (!string.IsNullOrEmpty(ServerUrl)) {
				writer.WriteBeginTag("link");
				writer.WriteAttribute("rel", "openid.server");
				writer.WriteAttribute("href",
					new Uri(Page.Request.Url, Page.ResolveUrl(ServerUrl)).AbsoluteUri);
				writer.Write(">");
				writer.WriteEndTag("link");
			}
			if (!string.IsNullOrEmpty(DelegateUrl)) {
				writer.WriteBeginTag("link");
				writer.WriteAttribute("rel", "openid.delegate");
				writer.WriteAttribute("href",
					new Uri(Page.Request.Url, Page.ResolveUrl(DelegateUrl)).AbsoluteUri);
				writer.Write(">");
				writer.WriteEndTag("link");
			}
		}
	}
}
