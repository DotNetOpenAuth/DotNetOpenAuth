//-----------------------------------------------------------------------
// <copyright file="IPrivateSecretStore.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.ChannelElements {
	/// <summary>
	/// Provides access to and persists a private secret that is used for signing.
	/// </summary>
	public interface IPrivateSecretStore {
		/// <summary>
		/// Gets or sets a secret key that can be used for signing.
		/// </summary>
		/// <value>A 64-byte binary value, which may contain null bytes.</value>
		byte[] PrivateSecret { get; set; }
	}
}
