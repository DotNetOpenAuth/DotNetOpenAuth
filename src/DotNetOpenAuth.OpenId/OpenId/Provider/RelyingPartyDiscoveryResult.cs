//-----------------------------------------------------------------------
// <copyright file="RelyingPartyDiscoveryResult.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	/// <summary>
	/// The result codes that may be returned from an attempt at relying party discovery.
	/// </summary>
	public enum RelyingPartyDiscoveryResult {
		/// <summary>
		/// Relying Party discovery failed to find an XRDS document or the document was invalid.
		/// </summary>
		/// <remarks>
		/// This can happen either when a relying party does not offer a service document at all,
		/// or when a man-in-the-middle attack is in progress that prevents the Provider from being
		/// able to discover that document.
		/// </remarks>
		NoServiceDocument,

		/// <summary>
		/// Relying Party discovery yielded a valid XRDS document, but no matching return_to URI was found.
		/// </summary>
		/// <remarks>
		/// This is perhaps the most dangerous rating for a relying party, since it suggests that
		/// they are implementing OpenID 2.0 securely, but that a hijack operation may be in progress.
		/// </remarks>
		NoMatchingReturnTo,

		/// <summary>
		/// Relying Party discovery succeeded, and a matching return_to URI was found.
		/// </summary>
		Success,
	}
}
