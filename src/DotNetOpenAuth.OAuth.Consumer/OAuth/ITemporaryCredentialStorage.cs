//-----------------------------------------------------------------------
// <copyright file="ITemporaryCredentialStorage.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth {
	using System.Collections.Generic;

	/// <summary>
	/// A token manager for use by an OAuth Consumer to store a temporary credential
	/// (previously known as "unauthorized request token and secret").
	/// </summary>
	/// <remarks>
	/// The credentials stored here are obtained as described in:
	/// http://tools.ietf.org/html/rfc5849#section-2.1
	/// </remarks>
	public interface ITemporaryCredentialStorage {
		/// <summary>
		/// Saves the specified temporary credential for later retrieval.
		/// </summary>
		/// <param name="identifier">The identifier.</param>
		/// <param name="secret">The secret.</param>
		void SaveTemporaryCredential(string identifier, string secret);

		/// <summary>
		/// Obtains a temporary credential secret, if available.
		/// </summary>
		/// <returns>The temporary credential identifier secret if available; otherwise a key value pair whose strings are left in their uninitialized <c>null</c> state.</returns>
		KeyValuePair<string, string> RetrieveTemporaryCredential();

		/// <summary>
		/// Clears the temporary credentials from storage.
		/// </summary>
		/// <remarks>
		/// DotNetOpenAuth calls this when the credentials are no longer needed.
		/// </remarks>
		void ClearTemporaryCredential();
	}
}
