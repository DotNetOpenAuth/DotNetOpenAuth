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
		[Flags]
		public enum XrdsUrlLocations {
			HttpHeader = 0x1,
			HtmlMeta = 0x2,
			Both = 0x3
		}

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

		const XrdsUrlLocations xrdsAdvertisementDefault = XrdsUrlLocations.HttpHeader;
		const string xrdsAdvertisementViewStateKey = "XrdsAdvertisement";
		[Bindable(true)]
		[Category("Behavior")]
		[DefaultValue(xrdsAdvertisementDefault)]
		[Description("Where the XRDS document URL is advertised in the web response.")]
		public XrdsUrlLocations XrdsAdvertisement {
			get {
				return ViewState[xrdsAdvertisementViewStateKey] == null ?
					xrdsAdvertisementDefault : (XrdsUrlLocations)ViewState[xrdsAdvertisementViewStateKey];
			}
			set { ViewState[xrdsAdvertisementViewStateKey] = value; }
		}

		const bool xrdsAutoRedirectDefault = true;
		const string xrdsAutoRedirectViewStateKey = "XrdsAutoRedirect";
		[Bindable(true)]
		[Category("Behavior")]
		[DefaultValue(xrdsAutoRedirectDefault)]
		[Description("Whether XRDS requests should be immediately redirected to the XRDS document.")]
		public bool XrdsAutoRedirect {
			get {
				return ViewState[xrdsAutoRedirectViewStateKey] == null ?
					xrdsAutoRedirectDefault : (bool)ViewState[xrdsAutoRedirectViewStateKey];
			}
			set { ViewState[xrdsAutoRedirectViewStateKey] = value; }
		}

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			if (!Page.IsPostBack) {
				if (!string.IsNullOrEmpty(XrdsUrl) && XrdsAutoRedirect) {
					// Check for the presence of an accept types header that is looking
					// for the XRDS document specifically.
					if (Page.Request.AcceptTypes != null &&
						Array.IndexOf(Page.Request.AcceptTypes, DotNetOpenId.Yadis.ContentType.Xrds) >= 0) {
						// Redirect the caller immediately and avoid sending the whole
						// web page's contents to the client since it isn't interested
						// anyway.
						Page.Response.Redirect(new Uri(Page.Request.Url, Page.Response.ApplyAppPathModifier(XrdsUrl)).AbsoluteUri, true);
					}
				}
			}
		}

		protected override void Render(HtmlTextWriter writer) {
			if (!string.IsNullOrEmpty(XrdsUrl)) {
				if ((XrdsAdvertisement & XrdsUrlLocations.HttpHeader) != 0) {
					Page.Response.AddHeader(Yadis.Yadis.HeaderName,
						new Uri(Page.Request.Url, Page.Response.ApplyAppPathModifier(XrdsUrl)).AbsoluteUri);
				}
				if ((XrdsAdvertisement & XrdsUrlLocations.HtmlMeta) != 0) {
					writer.WriteBeginTag("meta");
					writer.WriteAttribute("http-equiv", Yadis.Yadis.HeaderName);
					writer.WriteAttribute("content",
						new Uri(Page.Request.Url, Page.Response.ApplyAppPathModifier(XrdsUrl)).AbsoluteUri);
					writer.Write("/>");
					writer.WriteLine();
				}
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
