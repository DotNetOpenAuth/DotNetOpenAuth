//-----------------------------------------------------------------------
// <copyright file="MemoryTemporaryCredentialStorage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	/// <summary>
	/// Non-persistent memory storage for temporary credentials.
	/// Useful for installed apps (not redirection based web apps).
	/// </summary>
	public class MemoryTemporaryCredentialStorage : ITemporaryCredentialStorage {
		/// <summary>
		/// The identifier.
		/// </summary>
		private string identifier;

		/// <summary>
		/// The secret.
		/// </summary>
		private string secret;

		#region ITemporaryCredentialStorage Members

		/// <summary>
		/// Saves the specified temporary credential for later retrieval.
		/// </summary>
		/// <param name="identifier">The identifier.</param>
		/// <param name="secret">The secret.</param>
		public void SaveTemporaryCredential(string identifier, string secret) {
			this.identifier = identifier;
			this.secret = secret;
		}

		/// <summary>
		/// Obtains a temporary credential secret, if available.
		/// </summary>
		/// <returns>
		/// The temporary credential secret if available; otherwise <c>null</c>.
		/// </returns>
		public KeyValuePair<string, string> RetrieveTemporaryCredential() {
			return new KeyValuePair<string, string>(this.identifier, this.secret);
		}

		/// <summary>
		/// Clears the temporary credentials from storage.
		/// </summary>
		/// <remarks>
		/// DotNetOpenAuth calls this when the credentials are no longer needed.
		/// </remarks>
		public void ClearTemporaryCredential() {
			this.identifier = null;
			this.secret = null;
		}

		#endregion
	}
}
