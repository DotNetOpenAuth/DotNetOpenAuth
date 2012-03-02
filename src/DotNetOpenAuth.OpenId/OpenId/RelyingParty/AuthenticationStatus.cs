//-----------------------------------------------------------------------
// <copyright file="AuthenticationStatus.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	/// <summary>
	/// An enumeration of the possible results of an authentication attempt.
	/// </summary>
	public enum AuthenticationStatus {
		/// <summary>
		/// The authentication was canceled by the user agent while at the provider.
		/// </summary>
		Canceled,

		/// <summary>
		/// The authentication failed because an error was detected in the OpenId communication.
		/// </summary>
		Failed,

		/// <summary>
		/// <para>The Provider responded to a request for immediate authentication approval
		/// with a message stating that additional user agent interaction is required
		/// before authentication can be completed.</para>
		/// <para>Casting the <see cref="IAuthenticationResponse"/> to a 
		/// ISetupRequiredAuthenticationResponse in this case can help
		/// you retry the authentication using setup (non-immediate) mode.</para>
		/// </summary>
		SetupRequired,

		/// <summary>
		/// Authentication is completed successfully.
		/// </summary>
		Authenticated,

		/// <summary>
		/// The Provider sent a message that did not contain an identity assertion,
		/// but may carry OpenID extensions.
		/// </summary>
		ExtensionsOnly,
	}
}
