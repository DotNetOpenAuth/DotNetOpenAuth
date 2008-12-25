//-----------------------------------------------------------------------
// <copyright file="PrivateSecretMemoryStore.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using DotNetOpenAuth.OpenId.ChannelElements;

	/// <summary>
	/// The stock in-memory store for private secrets needed by Relying Parties.
	/// </summary>
	/// <remarks>
	/// This class is only good for NON-web farm/garden environments.
	/// Multi-process web applications must implement their own store
	/// that shares state across all instances of the web application
	/// so that signatures made on one server can be verified on another.
	/// </remarks>
	internal class PrivateSecretMemoryStore : IPrivateSecretStore {
		#region IPrivateSecretStore Members

		/// <summary>
		/// Gets or sets a secret key that can be used for signing.
		/// </summary>
		/// <value>A 64-byte binary value, which may contain null bytes.</value>
		public byte[] PrivateSecret { get; set; }

		#endregion
	}
}
