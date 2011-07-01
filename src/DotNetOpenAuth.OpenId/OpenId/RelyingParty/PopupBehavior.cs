//-----------------------------------------------------------------------
// <copyright file="PopupBehavior.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	/// <summary>
	/// Several ways that the relying party can direct the user to the Provider
	/// to complete authentication.
	/// </summary>
	public enum PopupBehavior {
		/// <summary>
		/// A full browser window redirect will be used to send the
		/// user to the Provider.
		/// </summary>
		Never,

		/// <summary>
		/// A popup window will be used to send the user to the Provider.
		/// </summary>
		Always,

		/// <summary>
		/// A popup window will be used to send the user to the Provider
		/// if the Provider advertises support for the popup UI extension;
		/// otherwise a standard redirect is used.
		/// </summary>
		IfProviderSupported,
	}
}
