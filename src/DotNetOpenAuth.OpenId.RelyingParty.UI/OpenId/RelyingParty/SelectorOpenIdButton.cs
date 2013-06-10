//-----------------------------------------------------------------------
// <copyright file="SelectorOpenIdButton.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.ComponentModel;
	using System.Drawing.Design;
	using System.Web.UI;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// A button that appears in the <see cref="OpenIdSelector"/> control that
	/// allows the user to type in a user-supplied identifier.
	/// </summary>
	public class SelectorOpenIdButton : SelectorButton {
		/// <summary>
		/// Initializes a new instance of the <see cref="SelectorOpenIdButton"/> class.
		/// </summary>
		public SelectorOpenIdButton() {
			Reporting.RecordFeatureUse(this);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SelectorOpenIdButton"/> class.
		/// </summary>
		/// <param name="imageUrl">The image to display on the button.</param>
		public SelectorOpenIdButton(string imageUrl)
			: this() {
			Requires.NotNullOrEmpty(imageUrl, "imageUrl");

			this.Image = imageUrl;
		}

		/// <summary>
		/// Gets or sets the path to the image to display on the button's surface.
		/// </summary>
		/// <value>The virtual path to the image.</value>
		[Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
		[UrlProperty]
		public string Image { get; set; }

		/// <summary>
		/// Ensures that this button has been initialized to a valid state.
		/// </summary>
		internal override void EnsureValid() {
			// Every button must have an image.
			ErrorUtilities.VerifyOperation(!string.IsNullOrEmpty(this.Image), OpenIdStrings.PropertyNotSet, "SelectorButton.Image");
		}

		/// <summary>
		/// Renders the leading attributes for the LI tag.
		/// </summary>
		/// <param name="writer">The writer.</param>
		protected internal override void RenderLeadingAttributes(HtmlTextWriter writer) {
			writer.AddAttribute(HtmlTextWriterAttribute.Id, "OpenIDButton");
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "OpenIDButton");
		}

		/// <summary>
		/// Renders the content of the button.
		/// </summary>
		/// <param name="writer">The writer.</param>
		/// <param name="selector">The containing selector control.</param>
		protected internal override void RenderButtonContent(HtmlTextWriter writer, OpenIdSelector selector) {
			writer.AddAttribute(HtmlTextWriterAttribute.Src, selector.Page.ResolveUrl(this.Image));
			writer.RenderBeginTag(HtmlTextWriterTag.Img);
			writer.RenderEndTag();

			writer.AddAttribute(HtmlTextWriterAttribute.Src, selector.Page.ClientScript.GetWebResourceUrl(typeof(OpenIdAjaxTextBox), OpenIdAjaxTextBox.EmbeddedLoginSuccessResourceName));
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "loginSuccess");
			writer.AddAttribute(HtmlTextWriterAttribute.Title, selector.AuthenticatedAsToolTip);
			writer.RenderBeginTag(HtmlTextWriterTag.Img);
			writer.RenderEndTag();
		}
	}
}
