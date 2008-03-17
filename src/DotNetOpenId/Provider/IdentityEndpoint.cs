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

		const string providerEndpointUrlViewStateKey = "ProviderEndpointUrl";
		[Bindable(true)]
		[Category("Behavior")]
		public string ProviderEndpointUrl {
			get { return (string)ViewState[providerEndpointUrlViewStateKey]; }
			set {
				RelyingParty.OpenIdTextBox.ValidateResolvableUrl(Page, DesignMode, value);
				ViewState[providerEndpointUrlViewStateKey] = value;
			}
		}

		const string providerLocalIdentifierViewStateKey = "ProviderLocalIdentifier";
		[Bindable(true)]
		[Category("Behavior")]
		public string ProviderLocalIdentifier {
			get { return (string)ViewState[providerLocalIdentifierViewStateKey]; }
			set {
				RelyingParty.OpenIdTextBox.ValidateResolvableUrl(Page, DesignMode, value);
				ViewState[providerLocalIdentifierViewStateKey] = value;
			}
		}

		const string xrdsUrlViewStateKey = "XrdsUrl";
		[Bindable(true)]
		[Category("Behavior")]
		public string XrdsUrl {
			get { return (string)ViewState[xrdsUrlViewStateKey]; }
			set {
				RelyingParty.OpenIdTextBox.ValidateResolvableUrl(Page, DesignMode, value);
				ViewState[xrdsUrlViewStateKey] = value;
			}
		}

		protected override void Render(HtmlTextWriter writer) {
			if (!string.IsNullOrEmpty(XrdsUrl)) {
				Page.Response.AddHeader(Yadis.Yadis.HeaderName,
					new Uri(Page.Request.Url, Page.Response.ApplyAppPathModifier(XrdsUrl)).AbsoluteUri);
			}
			if (!string.IsNullOrEmpty(XrdsUrl)) {
				writer.WriteBeginTag("meta");
				writer.WriteAttribute("http-equiv", Yadis.Yadis.HeaderName);
				writer.WriteAttribute("content", 
					new Uri(Page.Request.Url, Page.Response.ApplyAppPathModifier(XrdsUrl)).AbsoluteUri);
				writer.Write("/>");
				writer.WriteLine();
			}
			if (!string.IsNullOrEmpty(ProviderEndpointUrl)) {
				writer.WriteBeginTag("link");
				writer.WriteAttribute("rel", "openid.server");
				writer.WriteAttribute("href",
					new Uri(Page.Request.Url, Page.ResolveUrl(ProviderEndpointUrl)).AbsoluteUri);
				writer.Write(">");
				writer.WriteEndTag("link");
				writer.WriteLine();
			}
			if (!string.IsNullOrEmpty(ProviderLocalIdentifier)) {
				writer.WriteBeginTag("link");
				writer.WriteAttribute("rel", "openid.delegate");
				writer.WriteAttribute("href",
					new Uri(Page.Request.Url, Page.ResolveUrl(ProviderLocalIdentifier)).AbsoluteUri);
				writer.Write(">");
				writer.WriteEndTag("link");
				writer.WriteLine();
			}
		}
	}
}
