//-----------------------------------------------------------------------
// <copyright file="SelectorProviderButton.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.ComponentModel;
	using System.Drawing.Design;
	using System.Web.UI;
	using DotNetOpenAuth.ComponentModel;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// A button that appears in the <see cref="OpenIdSelector"/> control that
	/// provides one-click access to a popular OpenID Provider.
	/// </summary>
	public class SelectorProviderButton : SelectorButton {
		/// <summary>
		/// Initializes a new instance of the <see cref="SelectorProviderButton"/> class.
		/// </summary>
		public SelectorProviderButton() {
			Reporting.RecordFeatureUse(this);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SelectorProviderButton"/> class.
		/// </summary>
		/// <param name="providerIdentifier">The OP Identifier.</param>
		/// <param name="imageUrl">The image to display on the button.</param>
		public SelectorProviderButton(Identifier providerIdentifier, string imageUrl)
			: this() {
			Requires.NotNull(providerIdentifier, "providerIdentifier");
			Requires.NotNullOrEmpty(imageUrl, "imageUrl");

			this.OPIdentifier = providerIdentifier;
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
		/// Gets or sets the OP Identifier represented by the button.
		/// </summary>
		/// <value>
		/// The OP identifier, which may be provided in the easiest "user-supplied identifier" form,
		/// but for security should be provided with a leading https:// if possible.
		/// For example: "yahoo.com" or "https://me.yahoo.com/".
		/// </value>
		[TypeConverter(typeof(IdentifierConverter))]
		public Identifier OPIdentifier { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this Provider doesn't handle
		/// checkid_immediate messages correctly and background authentication
		/// should not be attempted.
		/// </summary>
		public bool SkipBackgroundAuthentication { get; set; }

		/// <summary>
		/// Ensures that this button has been initialized to a valid state.
		/// </summary>
		internal override void EnsureValid() {
			// Every button must have an image.
			ErrorUtilities.VerifyOperation(!string.IsNullOrEmpty(this.Image), OpenIdStrings.PropertyNotSet, "SelectorButton.Image");

			// Every button must have exactly one purpose.
			ErrorUtilities.VerifyOperation(this.OPIdentifier != null, OpenIdStrings.PropertyNotSet, "SelectorButton.OPIdentifier");
		}

		/// <summary>
		/// Renders the leading attributes for the LI tag.
		/// </summary>
		/// <param name="writer">The writer.</param>
		protected internal override void RenderLeadingAttributes(HtmlTextWriter writer) {
			writer.AddAttribute(HtmlTextWriterAttribute.Id, this.OPIdentifier);

			string style = "OPButton";
			if (this.SkipBackgroundAuthentication) {
				style += " NoAsyncAuth";
			}
			writer.AddAttribute(HtmlTextWriterAttribute.Class, style);
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
