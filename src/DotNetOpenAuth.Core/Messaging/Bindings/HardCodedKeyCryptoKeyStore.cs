//-----------------------------------------------------------------------
// <copyright file="HardCodedKeyCryptoKeyStore.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging.Bindings {
	using System;
	using System.Collections.Generic;
	using Validation;

	/// <summary>
	/// A trivial implementation of <see cref="ICryptoKeyStore"/> that has only one fixed key.
	/// This is meant for simple, low-security applications. Greater security requires an
	/// implementation of <see cref="ICryptoKeyStore"/> that actually stores and retrieves
	/// keys from a persistent store.
	/// </summary>
	public class HardCodedKeyCryptoKeyStore : ICryptoKeyStore {
		/// <summary>
		/// The handle to report for the hard-coded key.
		/// </summary>
		private const string HardCodedKeyHandle = "fxd";

		/// <summary>
		/// The one crypto key singleton instance.
		/// </summary>
		private readonly CryptoKey OneCryptoKey;

		/// <summary>
		/// Initializes a new instance of the <see cref="HardCodedKeyCryptoKeyStore"/> class.
		/// </summary>
		/// <param name="secretAsBase64">The 256-bit secret as a base64 encoded string.</param>
		public HardCodedKeyCryptoKeyStore(string secretAsBase64)
			: this(Convert.FromBase64String(Requires.NotNull(secretAsBase64, "secretAsBase64"))) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HardCodedKeyCryptoKeyStore"/> class.
		/// </summary>
		/// <param name="secret">The 256-bit secret.</param>
		public HardCodedKeyCryptoKeyStore(byte[] secret) {
			Requires.NotNull(secret, "secret");
			this.OneCryptoKey = new CryptoKey(secret, DateTime.MaxValue.AddDays(-2).ToUniversalTime());
		}

		#region ICryptoKeyStore Members

		/// <summary>
		/// Gets the key in a given bucket and handle.
		/// </summary>
		/// <param name="bucket">The bucket name.  Case sensitive.</param>
		/// <param name="handle">The key handle.  Case sensitive.</param>
		/// <returns>
		/// The cryptographic key, or <c>null</c> if no matching key was found.
		/// </returns>
		public CryptoKey GetKey(string bucket, string handle) {
			if (handle == HardCodedKeyHandle) {
				return this.OneCryptoKey;
			}

			return null;
		}

		/// <summary>
		/// Gets a sequence of existing keys within a given bucket.
		/// </summary>
		/// <param name="bucket">The bucket name.  Case sensitive.</param>
		/// <returns>
		/// A sequence of handles and keys, ordered by descending <see cref="CryptoKey.ExpiresUtc" />.
		/// </returns>
		public IEnumerable<KeyValuePair<string, CryptoKey>> GetKeys(string bucket) {
			return new[] { new KeyValuePair<string, CryptoKey>(HardCodedKeyHandle, this.OneCryptoKey) };
		}

		/// <summary>
		/// Stores a cryptographic key.
		/// </summary>
		/// <param name="bucket">The name of the bucket to store the key in.  Case sensitive.</param>
		/// <param name="handle">The handle to the key, unique within the bucket.  Case sensitive.</param>
		/// <param name="key">The key to store.</param>
		/// <exception cref="System.NotSupportedException">Always thrown.</exception>
		public void StoreKey(string bucket, string handle, CryptoKey key) {
			throw new NotSupportedException();
		}

		/// <summary>
		/// Removes the key.
		/// </summary>
		/// <param name="bucket">The bucket name.  Case sensitive.</param>
		/// <param name="handle">The key handle.  Case sensitive.</param>
		/// <exception cref="System.NotSupportedException">Always thrown.</exception>
		public void RemoveKey(string bucket, string handle) {
			throw new NotSupportedException();
		}

		#endregion
	}
}