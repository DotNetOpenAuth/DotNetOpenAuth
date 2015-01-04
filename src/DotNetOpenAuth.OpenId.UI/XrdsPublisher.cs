//-----------------------------------------------------------------------
// <copyright file="XrdsPublisher.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth {
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Drawing.Design;
	using System.Text;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Yadis;

	/// <summary>
	/// The locations the YADIS protocol describes can contain a reference
	/// to an XRDS document.
	/// </summary>
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Xrds", Justification = "Correct spelling")]
	[Flags]
	public enum XrdsUrlLocations {
		/// <summary>
		/// The XRDS document should not be advertised anywhere.
		/// </summary>
		/// <remarks>
		/// When the XRDS document is not referenced from anywhere,
		/// the XRDS content is only available when 
		/// <see cref="XrdsPublisher.XrdsAutoAnswer"/> is <c>true</c> 
		/// and the discovering client includes an 
		/// "Accept: application/xrds+xml" HTTP header.
		/// </remarks>
		None = 0x0,

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
		Both = 0x3,
	}

	/// <summary>
	/// An ASP.NET control that advertises an XRDS document and even responds to specially
	/// crafted requests to retrieve it.
	/// </summary>
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Xrds", Justification = "Correct spelling")]
	[DefaultProperty("XrdsLocation")]
	[ToolboxData("<{0}:XrdsPublisher runat=server></{0}:XrdsPublisher>")]
	public class XrdsPublisher : Control {
		/// <summary>
		/// The view state key to ues for storing the value of the <see cref="XrdsUrl"/> property.
		/// </summary>
		private const string XrdsUrlViewStateKey = "XrdsUrl";

		/// <summary>
		/// The default value for the <see cref="XrdsAdvertisement"/> property.
		/// </summary>
		private const XrdsUrlLocations XrdsAdvertisementDefault = XrdsUrlLocations.HttpHeader;

		/// <summary>
		/// The view state key to ues for storing the value of the <see cref="XrdsAdvertisement"/> property.
		/// </summary>
		private const string XrdsAdvertisementViewStateKey = "XrdsAdvertisement";

		/// <summary>
		/// The default value for the <see cref="XrdsAutoAnswer"/> property.
		/// </summary>
		private const bool XrdsAutoAnswerDefault = true;

		/// <summary>
		/// The view state key to ues for storing the value of the <see cref="XrdsAutoAnswer"/> property.
		/// </summary>
		private const string XrdsAutoAnswerViewStateKey = "XrdsAutoAnswer";

		/// <summary>
		/// The default value for the <see cref="Enabled"/> property.
		/// </summary>
		private const bool EnabledDefault = true;

		/// <summary>
		/// The view state key to ues for storing the value of the <see cref="Enabled"/> property.
		/// </summary>
		private const string EnabledViewStateKey = "Enabled";

		/// <summary>
		/// Initializes a new instance of the <see cref="XrdsPublisher"/> class.
		/// </summary>
		public XrdsPublisher() {
			Reporting.RecordFeatureUse(this);
		}

		#region Properties

		/// <summary>
		/// Gets or sets the location of the XRDS document.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Property grid")]
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Xrds", Justification = "Correct spelling")]
		[Category("Behavior"), Bindable(true)]
		[UrlProperty, Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
		public string XrdsUrl {
			get {
				return (string)ViewState[XrdsUrlViewStateKey];
			}

			set {
				UriUtil.ValidateResolvableUrl(Page, DesignMode, value);
				ViewState[XrdsUrlViewStateKey] = value;
			}
		}

		/// <summary>
		/// Gets or sets where the XRDS document URL is advertised in the web response.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Xrds", Justification = "Correct spelling")]
		[Category("Behavior"), DefaultValue(XrdsAdvertisementDefault), Bindable(true)]
		[Description("Where the XRDS document URL is advertised in the web response.")]
		public XrdsUrlLocations XrdsAdvertisement {
			get {
				return ViewState[XrdsAdvertisementViewStateKey] == null ?
					XrdsAdvertisementDefault : (XrdsUrlLocations)ViewState[XrdsAdvertisementViewStateKey];
			}

			set {
				ViewState[XrdsAdvertisementViewStateKey] = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether a specially crafted YADIS 
		/// search for an XRDS document is immediately answered by this control.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Xrds", Justification = "Correct spelling")]
		[Category("Behavior"), DefaultValue(XrdsAutoAnswerDefault), Bindable(true)]
		[Description("Whether XRDS requests should be immediately answered with the XRDS document if it is served by this web application.")]
		public bool XrdsAutoAnswer {
			get {
				return ViewState[XrdsAutoAnswerViewStateKey] == null ?
					XrdsAutoAnswerDefault : (bool)ViewState[XrdsAutoAnswerViewStateKey];
			}

			set {
				ViewState[XrdsAutoAnswerViewStateKey] = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the XRDS document is advertised.
		/// </summary>
		[Category("Behavior"), DefaultValue(EnabledDefault)]
		public bool Enabled {
			get {
				return ViewState[EnabledViewStateKey] == null ?
				EnabledDefault : (bool)ViewState[EnabledViewStateKey];
			}

			set {
				ViewState[EnabledViewStateKey] = value;
			}
		}

		#endregion

		/// <summary>
		/// Detects YADIS requests for the XRDS document and responds immediately
		/// if <see cref="XrdsAutoAnswer"/> is true.
		/// </summary>
		/// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			if (!this.Enabled) {
				return;
			}

			if (!this.Page.IsPostBack) {
				if (this.XrdsAutoAnswer && !string.IsNullOrEmpty(this.XrdsUrl) &&
					this.XrdsUrl.StartsWith("~/", StringComparison.Ordinal)) {
					// Check for the presence of an accept types header that is looking
					// for the XRDS document specifically.
					if (this.Page.Request.AcceptTypes != null && Array.IndexOf(this.Page.Request.AcceptTypes, ContentTypes.Xrds) >= 0) {
						// Respond to the caller immediately with an XRDS document
						// and avoid sending the whole web page's contents to the 
						// client since it isn't interested anyway.
						// We do NOT simply send a 301 redirect here because that would
						// alter the Claimed Identifier.
						Logger.Yadis.InfoFormat("Transferring request from {0} to {1} to respond to XRDS discovery request.", this.Page.Request.Url.AbsoluteUri, this.XrdsUrl);
						this.Page.Server.Transfer(this.XrdsUrl);
					}
				}
			}
		}

		/// <summary>
		/// Renders the HTTP Header and/or HTML HEAD tags.
		/// </summary>
		/// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the server control content.</param>
		[SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Diagnostics.Contracts.__ContractsRuntime.Assume(System.Boolean,System.String,System.String)", Justification = "Code contracts"), SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "Uri(Uri, string) accepts second arguments that Uri(Uri, new Uri(string)) does not that we must support.")]
		protected override void Render(HtmlTextWriter writer) {
			Assumes.True(writer != null, "Missing contract.");
			if (this.Enabled && this.Visible && !string.IsNullOrEmpty(this.XrdsUrl)) {
				Uri xrdsAddress = new Uri(MessagingUtilities.GetRequestUrlFromContext(), Page.Response.ApplyAppPathModifier(this.XrdsUrl));
				if ((this.XrdsAdvertisement & XrdsUrlLocations.HttpHeader) != 0) {
					Page.Response.AddHeader(Yadis.Yadis.HeaderName, xrdsAddress.AbsoluteUri);
				}
				if ((this.XrdsAdvertisement & XrdsUrlLocations.HtmlMeta) != 0) {
					writer.WriteBeginTag("meta");
					writer.WriteAttribute("http-equiv", Yadis.Yadis.HeaderName);
					writer.WriteAttribute("content", xrdsAddress.AbsoluteUri);
					writer.Write("/>");
					writer.WriteLine();
				}
			}
		}
	}
}
