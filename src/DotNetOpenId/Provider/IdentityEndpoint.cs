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

		#region Properties
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

		const bool xrdsAutoAnswerDefault = true;
		const string xrdsAutoAnswerViewStateKey = "XrdsAutoAnswer";
		[Bindable(true)]
		[Category("Behavior")]
		[DefaultValue(xrdsAutoAnswerDefault)]
		[Description("Whether XRDS requests should be immediately answered with the XRDS document if it is served by this web application.")]
		public bool XrdsAutoAnswer {
			get {
				return ViewState[xrdsAutoAnswerViewStateKey] == null ?
					xrdsAutoAnswerDefault : (bool)ViewState[xrdsAutoAnswerViewStateKey];
			}
			set { ViewState[xrdsAutoAnswerViewStateKey] = value; }
		}
		#endregion

		#region Properties to hide
		[Browsable(false), Bindable(false)]
		public override string AccessKey {
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override string CssClass {
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override bool EnableViewState {
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override bool Enabled {
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override System.Drawing.Color ForeColor {
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override System.Drawing.Color BackColor {
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override System.Drawing.Color BorderColor {
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override Unit BorderWidth {
			get { return Unit.Empty; }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override BorderStyle BorderStyle {
			get { return BorderStyle.None; }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override FontInfo Font {
			get { return null; }
		}
		[Browsable(false), Bindable(false)]
		public override Unit Height {
			get { return Unit.Empty; }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override Unit Width {
			get { return Unit.Empty; }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override short TabIndex {
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override string ToolTip {
			get { return string.Empty; }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override string SkinID {
			get { return string.Empty; }
			set { throw new NotSupportedException(); }
		}
		[Browsable(false), Bindable(false)]
		public override bool EnableTheming {
			get { return false; }
			set { throw new NotSupportedException(); }
		}
		#endregion

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			if (!Page.IsPostBack) {
				if (XrdsAutoAnswer && !string.IsNullOrEmpty(XrdsUrl) &&
					XrdsUrl.StartsWith("~/", StringComparison.Ordinal)) {
					// Check for the presence of an accept types header that is looking
					// for the XRDS document specifically.
					if (Page.Request.AcceptTypes != null &&
						Array.IndexOf(Page.Request.AcceptTypes, DotNetOpenId.Yadis.ContentType.Xrds) >= 0) {
						// Respond to the caller immediately with an XRDS document
						// and avoid sending the whole web page's contents to the 
						// client since it isn't interested anyway.
						Page.Server.Transfer(XrdsUrl);
						// We do NOT simply send a 301 redirect here because that would
						// alter the Claimed Identifier.
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
