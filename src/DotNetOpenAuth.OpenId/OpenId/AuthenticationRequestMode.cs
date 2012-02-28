//-----------------------------------------------------------------------
// <copyright file="AuthenticationRequestMode.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	/// <summary>
	/// Indicates the mode the Provider should use while authenticating the end user.
	/// </summary>
	public enum AuthenticationRequestMode {
		/// <summary>
		/// The Provider should use whatever credentials are immediately available
		/// to determine whether the end user owns the Identifier.  If sufficient
		/// credentials (i.e. cookies) are not immediately available, the Provider
		/// should fail rather than prompt the user.
		/// </summary>
		Immediate,

		/// <summary>
		/// The Provider should determine whether the end user owns the Identifier,
		/// displaying a web page to the user to login etc., if necessary.
		/// </summary>
		Setup,
	}
}
