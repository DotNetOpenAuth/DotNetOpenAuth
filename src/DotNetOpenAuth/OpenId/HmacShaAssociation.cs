//-----------------------------------------------------------------------
// <copyright file="HmacShaAssociation.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Diagnostics;
	using System.Security.Cryptography;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;

	internal class HmacShaAssociation : Association {
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

		private HmacSha typeIdentity;

		/// <summary>
		/// Initializes a new instance of the <see cref="HmacShaAssociation"/> class.
		/// </summary>
		/// <param name="typeIdentity"></param>
		/// <param name="handle">The association handle.</param>
		/// <param name="secret">The association secret.</param>
		/// <param name="totalLifeLength">The time duration the association will be good for.</param>
		private HmacShaAssociation(HmacSha typeIdentity, string handle, byte[] secret, TimeSpan totalLifeLength)
			: base(handle, secret, totalLifeLength, DateTime.UtcNow) {
			ErrorUtilities.VerifyArgumentNotNull(typeIdentity, "typeIdentity");

			Debug.Assert(secret.Length == typeIdentity.SecretLength);
			this.typeIdentity = typeIdentity;
		}

		public static HmacShaAssociation Create(Protocol protocol, string associationType, string handle, byte[] secret, TimeSpan totalLifeLength) {
			foreach (HmacSha shaType in hmacShaAssociationTypes) {
				if (String.Equals(shaType.GetAssociationType(protocol), associationType, StringComparison.Ordinal)) {
					return new HmacShaAssociation(shaType, handle, secret, totalLifeLength);
				}
			}
			throw new ArgumentOutOfRangeException("associationType");
		}

		public static HmacShaAssociation Create(int secretLength, string handle, byte[] secret, TimeSpan totalLifeLength) {
			foreach (HmacSha shaType in hmacShaAssociationTypes) {
				if (shaType.SecretLength == secretLength) {
					return new HmacShaAssociation(shaType, handle, secret, totalLifeLength);
				}
			}
			throw new ArgumentOutOfRangeException("secretLength");
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
		/// Returns the length of the shared secret (in bytes).
		/// </summary>
		/// <param name="protocol">The protocol version being used that will be used to lookup the text in <paramref name="associationType"/></param>
		/// <param name="associationType">The value of the protocol argument specifying the type of association.  For example: "HMAC-SHA1".</param>
		/// <returns>The length (in bytes) of the association secret.</returns>
		public static int GetSecretLength(Protocol protocol, string associationType) {
			foreach (HmacSha shaType in hmacShaAssociationTypes) {
				if (String.Equals(shaType.GetAssociationType(protocol), associationType, StringComparison.Ordinal)) {
					return shaType.SecretLength;
				}
			}
			throw new ArgumentOutOfRangeException("associationType");
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

		internal static bool IsDHSessionCompatible(Protocol protocol, string associationType, string sessionType) {
			// Under HTTPS, no DH encryption is required regardless of association type.
			if (string.Equals(sessionType, protocol.Args.SessionType.NoEncryption, StringComparison.Ordinal)) {
				return true;
			}

			// When there _is_ a DH session, it must match in hash length with the association type.
			foreach (HmacSha sha in hmacShaAssociationTypes) {
				if (string.Equals(associationType, sha.GetAssociationType(protocol), StringComparison.Ordinal)) {
					int hashSizeInBits = sha.SecretLength * 8;
					string matchingSessionName = DiffieHellmanUtilities.GetNameForSize(protocol, hashSizeInBits);
					if (string.Equals(sessionType, matchingSessionName, StringComparison.Ordinal)) {
						return true;
					}
				}
			}
			return false;
		}

		internal override string GetAssociationType(Protocol protocol) {
			return this.typeIdentity.GetAssociationType(protocol);
		}

		protected override HashAlgorithm CreateHasher() {
			return this.typeIdentity.CreateHasher(SecretKey);
		}

		private class HmacSha {
			/// <summary>
			/// Gets or sets the function that takes a particular OpenID version and returns the name of the association in that protocol.
			/// </summary>
			internal Func<Protocol, string> GetAssociationType { get; set; }

			internal Func<byte[], HashAlgorithm> CreateHasher { get; set; }

			internal HashAlgorithm BaseHashAlgorithm { get; set; }

			/// <summary>
			/// Gets the size of the hash (in bytes).
			/// </summary>
			internal int SecretLength { get { return this.BaseHashAlgorithm.HashSize / 8; } }
		}
	}
}