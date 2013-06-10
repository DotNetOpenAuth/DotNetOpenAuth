//-----------------------------------------------------------------------
// <copyright file="MemoryCryptoKeyAndNonceStore.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging.Bindings {
	using System;
	using System.Collections.Generic;
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
	/// <see cref="ICryptoKeyAndNonceStore"/> interface to use instead of this
	/// class.
	/// </remarks>
	public class MemoryCryptoKeyAndNonceStore : ICryptoKeyAndNonceStore {
		/// <summary>
		/// The nonce store to use.
		/// </summary>
		private readonly INonceStore nonceStore;

		/// <summary>
		/// The crypto key store where symmetric keys are persisted.
		/// </summary>
		private readonly ICryptoKeyStore cryptoKeyStore;

		/// <summary>
		/// Initializes a new instance of the <see cref="MemoryCryptoKeyAndNonceStore" /> class
		/// with a default max nonce lifetime of 5 minutes.
		/// </summary>
		public MemoryCryptoKeyAndNonceStore()
			: this(TimeSpan.FromMinutes(5)) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MemoryCryptoKeyAndNonceStore"/> class.
		/// </summary>
		/// <param name="maximumMessageAge">The maximum time to live of a message that might carry a nonce.</param>
		public MemoryCryptoKeyAndNonceStore(TimeSpan maximumMessageAge) {
			this.nonceStore = new MemoryNonceStore(maximumMessageAge);
			this.cryptoKeyStore = new MemoryCryptoKeyStore();
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

		#region ICryptoKeyStore

		/// <summary>
		/// Gets the key in a given bucket and handle.
		/// </summary>
		/// <param name="bucket">The bucket name.  Case sensitive.</param>
		/// <param name="handle">The key handle.  Case sensitive.</param>
		/// <returns>
		/// The cryptographic key, or <c>null</c> if no matching key was found.
		/// </returns>
		public CryptoKey GetKey(string bucket, string handle) {
			return this.cryptoKeyStore.GetKey(bucket, handle);
		}

		/// <summary>
		/// Gets a sequence of existing keys within a given bucket.
		/// </summary>
		/// <param name="bucket">The bucket name.  Case sensitive.</param>
		/// <returns>
		/// A sequence of handles and keys, ordered by descending <see cref="CryptoKey.ExpiresUtc"/>.
		/// </returns>
		public IEnumerable<KeyValuePair<string, CryptoKey>> GetKeys(string bucket) {
			return this.cryptoKeyStore.GetKeys(bucket);
		}

		/// <summary>
		/// Stores a cryptographic key.
		/// </summary>
		/// <param name="bucket">The name of the bucket to store the key in.  Case sensitive.</param>
		/// <param name="handle">The handle to the key, unique within the bucket.  Case sensitive.</param>
		/// <param name="key">The key to store.</param>
		/// <exception cref="CryptoKeyCollisionException">Thrown in the event of a conflict with an existing key in the same bucket and with the same handle.</exception>
		public void StoreKey(string bucket, string handle, CryptoKey key) {
			this.cryptoKeyStore.StoreKey(bucket, handle, key);
		}

		/// <summary>
		/// Removes the key.
		/// </summary>
		/// <param name="bucket">The bucket name.  Case sensitive.</param>
		/// <param name="handle">The key handle.  Case sensitive.</param>
		public void RemoveKey(string bucket, string handle) {
			this.cryptoKeyStore.RemoveKey(bucket, handle);
		}

		#endregion
	}
}
