//-----------------------------------------------------------------------
// <copyright file="OpenIdButton.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
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
	using System.Text;
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

		#endregion

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
		/// Raises the <see cref="E:System.Web.UI.Control.PreRender"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
		protected override void OnPreRender(EventArgs e) {
			base.OnPreRender(e);

			if (!this.DesignMode) {
				ErrorUtilities.VerifyOperation(this.Identifier != null, OpenIdStrings.NoIdentifierSet);
			}
		}

		/// <summary>
		/// Sends server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter"/> object, which writes the content to be rendered on the client.
		/// </summary>
		/// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the server control content.</param>
		protected override void Render(HtmlTextWriter writer) {
			if (string.IsNullOrEmpty(this.Identifier)) {
				writer.WriteEncodedText(string.Format(CultureInfo.CurrentCulture, "[{0}]", OpenIdStrings.NoIdentifierSet));
			} else {
				string tooltip = this.Text;
				IAuthenticationRequest request = this.CreateRequests().FirstOrDefault();
				if (request != null) {
					writer.AddAttribute(HtmlTextWriterAttribute.Href, request.RedirectingResponse.GetDirectUriRequest(this.RelyingParty.Channel).AbsoluteUri);
				} else {
					tooltip = OpenIdStrings.OpenIdEndpointNotFound;
					writer.AddAttribute(HtmlTextWriterAttribute.Title, tooltip);
				}
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
