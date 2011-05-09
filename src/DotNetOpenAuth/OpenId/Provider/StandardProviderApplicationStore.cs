//-----------------------------------------------------------------------
// <copyright file="StandardProviderApplicationStore.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging.Bindings;

	/// <summary>
	/// An in-memory store for Providers, suitable for single server, single process
	/// ASP.NET web sites.
	/// </summary>
	/// <remarks>
	/// This class provides only a basic implementation that is likely to work
	/// out of the box on most single-server web sites.  It is highly recommended
	/// that high traffic web sites consider using a database to store the information
	/// used by an OpenID Provider and write a custom implementation of the
	/// <see cref="IProviderApplicationStore"/> interface to use instead of this
	/// class.
	/// </remarks>
	public class StandardProviderApplicationStore : ProviderAssociationHandleEncoder, IProviderApplicationStore {
		/// <summary>
		/// The nonce store to use.
		/// </summary>
		private readonly INonceStore nonceStore;

		/// <summary>
		/// Initializes a new instance of the <see cref="StandardProviderApplicationStore"/> class.
		/// </summary>
		public StandardProviderApplicationStore() {
			this.nonceStore = new NonceMemoryStore(DotNetOpenAuthSection.Configuration.OpenId.MaxAuthenticationTime);
		}

		#region INonceStore Members

		/// <summary>
		/// Stores a given nonce and timestamp.
		/// </summary>
		/// <param name="context">The context, or namespace, within which the <paramref name="nonce"/> must be unique.</param>
		/// <param name="nonce">A series of random characters.</param>
		/// <param name="timestampUtc">The timestamp that together with the nonce string make it unique.
		/// The timestamp may also be used by the data store to clear out old nonces.</param>
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
		public bool StoreNonce(string context, string nonce, DateTime timestampUtc) {
			return this.nonceStore.StoreNonce(context, nonce, timestampUtc);
		}

		#endregion
	}
}
