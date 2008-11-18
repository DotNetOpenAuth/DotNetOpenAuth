//-----------------------------------------------------------------------
// <copyright file="HmacShaAssociation.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Diagnostics;
	using System.Linq;
	using System.Security.Cryptography;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// An association that uses the HMAC-SHA family of algorithms for message signing.
	/// </summary>
	internal class HmacShaAssociation : Association {
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
		/// <param name="associationUse">
		/// A value indicating whether the new association will be used privately by the Provider for "dumb mode" authentication
		/// or shared with the Relying Party for "smart mode" authentication.
		/// </param>
		/// <returns>The newly created association.</returns>
		/// <remarks>
		/// The new association is NOT automatically put into an association store.  This must be done by the caller.
		/// </remarks>
		internal static HmacShaAssociation Create(Protocol protocol, string associationType, AssociationRelyingPartyType associationUse) {
			// Generate the handle.  It must be unique, so we use a time element and a random data element to generate it.
			byte[] uniq_bytes = MessagingUtilities.GetCryptoRandomData(4);
			string uniq = Convert.ToBase64String(uniq_bytes);
			string handle = "{" + associationType + "}{" + DateTime.UtcNow.Ticks + "}{" + uniq + "}";

			// Generate the secret that will be used for signing
			int secretLength = GetSecretLength(protocol, associationType);
			byte[] secret = MessagingUtilities.GetCryptoRandomData(secretLength);

			TimeSpan lifetime = associationUse == AssociationRelyingPartyType.Smart ? SmartAssociationLifetime : DumbSecretLifetime;

			return Create(protocol, associationType, handle, secret, lifetime);
		}

		/// <summary>
		/// Looks for the longest hash length for a given protocol for which we have an association,
		/// and perhaps a matching Diffie-Hellman session type.
		/// </summary>
		/// <param name="protocol">The OpenID version that dictates which associations are available.</param>
		/// <param name="minimumHashSizeInBits">The minimum required hash length given security settings.</param>
		/// <param name="maximumHashSizeInBits">The maximum hash length to even attempt.  Useful for the RP side where we support SHA512 but most OPs do not -- why waste time trying?</param>
		/// <param name="requireMatchingDHSessionType">True for HTTP associations, False for HTTPS associations.</param>
		/// <param name="associationType">The resulting association type's well known protocol name.  (i.e. HMAC-SHA256)</param>
		/// <param name="sessionType">The resulting session type's well known protocol name, if a matching one is available.  (i.e. DH-SHA256)</param>
		/// <returns>True if a qualifying association could be found; false otherwise.</returns>
		internal static bool TryFindBestAssociation(Protocol protocol, int? minimumHashSizeInBits, int? maximumHashSizeInBits, bool requireMatchingDHSessionType, out string associationType, out string sessionType) {
			ErrorUtilities.VerifyArgumentNotNull(protocol, "protocol");
			associationType = null;
			sessionType = null;

			// We assume this enumeration is in decreasing bit length order.
			foreach (HmacSha sha in hmacShaAssociationTypes) {
				int hashSizeInBits = sha.SecretLength * 8;
				if (maximumHashSizeInBits.HasValue && hashSizeInBits > maximumHashSizeInBits.Value) {
					continue;
				}
				if (minimumHashSizeInBits.HasValue && hashSizeInBits < minimumHashSizeInBits.Value) {
					break;
				}
				sessionType = DiffieHellmanUtilities.GetNameForSize(protocol, hashSizeInBits);
				if (requireMatchingDHSessionType && sessionType == null) {
					continue;
				}
				associationType = sha.GetAssociationType(protocol);
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