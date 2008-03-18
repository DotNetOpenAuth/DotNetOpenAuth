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

		const string serverUrlViewStateKey = "ServerUrl";
		[Bindable(true)]
		[Category("Behavior")]
		[Localizable(true)]
		public string ServerUrl {
			get { return (string)ViewState[serverUrlViewStateKey]; }
			set {
				Consumer.OpenIdTextBox.ValidateResolvableUrl(Page, DesignMode, value);
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
				Consumer.OpenIdTextBox.ValidateResolvableUrl(Page, DesignMode, value);
				ViewState[delegateUrlViewStateKey] = value;
			}
		}

		const string xrdsUrlViewStateKey = "XrdsUrl";
		[Bindable(true)]
		[Category("Behavior")]
		public string XrdsUrl {
			get { return (string)ViewState[xrdsUrlViewStateKey]; }
			set {
				Consumer.OpenIdTextBox.ValidateResolvableUrl(Page, DesignMode, value);
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
				if (!string.IsNullOrEmpty(XrdsUrl) && XrdsAutoRedirect) {
					// Check for the presence of an accept types header that is looking
					// for the XRDS document specifically.
					if (Page.Request.AcceptTypes != null &&
						Array.IndexOf(Page.Request.AcceptTypes, Janrain.Yadis.Yadis.CONTENT_TYPE) >= 0) {
						// Redirect the caller immediately and avoid sending the whole
						// web page's contents to the client since it isn't interested
						// anyway.
						Page.Response.Redirect(new Uri(Page.Request.Url, Page.Response.ApplyAppPathModifier(XrdsUrl)).AbsoluteUri, true);
					}
				}
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
