//-----------------------------------------------------------------------
// <copyright file="SelectorButton.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System.ComponentModel;
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.ComponentModel;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A button that would appear in the <see cref="OpenIdSelector"/> control via its <see cref="OpenIdSelector.Buttons"/> collection.
	/// </summary>
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
	}

	/// <summary>
	/// A button that appears in the <see cref="OpenIdSelector"/> control that
	/// activates the Information Card selector on the browser, if one is available.
	/// </summary>
	public class SelectorInfoCardButton : SelectorButton {
		/// <summary>
		/// Ensures that this button has been initialized to a valid state.
		/// </summary>
		protected internal override void EnsureValid() {
		}
	}

	/// <summary>
	/// A button that appears in the <see cref="OpenIdSelector"/> control that
	/// allows the user to type in a user-supplied identifier.
	/// </summary>
	public class SelectorOpenIdButton : SelectorButton {
		/// <summary>
		/// Gets or sets the path to the image to display on the button's surface.
		/// </summary>
		/// <value>The virtual path to the image.</value>
		public string Image { get; set; }

		/// <summary>
		/// Ensures that this button has been initialized to a valid state.
		/// </summary>
		protected internal override void EnsureValid() {
			Contract.Ensures(!string.IsNullOrEmpty(this.Image));

			// Every button must have an image.
			ErrorUtilities.VerifyOperation(!string.IsNullOrEmpty(this.Image), OpenIdStrings.PropertyNotSet, "SelectorButton.Image");
		}
	}

	/// <summary>
	/// A button that appears in the <see cref="OpenIdSelector"/> control that
	/// provides one-click access to a popular OpenID Provider.
	/// </summary>
	public class SelectorProviderButton : SelectorButton {
		/// <summary>
		/// Initializes a new instance of the <see cref="SelectorProviderButton"/> class.
		/// </summary>
		public SelectorProviderButton() {
		}

		/// <summary>
		/// Gets or sets the path to the image to display on the button's surface.
		/// </summary>
		/// <value>The virtual path to the image.</value>
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
		/// Ensures that this button has been initialized to a valid state.
		/// </summary>
		protected internal override void EnsureValid() {
			Contract.Ensures(!string.IsNullOrEmpty(this.Image));
			Contract.Ensures(this.OPIdentifier != null);

			// Every button must have an image.
			ErrorUtilities.VerifyOperation(!string.IsNullOrEmpty(this.Image), OpenIdStrings.PropertyNotSet, "SelectorButton.Image");

			// Every button must have exactly one purpose.
			ErrorUtilities.VerifyOperation(this.OPIdentifier != null, OpenIdStrings.PropertyNotSet, "SelectorButton.OPIdentifier");
		}
	}
}
