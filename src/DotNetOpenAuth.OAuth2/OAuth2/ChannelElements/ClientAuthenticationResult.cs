//-----------------------------------------------------------------------
// <copyright file="ClientAuthenticationResult.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	/// <summary>
	/// Describes the various levels at which client information may be extracted from an inbound message.
	/// </summary>
	public enum ClientAuthenticationResult {
		/// <summary>
		/// No client identification or authentication was discovered.
		/// </summary>
		NoAuthenticationRecognized,

		/// <summary>
		/// The client identified itself, but did not attempt to authenticate itself.
		/// </summary>
		ClientIdNotAuthenticated,

		/// <summary>
		/// The client authenticated itself (provided compelling evidence that it was who it claims to be).
		/// </summary>
		ClientAuthenticated,

		/// <summary>
		/// The client failed in an attempt to authenticate itself, claimed to be an unrecognized client, or otherwise messed up.
		/// </summary>
		ClientAuthenticationRejected,
	}
}
