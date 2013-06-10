//-----------------------------------------------------------------------
// <copyright file="UIUtilities.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.UI {
	using System;
	using System.Globalization;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.RelyingParty;

	/// <summary>
	/// Constants used in implementing support for the UI extension.
	/// </summary>
	public static class UIUtilities {
		/// <summary>
		/// The required width of the popup window the relying party creates for the provider.
		/// </summary>
		public const int PopupWidth = 500; // UI extension calls for 450px, but Yahoo needs 500px

		/// <summary>
		/// The required height of the popup window the relying party creates for the provider.
		/// </summary>
		public const int PopupHeight = 500;
	}
}
