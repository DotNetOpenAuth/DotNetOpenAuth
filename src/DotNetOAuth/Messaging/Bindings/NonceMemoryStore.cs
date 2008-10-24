//-----------------------------------------------------------------------
// <copyright file="NonceMemoryStore.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messaging.Bindings {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOAuth.Messaging.Bindings;

	/// <summary>
	/// An in-memory nonce store.  Useful for single-server web applications.
	/// NOT for web farms.
	/// </summary>
	internal class NonceMemoryStore : INonceStore {
		/// <summary>
		/// The maximum age a message can be before it is discarded.
		/// </summary>
		/// <remarks>
		/// This is useful for knowing how long used nonces must be retained.
		/// </remarks>
		private readonly TimeSpan maximumMessageAge;

		/// <summary>
		/// Initializes a new instance of the <see cref="NonceMemoryStore"/> class.
		/// </summary>
		/// <param name="maximumMessageAge">The maximum age a message can be before it is discarded.</param>
		internal NonceMemoryStore(TimeSpan maximumMessageAge) {
			this.maximumMessageAge = maximumMessageAge;
		}

		#region INonceStore Members

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
		/// If the binding element is applicable to your channel, this expiration window
		/// is retrieved or set using the 
		/// <see cref="StandardExpirationBindingElement.MaximumMessageAge"/> property.
		/// </remarks>
		public bool StoreNonce(string nonce, DateTime timestamp) {
			if (timestamp.ToUniversalTime() + this.maximumMessageAge < DateTime.UtcNow) {
				// The expiration binding element should have taken care of this, but perhaps
				// it's at the boundary case.  We should fail just to be safe.
				return false;
			}

			// TODO: implement actual nonce checking.
			Logger.Warn("Nonce checking not implemented yet.");
			return true;
		}

		#endregion
	}
}
