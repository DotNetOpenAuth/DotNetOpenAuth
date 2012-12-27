//-----------------------------------------------------------------------
// <copyright file="HmacShaAssociation.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.Linq;
	using System.Security.Cryptography;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using Validation;

	/// <summary>
	/// An association that uses the HMAC-SHA family of algorithms for message signing.
	/// </summary>
	internal class HmacShaAssociation : Association {
		/// <summary>
		/// A list of HMAC-SHA algorithms in order of decreasing bit lengths.
		/// </summary>
		private static HmacSha[] hmacShaAssociationTypes = CreateAssociationTypes();

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
			Requires.NotNull(typeIdentity, "typeIdentity");
			Requires.NotNullOrEmpty(handle, "handle");
			Requires.NotNull(secret, "secret");
			Requires.Range(totalLifeLength > TimeSpan.Zero, "totalLifeLength");
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
			Requires.NotNull(protocol, "protocol");
			Requires.NotNullOrEmpty(associationType, "associationType");
			Requires.NotNull(secret, "secret");
			HmacSha match = hmacShaAssociationTypes.FirstOrDefault(sha => string.Equals(sha.GetAssociationType(protocol), associationType, StringComparison.Ordinal));
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
			Requires.NotNullOrEmpty(handle, "handle");
			Requires.NotNull(secret, "secret");

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
			HmacSha match = hmacShaAssociationTypes.FirstOrDefault(shaType => string.Equals(shaType.GetAssociationType(protocol), associationType, StringComparison.Ordinal));
			ErrorUtilities.VerifyProtocol(match != null, OpenIdStrings.NoAssociationTypeFoundByName, associationType);
			return match.SecretLength;
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
			Requires.NotNull(protocol, "protocol");
			Requires.NotNull(securityRequirements, "securityRequirements");

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

				if (OpenIdUtilities.IsDiffieHellmanPresent) {
					sessionType = DiffieHellmanUtilities.GetNameForSize(protocol, hashSizeInBits);
				} else {
					sessionType = requireMatchingDHSessionType ? null : protocol.Args.SessionType.NoEncryption;
				}

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
			Requires.NotNull(protocol, "protocol");
			Requires.NotNullOrEmpty(associationType, "associationType");
			Requires.NotNull(sessionType, "sessionType");

			// All association types can work when no DH session is used at all.
			if (string.Equals(sessionType, protocol.Args.SessionType.NoEncryption, StringComparison.Ordinal)) {
				return true;
			}

			if (OpenIdUtilities.IsDiffieHellmanPresent) {
				// When there _is_ a DH session, it must match in hash length with the association type.
				int associationSecretLengthInBytes = GetSecretLength(protocol, associationType);
				int sessionHashLengthInBytes = DiffieHellmanUtilities.Lookup(protocol, sessionType).HashSize / 8;
				return associationSecretLengthInBytes == sessionHashLengthInBytes;
			} else {
				return false;
			}
		}

		/// <summary>
		/// Gets the string to pass as the assoc_type value in the OpenID protocol.
		/// </summary>
		/// <param name="protocol">The protocol version of the message that the assoc_type value will be included in.</param>
		/// <returns>
		/// The value that should be used for  the openid.assoc_type parameter.
		/// </returns>
		[Pure]
		internal override string GetAssociationType(Protocol protocol) {
			return this.typeIdentity.GetAssociationType(protocol);
		}

		/// <summary>
		/// Returns the specific hash algorithm used for message signing.
		/// </summary>
		/// <returns>
		/// The hash algorithm used for message signing.
		/// </returns>
		[Pure]
		protected override HashAlgorithm CreateHasher() {
			var result = this.typeIdentity.CreateHasher(SecretKey);
			Assumes.True(result != null);
			return result;
		}

		/// <summary>
		/// Returns the value used to initialize the static field storing association types.
		/// </summary>
		/// <returns>A non-null, non-empty array.</returns>
		/// <remarks>>
		/// This is a method rather than being inlined to the field initializer to try to avoid
		/// the CLR bug that crops up sometimes if we initialize arrays using object initializer syntax.
		/// </remarks>
		private static HmacSha[] CreateAssociationTypes() {
			return new[] {
				new HmacSha {
					HmacAlgorithmName = HmacAlgorithms.HmacSha384,
					GetAssociationType = protocol => protocol.Args.SignatureAlgorithm.HMAC_SHA512,
					BaseHashAlgorithm = SHA512.Create(),
				},
				new HmacSha {
					HmacAlgorithmName = HmacAlgorithms.HmacSha384,
					GetAssociationType = protocol => protocol.Args.SignatureAlgorithm.HMAC_SHA384,
					BaseHashAlgorithm = SHA384.Create(),
				},
				new HmacSha {
					HmacAlgorithmName = HmacAlgorithms.HmacSha256,
					GetAssociationType = protocol => protocol.Args.SignatureAlgorithm.HMAC_SHA256,
					BaseHashAlgorithm = SHA256.Create(),
				},
				new HmacSha {
					HmacAlgorithmName = HmacAlgorithms.HmacSha1,
					GetAssociationType = protocol => protocol.Args.SignatureAlgorithm.HMAC_SHA1,
					BaseHashAlgorithm = SHA1.Create(),
				},
			};
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
			/// Gets or sets the name of the HMAC-SHA algorithm. (e.g. "HMAC-SHA256")
			/// </summary>
			internal string HmacAlgorithmName { get; set; }

			/// <summary>
			/// Gets or sets the base hash algorithm.
			/// </summary>
			internal HashAlgorithm BaseHashAlgorithm { get; set; }

			/// <summary>
			/// Gets the size of the hash (in bytes).
			/// </summary>
			internal int SecretLength { get { return this.BaseHashAlgorithm.HashSize / 8; } }

			/// <summary>
			/// Creates the <see cref="HashAlgorithm"/> using a given shared secret for the mac.
			/// </summary>
			/// <param name="secret">The HMAC secret.</param>
			/// <returns>The algorithm.</returns>
			internal HashAlgorithm CreateHasher(byte[] secret) {
				return HmacAlgorithms.Create(this.HmacAlgorithmName, secret);
			}
		}
	}
}