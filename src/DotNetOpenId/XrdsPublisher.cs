using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DotNetOpenId {
	/// <summary>
	/// The locations the YADIS protocol describes can contain a reference
	/// to an XRDS document.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Xrds"), Flags]
	public enum XrdsUrlLocations {
		/// <summary>
		/// Indicates XRDS document referencing from an HTTP protocol header (outside the HTML).
		/// </summary>
		HttpHeader = 0x1,
		/// <summary>
		/// Indicates XRDS document referencing from within an HTML page's &lt;HEAD&gt; tag.
		/// </summary>
		HtmlMeta = 0x2,
		/// <summary>
		/// Indicates XRDS document referencing in both HTTP headers and HTML HEAD tags.
		/// </summary>
		Both = 0x3
	}

	/// <summary>
	/// An ASP.NET control that advertises an XRDS document and even responds to specially
	/// crafted requests to retrieve it.
	/// </summary>
	[DefaultProperty("XrdsLocation")]
	[ToolboxData("<{0}:XrdsPublisher runat=server></{0}:XrdsPublisher>")]
	public class XrdsPublisher : Control {
		#region Properties
		const string xrdsUrlViewStateKey = "XrdsUrl";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Xrds"), Bindable(true)]
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Xrds"), Bindable(true)]
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Xrds"), Bindable(true)]
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

		const bool enabledDefault = true;
		const string enabledViewStateKey = "Enabled";
		[Category("Behavior")]
		[DefaultValue(enabledDefault)]
		public bool Enabled {
			get {
				return ViewState[enabledViewStateKey] == null ?
				enabledDefault : (bool)ViewState[enabledViewStateKey];
			}
			set { ViewState[enabledViewStateKey] = value; }
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
						Array.IndexOf(Page.Request.AcceptTypes, DotNetOpenId.Yadis.ContentTypes.Xrds) >= 0) {
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

		/// <summary>
		/// Renders the HTTP Header and/or HTML HEAD tags.
		/// </summary>
		/// <param name="writer"></param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings")]
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
		}
	}
}
