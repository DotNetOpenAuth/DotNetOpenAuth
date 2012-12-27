//-----------------------------------------------------------------------
// <copyright file="SelectorButton.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Web.UI;

	/// <summary>
	/// A button that would appear in the <see cref="OpenIdSelector"/> control via its <see cref="OpenIdSelector.Buttons"/> collection.
	/// </summary>
	[Serializable]
	public abstract class SelectorButton {
		/// <summary>
		/// Initializes a new instance of the <see cref="SelectorButton"/> class.
		/// </summary>
		protected SelectorButton() {
		}

		/// <summary>
		/// Ensures that this button has been initialized to a valid state.
		/// </summary>
		/// <remarks>
		/// This is "internal" -- NOT "protected internal" deliberately.  It makes it impossible
		/// to derive from this class outside the assembly, which suits our purposes since the
		/// <see cref="OpenIdSelector"/> control is not designed for an extensible set of button types.
		/// </remarks>
		internal abstract void EnsureValid();

		/// <summary>
		/// Renders the leading attributes for the LI tag.
		/// </summary>
		/// <param name="writer">The writer.</param>
		protected internal abstract void RenderLeadingAttributes(HtmlTextWriter writer);

		/// <summary>
		/// Renders the content of the button.
		/// </summary>
		/// <param name="writer">The writer.</param>
		/// <param name="selector">The containing selector control.</param>
		protected internal abstract void RenderButtonContent(HtmlTextWriter writer, OpenIdSelector selector);
	}
}
