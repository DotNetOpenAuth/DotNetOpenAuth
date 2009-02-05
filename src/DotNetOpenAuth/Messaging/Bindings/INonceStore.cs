//-----------------------------------------------------------------------
// <copyright file="INonceStore.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging.Bindings {
	using System;

	/// <summary>
	/// Describes the contract a nonce store must fulfill.
	/// </summary>
	public interface INonceStore {
		/// <summary>
		/// Stores a given nonce and timestamp.
		/// </summary>
		/// <param name="nonce">
		/// A series of random characters.
		/// </param>
		/// <param name="timestamp">
		/// The timestamp that together with the nonce string make it unique.
		/// The timestamp may also be used by the data store to clear out old nonces.
		/// </param>
		/// <returns>
		/// True if the nonce+timestamp (combination) was not previously in the database.
		/// False if the nonce was stored previously with the same timestamp.
		/// </returns>
		/// <remarks>
		/// The nonce must be stored for no less than the maximum time window a message may
		/// be processed within before being discarded as an expired message.
		/// This maximum message age can be looked up via the 
		/// <see cref="DotNetOpenAuth.Configuration.MessagingElement.MaximumMessageLifetime"/>
		/// property, accessible via the <see cref="DotNetOpenAuth.Configuration.DotNetOpenAuthSection.Configuration"/>
		/// property.
		/// </remarks>
		bool StoreNonce(string nonce, DateTime timestamp);
	}
}
