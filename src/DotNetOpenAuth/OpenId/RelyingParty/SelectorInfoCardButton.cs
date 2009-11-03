//-----------------------------------------------------------------------
// <copyright file="SelectorInfoCardButton.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System.Web.UI;

	/// <summary>
	/// A button that appears in the <see cref="OpenIdSelector"/> control that
	/// activates the Information Card selector on the browser, if one is available.
	/// </summary>
	public class SelectorInfoCardButton : SelectorButton {
		/// <summary>
		/// Initializes a new instance of the <see cref="SelectorInfoCardButton"/> class.
		/// </summary>
		public SelectorInfoCardButton() {
		}

		/// <summary>
		/// Ensures that this button has been initialized to a valid state.
		/// </summary>
		internal override void EnsureValid() {
		}

		/// <summary>
		/// Renders the leading attributes for the LI tag.
		/// </summary>
		/// <param name="writer">The writer.</param>
		protected internal override void RenderLeadingAttributes(HtmlTextWriter writer) {
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "infocard");
		}

		/// <summary>
		/// Renders the content of the button.
		/// </summary>
		/// <param name="writer">The writer.</param>
		/// <param name="selector">The containing selector control.</param>
		protected internal override void RenderButtonContent(HtmlTextWriter writer, OpenIdSelector selector) {
			selector.InfoCardSelector.RenderControl(writer);
		}
	}
}
