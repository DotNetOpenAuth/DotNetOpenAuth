//-----------------------------------------------------------------------
// <copyright file="SelectorButton.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System.ComponentModel;
	using DotNetOpenAuth.ComponentModel;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A button that would appear in the <see cref="OpenIdSelector"/> control via its <see cref="OpenIdSelector.Buttons"/> collection.
	/// </summary>
	public class SelectorButton {
		/// <summary>
		/// Initializes a new instance of the <see cref="SelectorButton"/> class.
		/// </summary>
		public SelectorButton() {
		}

		/// <summary>
		/// Gets or sets the OP Identifier represented by the button.
		/// </summary>
		/// <value>
		/// The OP identifier, which may be provided in the easiest "user-supplied identifier" form,
		/// but for security should be provided with a leading https:// if possible.
		/// </value>
		[TypeConverter(typeof(IdentifierConverter))]
		public Identifier OPIdentifier { get; set; }

		/// <summary>
		/// Gets or sets the path to the image to display on the button's surface.
		/// </summary>
		/// <value>The virtual path to the image.</value>
		public string Image { get; set; }

		/// <summary>
		/// Ensures that this button has been initialized to a valid state.
		/// </summary>
		internal void EnsureValid() {
			// Every button must have an image.
			ErrorUtilities.VerifyOperation(!string.IsNullOrEmpty(this.Image), OpenIdStrings.PropertyNotSet, "SelectorButton.Image");

			// Every button must have exactly one purpose.
			////ErrorUtilities.VerifyOperation(this.OPIdentifier != null, OpenIdStrings.PropertyNotSet, "SelectorButton.OPIdentifier");
		}
	}
}
