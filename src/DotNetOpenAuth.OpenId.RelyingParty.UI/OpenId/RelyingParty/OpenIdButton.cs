//-----------------------------------------------------------------------
// <copyright file="OpenIdButton.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Drawing.Design;
	using System.Globalization;
	using System.Linq;
	using System.Net.Http;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using System.Web.UI;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// An ASP.NET control that renders a button that initiates an
	/// authentication when clicked.
	/// </summary>
	public class OpenIdButton : OpenIdRelyingPartyControlBase {
		#region Property defaults

		/// <summary>
		/// The default value for the <see cref="Text"/> property.
		/// </summary>
		private const string TextDefault = "Log in with [Provider]!";

		/// <summary>
		/// The default value for the <see cref="PrecreateRequest"/> property.
		/// </summary>
		private const bool PrecreateRequestDefault = false;

		#endregion

		#region View state keys

		/// <summary>
		/// The key under which the value for the <see cref="Text"/> property will be stored.
		/// </summary>
		private const string TextViewStateKey = "Text";

		/// <summary>
		/// The key under which the value for the <see cref="ImageUrl"/> property will be stored.
		/// </summary>
		private const string ImageUrlViewStateKey = "ImageUrl";

		/// <summary>
		/// The key under which the value for the <see cref="PrecreateRequest"/> property will be stored.
		/// </summary>
		private const string PrecreateRequestViewStateKey = "PrecreateRequest";

		#endregion

		/// <summary>
		/// Stores the asynchronously created message that can be used to initiate a redirection-based authentication request.
		/// </summary>
		private HttpResponseMessage authenticationRequestRedirect;

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdButton"/> class.
		/// </summary>
		public OpenIdButton() {
		}

		/// <summary>
		/// Gets or sets the text to display for the link.
		/// </summary>
		[Bindable(true), DefaultValue(TextDefault), Category(AppearanceCategory)]
		[Description("The text to display for the link.")]
		public string Text {
			get { return (string)ViewState[TextViewStateKey] ?? TextDefault; }
			set { ViewState[TextViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets the image to display.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Bindable property must be simple type")]
		[Bindable(true), Category(AppearanceCategory)]
		[Description("The image to display.")]
		[UrlProperty, Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
		public string ImageUrl {
			get {
				return (string)ViewState[ImageUrlViewStateKey];
			}

			set {
				UriUtil.ValidateResolvableUrl(Page, DesignMode, value);
				ViewState[ImageUrlViewStateKey] = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether to pre-discover the identifier so
		/// the user agent has an immediate redirect.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Precreate", Justification = "Breaking change to public API")]
		[Bindable(true), Category(OpenIdCategory), DefaultValue(PrecreateRequestDefault)]
		[Description("Whether to pre-discover the identifier so the user agent has an immediate redirect.")]
		public bool PrecreateRequest {
			get { return (bool)(ViewState[PrecreateRequestViewStateKey] ?? PrecreateRequestDefault); }
			set { ViewState[PrecreateRequestViewStateKey] = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating when to use a popup window to complete the login experience.
		/// </summary>
		/// <value>The default value is <see cref="PopupBehavior.Never"/>.</value>
		[Bindable(false), Browsable(false)]
		public override PopupBehavior Popup {
			get { return base.Popup; }
			set { ErrorUtilities.VerifySupported(value == base.Popup, OpenIdStrings.PropertyValueNotSupported); }
		}

		/// <summary>
		/// When implemented by a class, enables a server control to process an event raised when a form is posted to the server.
		/// </summary>
		/// <param name="eventArgument">A <see cref="T:System.String"/> that represents an optional event argument to be passed to the event handler.</param>
		protected override void RaisePostBackEvent(string eventArgument) {
			if (!this.PrecreateRequest) {
				this.Page.RegisterAsyncTask(new PageAsyncTask(async ct => {
					try {
						var requests = await this.CreateRequestsAsync(ct);
						var request = requests.First();
						await request.RedirectToProviderAsync(new HttpContextWrapper(this.Context), ct);
					} catch (InvalidOperationException ex) {
						throw ErrorUtilities.Wrap(ex, OpenIdStrings.OpenIdEndpointNotFound);
					}
				}));
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Web.UI.Control.PreRender"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
		protected override void OnPreRender(EventArgs e) {
			base.OnPreRender(e);

			if (!this.DesignMode) {
				ErrorUtilities.VerifyOperation(this.Identifier != null, OpenIdStrings.NoIdentifierSet);

				if (this.PrecreateRequest) {
					this.Page.RegisterAsyncTask(
						new PageAsyncTask(
							async ct => {
								var requests = await this.CreateRequestsAsync(ct);
								this.authenticationRequestRedirect = await requests.FirstOrDefault().GetRedirectingResponseAsync(ct);
							}));
				}
			}
		}

		/// <summary>
		/// Sends server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter"/> object, which writes the content to be rendered on the client.
		/// </summary>
		/// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the server control content.</param>
		[SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Web.UI.HtmlTextWriter.WriteEncodedText(System.String)", Justification = "Not localizable")]
		protected override void Render(HtmlTextWriter writer) {
			if (string.IsNullOrEmpty(this.Identifier)) {
				writer.WriteEncodedText(string.Format(CultureInfo.CurrentCulture, "[{0}]", OpenIdStrings.NoIdentifierSet));
			} else {
				string tooltip = this.Text;
				if (this.PrecreateRequest && !this.DesignMode) {
					if (this.authenticationRequestRedirect != null) {
						this.RenderOpenIdMessageTransmissionAsAnchorAttributes(writer, this.authenticationRequestRedirect, tooltip);
					} else {
						tooltip = OpenIdStrings.OpenIdEndpointNotFound;
					}
				} else {
					writer.AddAttribute(HtmlTextWriterAttribute.Href, this.Page.ClientScript.GetPostBackClientHyperlink(this, null));
				}

				writer.AddAttribute(HtmlTextWriterAttribute.Title, tooltip);
				writer.RenderBeginTag(HtmlTextWriterTag.A);

				if (!string.IsNullOrEmpty(this.ImageUrl)) {
					writer.AddAttribute(HtmlTextWriterAttribute.Src, this.ResolveClientUrl(this.ImageUrl));
					writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
					writer.AddAttribute(HtmlTextWriterAttribute.Alt, this.Text);
					writer.AddAttribute(HtmlTextWriterAttribute.Title, this.Text);
					writer.RenderBeginTag(HtmlTextWriterTag.Img);
					writer.RenderEndTag();
				} else if (!string.IsNullOrEmpty(this.Text)) {
					writer.WriteEncodedText(this.Text);
				}

				writer.RenderEndTag();
			}
		}
	}
}
