//-----------------------------------------------------------------------
// <copyright file="PrivatePersonalIdentifierProviderBase.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// Provides standard PPID Identifiers to users to protect their identity from individual relying parties
	/// and from colluding groups of relying parties.
	/// </summary>
	public abstract class PrivatePersonalIdentifierProviderBase : IDirectedIdentityIdentifierProvider {
		/// <summary>
		/// The type of hash function to use for the <see cref="Hasher"/> property.
		/// </summary>
		private const string HashAlgorithmName = "SHA256";

		/// <summary>
		/// The length of the salt to generate for first time PPID-users.
		/// </summary>
		private int newSaltLength = 20;

		/// <summary>
		/// Initializes a new instance of the <see cref="PrivatePersonalIdentifierProviderBase"/> class.
		/// </summary>
		/// <param name="baseIdentifier">The base URI on which to append the anonymous part.</param>
		protected PrivatePersonalIdentifierProviderBase(Uri baseIdentifier) {
			Requires.NotNull(baseIdentifier, "baseIdentifier");

			this.Hasher = HashAlgorithm.Create(HashAlgorithmName);
			this.Encoder = Encoding.UTF8;
			this.BaseIdentifier = baseIdentifier;
			this.PairwiseUnique = AudienceScope.Realm;
		}

		/// <summary>
		/// A granularity description for who wide of an audience sees the same generated PPID.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "Breaking change")]
		public enum AudienceScope {
			/// <summary>
			/// A unique Identifier is generated for every realm.  This is the highest security setting.
			/// </summary>
			Realm,

			/// <summary>
			/// Only the host name in the realm is used in calculating the PPID,
			/// allowing for some level of sharing of the PPID Identifiers between RPs
			/// that are able to share the same realm host value.
			/// </summary>
			RealmHost,

			/// <summary>
			/// Although the user's Identifier is still opaque to the RP so they cannot determine
			/// who the user is at the OP, the same Identifier is used at all RPs so collusion
			/// between the RPs is possible.
			/// </summary>
			Global,
		}

		/// <summary>
		/// Gets the base URI on which to append the anonymous part.
		/// </summary>
		public Uri BaseIdentifier { get; private set; }

		/// <summary>
		/// Gets or sets a value indicating whether each Realm will get its own private identifier
		/// for the authenticating uesr.
		/// </summary>
		/// <value>The default value is <see cref="AudienceScope.Realm"/>.</value>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pairwise", Justification = "Meaningful word")]
		public AudienceScope PairwiseUnique { get; set; }

		/// <summary>
		/// Gets the hash function to use to perform the one-way transform of a personal identifier
		/// to an "anonymous" looking one.
		/// </summary>
		protected HashAlgorithm Hasher { get; private set; }

		/// <summary>
		/// Gets the encoder to use for transforming the personal identifier into bytes for hashing.
		/// </summary>
		protected Encoding Encoder { get; private set; }

		/// <summary>
		/// Gets or sets the new length of the salt.
		/// </summary>
		/// <value>The new length of the salt.</value>
		protected int NewSaltLength {
			get {
				return this.newSaltLength;
			}

			set {
				Requires.Range(value > 0, "value");
				this.newSaltLength = value;
			}
		}

		#region IDirectedIdentityIdentifierProvider Members

		/// <summary>
		/// Gets the Identifier to use for the Claimed Identifier and Local Identifier of
		/// an outgoing positive assertion.
		/// </summary>
		/// <param name="localIdentifier">The OP local identifier for the authenticating user.</param>
		/// <param name="relyingPartyRealm">The realm of the relying party receiving the assertion.</param>
		/// <returns>
		/// A valid, discoverable OpenID Identifier that should be used as the value for the
		/// openid.claimed_id and openid.local_id parameters.  Must not be null.
		/// </returns>
		public Uri GetIdentifier(Identifier localIdentifier, Realm relyingPartyRealm) {
			byte[] salt = this.GetHashSaltForLocalIdentifier(localIdentifier);
			string valueToHash = localIdentifier + "#";
			switch (this.PairwiseUnique) {
				case AudienceScope.Realm:
					valueToHash += relyingPartyRealm;
					break;
				case AudienceScope.RealmHost:
					valueToHash += relyingPartyRealm.Host;
					break;
				case AudienceScope.Global:
					break;
				default:
					throw new InvalidOperationException(
						string.Format(
							CultureInfo.CurrentCulture,
							OpenIdStrings.UnexpectedEnumPropertyValue,
							"PairwiseUnique",
							this.PairwiseUnique));
			}

			byte[] valueAsBytes = this.Encoder.GetBytes(valueToHash);
			byte[] bytesToHash = new byte[valueAsBytes.Length + salt.Length];
			valueAsBytes.CopyTo(bytesToHash, 0);
			salt.CopyTo(bytesToHash, valueAsBytes.Length);
			byte[] hash = this.Hasher.ComputeHash(bytesToHash);
			string base64Hash = Convert.ToBase64String(hash);
			Uri anonymousIdentifier = this.AppendIdentifiers(base64Hash);
			return anonymousIdentifier;
		}

		/// <summary>
		/// Determines whether a given identifier is the primary (non-PPID) local identifier for some user.
		/// </summary>
		/// <param name="identifier">The identifier in question.</param>
		/// <returns>
		/// 	<c>true</c> if the given identifier is the valid, unique identifier for some uesr (and NOT a PPID); otherwise, <c>false</c>.
		/// </returns>
		public virtual bool IsUserLocalIdentifier(Identifier identifier) {
			return !identifier.ToString().StartsWith(this.BaseIdentifier.AbsoluteUri, StringComparison.Ordinal);
		}

		#endregion

		/// <summary>
		/// Creates a new salt to assign to a user.
		/// </summary>
		/// <returns>A non-null buffer of length <see cref="NewSaltLength"/> filled with a random salt.</returns>
		protected virtual byte[] CreateSalt() {
			// We COULD use a crypto random function, but for a salt it seems overkill.
			return MessagingUtilities.GetNonCryptoRandomData(this.NewSaltLength);
		}

		/// <summary>
		/// Creates a new PPID Identifier by appending a pseudonymous identifier suffix to
		/// the <see cref="BaseIdentifier"/>.
		/// </summary>
		/// <param name="uriHash">The unique part of the Identifier to append to the common first part.</param>
		/// <returns>The full PPID Identifier.</returns>
		[SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "NOT equivalent overload.  The recommended one breaks on relative URIs.")]
		protected virtual Uri AppendIdentifiers(string uriHash) {
			Requires.NotNullOrEmpty(uriHash, "uriHash");

			if (string.IsNullOrEmpty(this.BaseIdentifier.Query)) {
				// The uriHash will appear on the path itself.
				string pathEncoded = Uri.EscapeUriString(uriHash.Replace('/', '_'));
				return new Uri(this.BaseIdentifier, pathEncoded);
			} else {
				// The uriHash will appear on the query string.
				string dataEncoded = Uri.EscapeDataString(uriHash);
				return new Uri(this.BaseIdentifier + dataEncoded);
			}
		}

		/// <summary>
		/// Gets the salt to use for generating an anonymous identifier for a given OP local identifier.
		/// </summary>
		/// <param name="localIdentifier">The OP local identifier.</param>
		/// <returns>The salt to use in the hash.</returns>
		/// <remarks>
		/// It is important that this method always return the same value for a given 
		/// <paramref name="localIdentifier"/>.  
		/// New salts can be generated for local identifiers without previously assigned salt
		/// values by calling <see cref="CreateSalt"/> or by a custom method.
		/// </remarks>
		protected abstract byte[] GetHashSaltForLocalIdentifier(Identifier localIdentifier);

#if CONTRACTS_FULL
		/// <summary>
		/// Verifies conditions that should be true for any valid state of this object.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called by code contracts.")]
		[ContractInvariantMethod]
		private void ObjectInvariant() {
			Contract.Invariant(this.Hasher != null);
			Contract.Invariant(this.Encoder != null);
			Contract.Invariant(this.BaseIdentifier != null);
			Contract.Invariant(this.NewSaltLength > 0);
		}
#endif
	}
}
