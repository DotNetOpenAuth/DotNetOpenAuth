//-----------------------------------------------------------------------
// <copyright file="DiffieHellmanUtilities.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using Org.Mentalis.Security.Cryptography;

	/// <summary>
	/// Diffie-Hellman encryption methods used by both the relying party and provider.
	/// </summary>
	internal class DiffieHellmanUtilities {
		private static DHSha[] diffieHellmanSessionTypes = {
			new DHSha(new SHA512Managed(), protocol => protocol.Args.SessionType.DH_SHA512),
			new DHSha(new SHA384Managed(), protocol => protocol.Args.SessionType.DH_SHA384),
			new DHSha(new SHA256Managed(), protocol => protocol.Args.SessionType.DH_SHA256),
			new DHSha(new SHA1Managed(), protocol => protocol.Args.SessionType.DH_SHA1),
		};

		public static HashAlgorithm Lookup(Protocol protocol, string sessionType) {
			ErrorUtilities.VerifyArgumentNotNull(protocol, "protocol");
			ErrorUtilities.VerifyArgumentNotNull(sessionType, "sessionType");

			foreach (DHSha dhsha in diffieHellmanSessionTypes) {
				if (String.Equals(dhsha.GetName(protocol), sessionType, StringComparison.Ordinal)) {
					return dhsha.Algorithm;
				}
			}
			throw new ArgumentOutOfRangeException("name");
		}

		internal static string GetNameForSize(Protocol protocol, int hashSizeInBits) {
			ErrorUtilities.VerifyArgumentNotNull(protocol, "protocol");
			foreach (DHSha dhsha in diffieHellmanSessionTypes) {
				if (dhsha.Algorithm.HashSize == hashSizeInBits) {
					return dhsha.GetName(protocol);
				}
			}
			return null;
		}

		/// <summary>
		/// Encrypts/decrypts a shared secret.
		/// </summary>
		/// <param name="hasher">The hashing algorithm that is agreed by both parties to use as part of the secret exchange.</param>
		/// <param name="dh">
		/// If the secret is being encrypted, this is the new Diffie Hellman object to use.
		/// If the secret is being decrypted, this must be the same Diffie Hellman object used to send the original request message.
		/// </param>
		/// <param name="remotePublicKey">The public key of the remote party.</param>
		/// <param name="plainOrEncryptedSecret">The secret to encode, or the encoded secret.  Whichever one is given will generate the opposite in the return value.</param>
		/// <returns>
		/// The encrypted version of the secret if the secret itself was given in <paramref name="remotePublicKey"/>.
		/// The secret itself if the encrypted version of the secret was given in <paramref name="remotePublicKey"/>.
		/// </returns>
		internal static byte[] SHAHashXorSecret(HashAlgorithm hasher, DiffieHellman dh, byte[] remotePublicKey, byte[] plainOrEncryptedSecret) {
			ErrorUtilities.VerifyArgumentNotNull(hasher, "hasher");
			ErrorUtilities.VerifyArgumentNotNull(dh, "dh");
			ErrorUtilities.VerifyArgumentNotNull(remotePublicKey, "remotePublicKey");
			ErrorUtilities.VerifyArgumentNotNull(plainOrEncryptedSecret, "plainOrEncryptedSecret");

			byte[] dhShared = dh.DecryptKeyExchange(remotePublicKey);
			byte[] shaDhShared = hasher.ComputeHash(EnsurePositive(dhShared));
			if (shaDhShared.Length != plainOrEncryptedSecret.Length) {
				throw new ArgumentOutOfRangeException(string.Format(CultureInfo.CurrentCulture,
					"encMacKey's length ({0}) does not match the length of the hashing algorithm ({1}).",
					plainOrEncryptedSecret.Length, shaDhShared.Length));
			}

			byte[] secret = new byte[plainOrEncryptedSecret.Length];
			for (int i = 0; i < plainOrEncryptedSecret.Length; i++) {
				secret[i] = (byte)(plainOrEncryptedSecret[i] ^ shaDhShared[i]);
			}
			return secret;
		}

		/// <summary>
		/// Ensures that the big integer represented by a given series of bytes
		/// is a positive integer.
		/// </summary>
		/// <param name="inputBytes">The bytes that make up the big integer.</param>
		/// <returns>
		/// A byte array (possibly new if a change was required) whose
		/// integer is guaranteed to be positive.
		/// </returns>
		/// <remarks>
		/// This is to be consistent with OpenID spec section 4.2.
		/// </remarks>
		internal static byte[] EnsurePositive(byte[] inputBytes) {
			ErrorUtilities.VerifyArgumentNotNull(inputBytes, "inputBytes");
			if (inputBytes.Length == 0) {
				throw new ArgumentException(MessagingStrings.UnexpectedEmptyArray, "inputBytes");
			}

			int i = (int)inputBytes[0];
			if (i > 127) {
				byte[] nowPositive = new byte[inputBytes.Length + 1];
				nowPositive[0] = 0;
				inputBytes.CopyTo(nowPositive, 1);
				return nowPositive;
			}

			return inputBytes;
		}

		private class DHSha {
			public DHSha(HashAlgorithm algorithm, Func<Protocol, string> getName) {
				ErrorUtilities.VerifyArgumentNotNull(algorithm, "algorithm");
				ErrorUtilities.VerifyArgumentNotNull(getName, "getName");

				this.GetName = getName;
				this.Algorithm = algorithm;
			}

			internal Func<Protocol, string> GetName { get; set; }

			internal HashAlgorithm Algorithm { get; private set; }
		}
	}
}
