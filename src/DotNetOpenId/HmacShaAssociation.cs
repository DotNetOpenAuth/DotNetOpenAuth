using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Diagnostics;

namespace DotNetOpenId {
	internal class HmacShaAssociation : Association {

		class HmacSha {
			internal Util.Func<Protocol, string> GetAssociationType;
			internal Util.Func<byte[], HashAlgorithm> CreateHasher;
			internal HashAlgorithm BaseHashAlgorithm;
			/// <summary>
			/// The size of the hash (in bytes).
			/// </summary>
			internal int SecretLength { get { return BaseHashAlgorithm.HashSize / 8; } }
		}
		static HmacSha[] HmacShaAssociationTypes = {
			new HmacSha {
				CreateHasher = secretKey => new HMACSHA1(secretKey),
				GetAssociationType = protocol => protocol.Args.SignatureAlgorithm.HMAC_SHA1,
				BaseHashAlgorithm = new SHA1Managed(),
			},
			new HmacSha {
				CreateHasher = secretKey => new HMACSHA256(secretKey),
				GetAssociationType = protocol => protocol.Args.SignatureAlgorithm.HMAC_SHA256,
				BaseHashAlgorithm = new SHA256Managed(),
			},
			new HmacSha {
				CreateHasher = secretKey => new HMACSHA384(secretKey),
				GetAssociationType = protocol => protocol.Args.SignatureAlgorithm.HMAC_SHA384,
				BaseHashAlgorithm = new SHA384Managed(),
			},
			new HmacSha {
				CreateHasher = secretKey => new HMACSHA512(secretKey),
				GetAssociationType = protocol => protocol.Args.SignatureAlgorithm.HMAC_SHA512,
				BaseHashAlgorithm = new SHA512Managed(),
			},
		};

		public static HmacShaAssociation Create(Protocol protocol, string associationType,
			string handle, byte[] secret, TimeSpan totalLifeLength) {
			foreach (HmacSha shaType in HmacShaAssociationTypes) {
				if (String.Equals(shaType.GetAssociationType(protocol), associationType, StringComparison.Ordinal)) {
					return new HmacShaAssociation(shaType, handle, secret, totalLifeLength);
				}
			}
			throw new ArgumentOutOfRangeException("associationType");
		}

		public static HmacShaAssociation Create(int secretLength,
			string handle, byte[] secret, TimeSpan totalLifeLength) {
			foreach (HmacSha shaType in HmacShaAssociationTypes) {
				if (shaType.SecretLength == secretLength) {
					return new HmacShaAssociation(shaType, handle, secret, totalLifeLength);
				}
			}
			throw new ArgumentOutOfRangeException("secretLength");
		}

		public static int GetSecretLength(Protocol protocol, string associationType) {
			foreach (HmacSha shaType in HmacShaAssociationTypes) {
				if (String.Equals(shaType.GetAssociationType(protocol), associationType, StringComparison.Ordinal)) {
					return shaType.SecretLength;
				}
			}
			throw new ArgumentOutOfRangeException("associationType");
		}

		HmacShaAssociation(HmacSha typeIdentity, string handle, byte[] secret, TimeSpan totalLifeLength)
			: base(handle, secret, totalLifeLength, DateTime.UtcNow) {
			if (typeIdentity == null) throw new ArgumentNullException("typeIdentity");

			Debug.Assert(secret.Length == typeIdentity.SecretLength);
			this.typeIdentity = typeIdentity;
		}

		HmacSha typeIdentity;

		internal override string GetAssociationType(Protocol protocol) {
			return typeIdentity.GetAssociationType(protocol);
		}

		protected override HashAlgorithm CreateHasher() {
			return typeIdentity.CreateHasher(SecretKey);
		}
	}
}