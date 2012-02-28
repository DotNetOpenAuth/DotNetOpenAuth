//-----------------------------------------------------------------------
// <copyright file="SelectorButtonContract.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Diagnostics.Contracts;
	using System.Web.UI;

	/// <summary>
	/// The contract class for the <see cref="SelectorButton"/> class.
	/// </summary>
	[ContractClassFor(typeof(SelectorButton))]
	internal abstract class SelectorButtonContract : SelectorButton {
		/// <summary>
		/// Ensures that this button has been initialized to a valid state.
		/// </summary>
		/// <remarks>
		/// This is "internal" -- NOT "protected internal" deliberately.  It makes it impossible
		/// to derive from this class outside the assembly, which suits our purposes since the
		/// <see cref="OpenIdSelector"/> control is not designed for an extensible set of button types.
		/// </remarks>
		internal override void EnsureValid() {
		}

		/// <summary>
		/// Renders the leading attributes for the LI tag.
		/// </summary>
		/// <param name="writer">The writer.</param>
		protected internal override void RenderLeadingAttributes(HtmlTextWriter writer) {
			Requires.NotNull(writer, "writer");
		}

		/// <summary>
		/// Renders the content of the button.
		/// </summary>
		/// <param name="writer">The writer.</param>
		/// <param name="selector">The containing selector control.</param>
		protected internal override void RenderButtonContent(HtmlTextWriter writer, OpenIdSelector selector) {
			Requires.NotNull(writer, "writer");
			Requires.NotNull(selector, "selector");
		}
	}
}
