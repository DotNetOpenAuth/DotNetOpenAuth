//-----------------------------------------------------------------------
// <copyright file="CryptoKey.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging.Bindings {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// A cryptographic key and metadata concerning it.
	/// </summary>
	public class CryptoKey {
		/// <summary>
		/// Backing field for the <see cref="Key"/> property.
		/// </summary>
		private readonly byte[] key;

		/// <summary>
		/// Backing field for the <see cref="ExpiresUtc"/> property.
		/// </summary>
		private readonly DateTime expiresUtc;

		/// <summary>
		/// Initializes a new instance of the <see cref="CryptoKey"/> class.
		/// </summary>
		/// <param name="key">The cryptographic key.</param>
		/// <param name="expiresUtc">The expires UTC.</param>
		public CryptoKey(byte[] key, DateTime expiresUtc) {
			Requires.NotNull(key, "key");
			Requires.That(expiresUtc.Kind == DateTimeKind.Utc, "expiresUtc", "Time must be expressed in UTC.");
			this.key = key;
			this.expiresUtc = expiresUtc;
		}

		/// <summary>
		/// Gets the key.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "It's a buffer")]
		public byte[] Key {
			get {
				return this.key;
			}
		}

		/// <summary>
		/// Gets the expiration date of this key (UTC time).
		/// </summary>
		public DateTime ExpiresUtc {
			get {
				return this.expiresUtc;
			}
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="T:System.NullReferenceException">
		/// The <paramref name="obj"/> parameter is null.
		/// </exception>
		public override bool Equals(object obj) {
			var other = obj as CryptoKey;
			if (other == null) {
				return false;
			}

			return this.ExpiresUtc == other.ExpiresUtc
				&& MessagingUtilities.AreEquivalent(this.Key, other.Key);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode() {
			return this.ExpiresUtc.GetHashCode();
		}
	}
}
