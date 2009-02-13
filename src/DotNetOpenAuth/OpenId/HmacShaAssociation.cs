//-----------------------------------------------------------------------
// <copyright file="HmacShaAssociation.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Security.Cryptography;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;

	/// <summary>
	/// An association that uses the HMAC-SHA family of algorithms for message signing.
	/// </summary>
	internal class HmacShaAssociation : Association {
		/// <summary>
		/// The default lifetime of a shared association when no lifetime is given
		/// for a specific association type.
		/// </summary>
		private static readonly TimeSpan DefaultMaximumLifetime = TimeSpan.FromDays(14);

		/// <summary>
		/// A list of HMAC-SHA algorithms in order of decreasing bit lengths.
		/// </summary>
		private static HmacSha[] hmacShaAssociationTypes = {
			new HmacSha {
				CreateHasher = secretKey => new HMACSHA512(secretKey),
				GetAssociationType = protocol => protocol.Args.SignatureAlgorithm.HMAC_SHA512,
				BaseHashAlgorithm = new SHA512Managed(),
			},
			new HmacSha {
				CreateHasher = secretKey => new HMACSHA384(secretKey),
				GetAssociationType = protocol => protocol.Args.SignatureAlgorithm.HMAC_SHA384,
				BaseHashAlgorithm = new SHA384Managed(),
			},
			new HmacSha {
				CreateHasher = secretKey => new HMACSHA256(secretKey),
				GetAssociationType = protocol => protocol.Args.SignatureAlgorithm.HMAC_SHA256,
				BaseHashAlgorithm = new SHA256Managed(),
			},
			new HmacSha {
				CreateHasher = secretKey => new HMACSHA1(secretKey),
				GetAssociationType = protocol => protocol.Args.SignatureAlgorithm.HMAC_SHA1,
				BaseHashAlgorithm = new SHA1Managed(),
			},
		};

		/// <summary>
		/// The specific variety of HMAC-SHA this association is based on (whether it be HMAC-SHA1, HMAC-SHA256, etc.)
		/// </summary>
		private HmacSha typeIdentity;

		/// <summary>
		/// Initializes a new instance of the <see cref="HmacShaAssociation"/> class.
		/// </summary>
		/// <param name="typeIdentity">The specific variety of HMAC-SHA this association is based on (whether it be HMAC-SHA1, HMAC-SHA256, etc.)</param>
		/// <param name="handle">The association handle.</param>
		/// <param name="secret">The association secret.</param>
		/// <param name="totalLifeLength">The time duration the association will be good for.</param>
		private HmacShaAssociation(HmacSha typeIdentity, string handle, byte[] secret, TimeSpan totalLifeLength)
			: base(handle, secret, totalLifeLength, DateTime.UtcNow) {
			ErrorUtilities.VerifyArgumentNotNull(typeIdentity, "typeIdentity");
			ErrorUtilities.VerifyNonZeroLength(handle, "handle");
			ErrorUtilities.VerifyArgumentNotNull(secret, "secret");
			ErrorUtilities.VerifyProtocol(secret.Length == typeIdentity.SecretLength, OpenIdStrings.AssociationSecretAndTypeLengthMismatch, secret.Length, typeIdentity.GetAssociationType(Protocol.Default));

			this.typeIdentity = typeIdentity;
		}

		/// <summary>
		/// Gets the length (in bits) of the hash this association creates when signing.
		/// </summary>
		public override int HashBitLength {
			get {
				Protocol protocol = Protocol.Default;
				return HmacShaAssociation.GetSecretLength(protocol, this.GetAssociationType(protocol)) * 8;
			}
		}

		/// <summary>
		/// Creates an HMAC-SHA association.
		/// </summary>
		/// <param name="protocol">The OpenID protocol version that the request for an association came in on.</param>
		/// <param name="associationType">The value of the openid.assoc_type parameter.</param>
		/// <param name="handle">The association handle.</param>
		/// <param name="secret">The association secret.</param>
		/// <param name="totalLifeLength">How long the association will be good for.</param>
		/// <returns>The newly created association.</returns>
		public static HmacShaAssociation Create(Protocol protocol, string associationType, string handle, byte[] secret, TimeSpan totalLifeLength) {
			ErrorUtilities.VerifyArgumentNotNull(protocol, "protocol");
			ErrorUtilities.VerifyNonZeroLength(associationType, "associationType");
			ErrorUtilities.VerifyArgumentNotNull(secret, "secret");

			HmacSha match = hmacShaAssociationTypes.FirstOrDefault(sha => String.Equals(sha.GetAssociationType(protocol), associationType, StringComparison.Ordinal));
			ErrorUtilities.VerifyProtocol(match != null, OpenIdStrings.NoAssociationTypeFoundByName, associationType);
			return new HmacShaAssociation(match, handle, secret, totalLifeLength);
		}

		/// <summary>
		/// Creates an association with the specified handle, secret, and lifetime.
		/// </summary>
		/// <param name="handle">The handle.</param>
		/// <param name="secret">The secret.</param>
		/// <param name="totalLifeLength">Total lifetime.</param>
		/// <returns>The newly created association.</returns>
		public static HmacShaAssociation Create(string handle, byte[] secret, TimeSpan totalLifeLength) {
			ErrorUtilities.VerifyNonZeroLength(handle, "handle");
			ErrorUtilities.VerifyArgumentNotNull(secret, "secret");

			HmacSha shaType = hmacShaAssociationTypes.FirstOrDefault(sha => sha.SecretLength == secret.Length);
			ErrorUtilities.VerifyProtocol(shaType != null, OpenIdStrings.NoAssociationTypeFoundByLength, secret.Length);
			return new HmacShaAssociation(shaType, handle, secret, totalLifeLength);
		}

		/// <summary>
		/// Returns the length of the shared secret (in bytes).
		/// </summary>
		/// <param name="protocol">The protocol version being used that will be used to lookup the text in <paramref name="associationType"/></param>
		/// <param name="associationType">The value of the protocol argument specifying the type of association.  For example: "HMAC-SHA1".</param>
		/// <returns>The length (in bytes) of the association secret.</returns>
		/// <exception cref="ProtocolException">Thrown if no association can be found by the given name.</exception>
		public static int GetSecretLength(Protocol protocol, string associationType) {
			HmacSha match = hmacShaAssociationTypes.FirstOrDefault(shaType => String.Equals(shaType.GetAssociationType(protocol), associationType, StringComparison.Ordinal));
			ErrorUtilities.VerifyProtocol(match != null, OpenIdStrings.NoAssociationTypeFoundByName, associationType);
			return match.SecretLength;
		}

		/// <summary>
		/// Creates a new association of a given type.
		/// </summary>
		/// <param name="protocol">The protocol.</param>
		/// <param name="associationType">Type of the association.</param>
		/// <param name="associationUse">A value indicating whether the new association will be used privately by the Provider for "dumb mode" authentication
		/// or shared with the Relying Party for "smart mode" authentication.</param>
		/// <param name="securitySettings">The security settings of the Provider.</param>
		/// <returns>The newly created association.</returns>
		/// <remarks>
		/// The new association is NOT automatically put into an association store.  This must be done by the caller.
		/// </remarks>
		internal static HmacShaAssociation Create(Protocol protocol, string associationType, AssociationRelyingPartyType associationUse, ProviderSecuritySettings securitySettings) {
			ErrorUtilities.VerifyArgumentNotNull(protocol, "protocol");
			ErrorUtilities.VerifyNonZeroLength(associationType, "associationType");
			ErrorUtilities.VerifyArgumentNotNull(securitySettings, "securitySettings");

			// Generate the handle.  It must be unique, and preferably unpredictable,
			// so we use a time element and a random data element to generate it.
			string uniq = MessagingUtilities.GetCryptoRandomDataAsBase64(4);
			string handle = "{" + associationType + "}{" + DateTime.UtcNow.Ticks + "}{" + uniq + "}";

			// Generate the secret that will be used for signing
			int secretLength = GetSecretLength(protocol, associationType);
			byte[] secret = MessagingUtilities.GetCryptoRandomData(secretLength);

			TimeSpan lifetime;
			if (associationUse == AssociationRelyingPartyType.Smart) {
				if (!securitySettings.AssociationLifetimes.TryGetValue(associationType, out lifetime)) {
					lifetime = DefaultMaximumLifetime;
				}
			} else {
				lifetime = DumbSecretLifetime;
			}

			return Create(protocol, associationType, handle, secret, lifetime);
		}

		/// <summary>
		/// Looks for the first association type in a preferred-order list that is
		/// likely to be supported given a specific OpenID version and the security settings,
		/// and perhaps a matching Diffie-Hellman session type.
		/// </summary>
		/// <param name="protocol">The OpenID version that dictates which associations are available.</param>
		/// <param name="highSecurityIsBetter">A value indicating whether to consider higher strength security to be better.  Use <c>true</c> for initial association requests from the Relying Party; use <c>false</c> from Providers when the Relying Party asks for an unrecognized association in order to pick a suggested alternative that is likely to be supported on both sides.</param>
		/// <param name="securityRequirements">The set of requirements the selected association type must comply to.</param>
		/// <param name="requireMatchingDHSessionType">Use <c>true</c> for HTTP associations, <c>false</c> for HTTPS associations.</param>
		/// <param name="associationType">The resulting association type's well known protocol name.  (i.e. HMAC-SHA256)</param>
		/// <param name="sessionType">The resulting session type's well known protocol name, if a matching one is available.  (i.e. DH-SHA256)</param>
		/// <returns>
		/// True if a qualifying association could be found; false otherwise.
		/// </returns>
		internal static bool TryFindBestAssociation(Protocol protocol, bool highSecurityIsBetter, SecuritySettings securityRequirements, bool requireMatchingDHSessionType, out string associationType, out string sessionType) {
			ErrorUtilities.VerifyArgumentNotNull(protocol, "protocol");
			ErrorUtilities.VerifyArgumentNotNull(securityRequirements, "securityRequirements");

			associationType = null;
			sessionType = null;

			// We use AsEnumerable() to avoid VerificationException (http://stackoverflow.com/questions/478422/why-does-simple-array-and-linq-generate-verificationexception-operation-could-de)
			IEnumerable<HmacSha> preferredOrder = highSecurityIsBetter ?
				hmacShaAssociationTypes.AsEnumerable() : hmacShaAssociationTypes.Reverse();

			foreach (HmacSha sha in preferredOrder) {
				int hashSizeInBits = sha.SecretLength * 8;
				if (hashSizeInBits > securityRequirements.MaximumHashBitLength ||
					hashSizeInBits < securityRequirements.MinimumHashBitLength) {
					continue;
				}
				sessionType = DiffieHellmanUtilities.GetNameForSize(protocol, hashSizeInBits);
				if (requireMatchingDHSessionType && sessionType == null) {
					continue;
				}
				associationType = sha.GetAssociationType(protocol);
				if (associationType == null) {
					continue;
				}

				return true;
			}

			return false;
		}

		/// <summary>
		/// Determines whether a named Diffie-Hellman session type and association type can be used together.
		/// </summary>
		/// <param name="protocol">The protocol carrying the names of the session and association types.</param>
		/// <param name="associationType">The value of the openid.assoc_type parameter.</param>
		/// <param name="sessionType">The value of the openid.session_type parameter.</param>
		/// <returns>
		/// 	<c>true</c> if the named association and session types are compatible; otherwise, <c>false</c>.
		/// </returns>
		internal static bool IsDHSessionCompatible(Protocol protocol, string associationType, string sessionType) {
			ErrorUtilities.VerifyArgumentNotNull(protocol, "protocol");
			ErrorUtilities.VerifyNonZeroLength(associationType, "associationType");
			ErrorUtilities.VerifyArgumentNotNull(sessionType, "sessionType");

			// All association types can work when no DH session is used at all.
			if (string.Equals(sessionType, protocol.Args.SessionType.NoEncryption, StringComparison.Ordinal)) {
				return true;
			}

			// When there _is_ a DH session, it must match in hash length with the association type.
			int associationSecretLengthInBytes = GetSecretLength(protocol, associationType);
			int sessionHashLengthInBytes = DiffieHellmanUtilities.Lookup(protocol, sessionType).HashSize / 8;
			return associationSecretLengthInBytes == sessionHashLengthInBytes;
		}

		/// <summary>
		/// Gets the string to pass as the assoc_type value in the OpenID protocol.
		/// </summary>
		/// <param name="protocol">The protocol version of the message that the assoc_type value will be included in.</param>
		/// <returns>
		/// The value that should be used for  the openid.assoc_type parameter.
		/// </returns>
		internal override string GetAssociationType(Protocol protocol) {
			return this.typeIdentity.GetAssociationType(protocol);
		}

		/// <summary>
		/// Returns the specific hash algorithm used for message signing.
		/// </summary>
		/// <returns>
		/// The hash algorithm used for message signing.
		/// </returns>
		protected override HashAlgorithm CreateHasher() {
			return this.typeIdentity.CreateHasher(SecretKey);
		}

		/// <summary>
		/// Provides information about some HMAC-SHA hashing algorithm that OpenID supports.
		/// </summary>
		private class HmacSha {
			/// <summary>
			/// Gets or sets the function that takes a particular OpenID version and returns the value of the openid.assoc_type parameter in that protocol.
			/// </summary>
			internal Func<Protocol, string> GetAssociationType { get; set; }

			/// <summary>
			/// Gets or sets a function that will create the <see cref="HashAlgorithm"/> using a given shared secret for the mac.
			/// </summary>
			internal Func<byte[], HashAlgorithm> CreateHasher { get; set; }

			/// <summary>
			/// Gets or sets the base hash algorithm.
			/// </summary>
			internal HashAlgorithm BaseHashAlgorithm { get; set; }

			/// <summary>
			/// Gets the size of the hash (in bytes).
			/// </summary>
			internal int SecretLength { get { return this.BaseHashAlgorithm.HashSize / 8; } }
		}
	}
}