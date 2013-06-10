//-----------------------------------------------------------------------
// <copyright file="Association.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using System.Security.Cryptography;
	using System.Text;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// Stores a secret used in signing and verifying messages.
	/// </summary>
	/// <remarks>
	/// OpenID associations may be shared between Provider and Relying Party (smart
	/// associations), or be a way for a Provider to recall its own secret for later
	/// (dumb associations).
	/// </remarks>
	[DebuggerDisplay("Handle = {Handle}, Expires = {Expires}")]
	public abstract class Association {
		/// <summary>
		/// Initializes a new instance of the <see cref="Association"/> class.
		/// </summary>
		/// <param name="handle">The handle.</param>
		/// <param name="secret">The secret.</param>
		/// <param name="totalLifeLength">How long the association will be useful.</param>
		/// <param name="issued">The UTC time of when this association was originally issued by the Provider.</param>
		protected Association(string handle, byte[] secret, TimeSpan totalLifeLength, DateTime issued) {
			Requires.NotNullOrEmpty(handle, "handle");
			Requires.NotNull(secret, "secret");
			Requires.Range(totalLifeLength > TimeSpan.Zero, "totalLifeLength");
			Requires.That(issued.Kind == DateTimeKind.Utc, "issued", "UTC time required.");
			Requires.Range(issued <= DateTime.UtcNow, "issued");

			this.Handle = handle;
			this.SecretKey = secret;
			this.TotalLifeLength = totalLifeLength;
			this.Issued = OpenIdUtilities.CutToSecond(issued);
		}

		/// <summary>
		/// Gets a unique handle by which this <see cref="Association"/> may be stored or retrieved.
		/// </summary>
		public string Handle { get; internal set; }

		/// <summary>
		/// Gets the UTC time when this <see cref="Association"/> will expire.
		/// </summary>
		public DateTime Expires {
			get { return this.Issued + this.TotalLifeLength; }
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="Association"/> has already expired.
		/// </summary>
		public bool IsExpired {
			get { return this.Expires < DateTime.UtcNow; }
		}

		/// <summary>
		/// Gets the length (in bits) of the hash this association creates when signing.
		/// </summary>
		public abstract int HashBitLength { get; }

		/// <summary>
		/// Gets a value indicating whether this instance has useful life remaining.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance has useful life remaining; otherwise, <c>false</c>.
		/// </value>
		internal bool HasUsefulLifeRemaining {
			get { return this.TimeTillExpiration >= MinimumUsefulAssociationLifetime; }
		}

		/// <summary>
		/// Gets or sets the UTC time that this <see cref="Association"/> was first created.
		/// </summary>
		[MessagePart]
		internal DateTime Issued { get; set; }

		/// <summary>
		/// Gets the duration a secret key used for signing dumb client requests will be good for.
		/// </summary>
		protected internal static TimeSpan DumbSecretLifetime {
			get {
				return OpenIdElement.Configuration.MaxAuthenticationTime;
			}
		}

		/// <summary>
		/// Gets the number of seconds until this <see cref="Association"/> expires.
		/// Never negative (counter runs to zero).
		/// </summary>
		protected internal long SecondsTillExpiration {
			get {
				return Math.Max(0, (long)this.TimeTillExpiration.TotalSeconds);
			}
		}

		/// <summary>
		/// Gets the shared secret key between the consumer and provider.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "It is a buffer.")]
		[MessagePart("key")]
		protected internal byte[] SecretKey { get; private set; }

		/// <summary>
		/// Gets the lifetime the OpenID provider permits this <see cref="Association"/>.
		/// </summary>
		[MessagePart("ttl")]
		protected TimeSpan TotalLifeLength { get; private set; }

		/// <summary>
		/// Gets the minimum lifetime an association must still be good for in order for it to be used for a future authentication.
		/// </summary>
		/// <remarks>
		/// Associations that are not likely to last the duration of a user login are not worth using at all.
		/// </remarks>
		private static TimeSpan MinimumUsefulAssociationLifetime {
			get {
				return OpenIdElement.Configuration.MaxAuthenticationTime;
			}
		}

		/// <summary>
		/// Gets the TimeSpan till this association expires.
		/// </summary>
		private TimeSpan TimeTillExpiration {
			get { return this.Expires - DateTime.UtcNow; }
		}

		/// <summary>
		/// Re-instantiates an <see cref="Association"/> previously persisted in a database or some
		/// other shared store.
		/// </summary>
		/// <param name="handle">
		/// The <see cref="Handle"/> property of the previous <see cref="Association"/> instance.
		/// </param>
		/// <param name="expiresUtc">
		/// The UTC value of the <see cref="Expires"/> property of the previous <see cref="Association"/> instance.
		/// </param>
		/// <param name="privateData">
		/// The byte array returned by a call to <see cref="SerializePrivateData"/> on the previous
		/// <see cref="Association"/> instance.
		/// </param>
		/// <returns>
		/// The newly dehydrated <see cref="Association"/>, which can be returned
		/// from a custom association store's 
		/// IRelyingPartyAssociationStore.GetAssociation method.
		/// </returns>
		public static Association Deserialize(string handle, DateTime expiresUtc, byte[] privateData) {
			Requires.NotNullOrEmpty(handle, "handle");
			Requires.NotNull(privateData, "privateData");

			expiresUtc = expiresUtc.ToUniversalTimeSafe();
			TimeSpan remainingLifeLength = expiresUtc - DateTime.UtcNow;
			byte[] secret = privateData; // the whole of privateData is the secret key for now.
			// We figure out what derived type to instantiate based on the length of the secret.
			try {
				return HmacShaAssociation.Create(handle, secret, remainingLifeLength);
			} catch (ArgumentException ex) {
				throw new ArgumentException(OpenIdStrings.BadAssociationPrivateData, "privateData", ex);
			}
		}

		/// <summary>
		/// Returns private data required to persist this <see cref="Association"/> in
		/// permanent storage (a shared database for example) for deserialization later.
		/// </summary>
		/// <returns>
		/// An opaque byte array that must be stored and returned exactly as it is provided here.
		/// The byte array may vary in length depending on the specific type of <see cref="Association"/>,
		/// but in current versions are no larger than 256 bytes.
		/// </returns>
		/// <remarks>
		/// Values of public properties on the base class <see cref="Association"/> are not included
		/// in this byte array, as they are useful for fast database lookup and are persisted separately.
		/// </remarks>
		public byte[] SerializePrivateData() {
			// We may want to encrypt this secret using the machine.config private key,
			// and add data regarding which Association derivative will need to be
			// re-instantiated on deserialization.
			// For now, we just send out the secret key.  We can derive the type from the length later.
			byte[] secretKeyCopy = new byte[this.SecretKey.Length];
			if (this.SecretKey.Length > 0) {
				this.SecretKey.CopyTo(secretKeyCopy, 0);
			}
			return secretKeyCopy;
		}

		/// <summary>
		/// Tests equality of two <see cref="Association"/> objects.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
		/// <returns>
		/// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
		/// </returns>
		public override bool Equals(object obj) {
			Association a = obj as Association;
			if (a == null) {
				return false;
			}
			if (a.GetType() != GetType()) {
				return false;
			}

			if (a.Handle != this.Handle ||
				a.Issued != this.Issued ||
				!MessagingUtilities.Equals(a.TotalLifeLength, this.TotalLifeLength, TimeSpan.FromSeconds(2))) {
				return false;
			}

			if (!MessagingUtilities.AreEquivalent(a.SecretKey, this.SecretKey)) {
				return false;
			}

			return true;
		}

		/// <summary>
		/// Returns the hash code.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		public override int GetHashCode() {
			var hmac = HmacAlgorithms.Create(HmacAlgorithms.HmacSha1, this.SecretKey);
			try {
				CryptoStream cs = new CryptoStream(Stream.Null, hmac, CryptoStreamMode.Write);

				byte[] hbytes = ASCIIEncoding.ASCII.GetBytes(this.Handle);

				cs.Write(hbytes, 0, hbytes.Length);
				cs.Close();

				byte[] hash = hmac.Hash;
				hmac.Clear();

				long val = 0;
				for (int i = 0; i < hash.Length; i++) {
					val = val ^ (long)hash[i];
				}

				val = val ^ this.Expires.ToFileTimeUtc();

				return (int)val;
			} finally {
				((IDisposable)hmac).Dispose();
			}
		}

		/// <summary>
		/// The string to pass as the assoc_type value in the OpenID protocol.
		/// </summary>
		/// <param name="protocol">The protocol version of the message that the assoc_type value will be included in.</param>
		/// <returns>The value that should be used for  the openid.assoc_type parameter.</returns>
		internal abstract string GetAssociationType(Protocol protocol);

		/// <summary>
		/// Generates a signature from a given blob of data.
		/// </summary>
		/// <param name="data">The data to sign.  This data will not be changed (the signature is the return value).</param>
		/// <returns>The calculated signature of the data.</returns>
		protected internal byte[] Sign(byte[] data) {
			Requires.NotNull(data, "data");
			using (HashAlgorithm hasher = this.CreateHasher()) {
				return hasher.ComputeHash(data);
			}
		}

		/// <summary>
		/// Returns the specific hash algorithm used for message signing.
		/// </summary>
		/// <returns>The hash algorithm used for message signing.</returns>
		protected abstract HashAlgorithm CreateHasher();

#if CONTRACTS_FULL
		/// <summary>
		/// Verifies conditions that should be true for any valid state of this object.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Called by code contracts.")]
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called by code contracts.")]
		[ContractInvariantMethod]
		private void ObjectInvariant() {
			Contract.Invariant(!string.IsNullOrEmpty(this.Handle));
			Contract.Invariant(this.TotalLifeLength > TimeSpan.Zero);
			Contract.Invariant(this.SecretKey != null);
		}
#endif
	}
}
