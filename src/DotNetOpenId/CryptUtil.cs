using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using Org.Mentalis.Security.Cryptography;
using System.Globalization;


namespace DotNetOpenId {
	internal static class CryptUtil {
		public static byte[] DEFAULT_GEN = { 2 };
		public static byte[] DEFAULT_MOD = {0, 220, 249, 58, 11, 136, 57, 114, 236, 14, 25, 152, 154, 197, 162,
			206, 49, 14, 29, 55, 113, 126, 141, 149, 113, 187, 118, 35, 115, 24,
			102, 230, 30, 247, 90, 46, 39, 137, 139, 5, 127, 152, 145, 194, 226,
			122, 99, 156, 63, 41, 182, 8, 20, 88, 28, 211, 178, 202, 57, 134, 210,
			104, 55, 5, 87, 125, 69, 194, 231, 229, 45, 200, 28, 122, 23, 24, 118,
			229, 206, 167, 75, 20, 72, 191, 223, 175, 24, 130, 142, 253, 37, 25,
			241, 78, 69, 227, 130, 102, 52, 175, 25, 73, 229, 181, 53, 204, 130,
			154, 72, 59, 138, 118, 34, 62, 93, 73, 10, 37, 127, 5, 189, 255, 22,
			242, 251, 34, 197, 131, 171};

		internal static SHA1CryptoServiceProvider Sha1 = new SHA1CryptoServiceProvider();
		internal static SHA256Managed Sha256 = new SHA256Managed();

		public static string UnsignedToBase64(byte[] inputBytes) {
			return Convert.ToBase64String(ensurePositive(inputBytes));
		}

		public static DiffieHellman CreateDiffieHellman() {
			return new DiffieHellmanManaged(DEFAULT_MOD, DEFAULT_GEN, 1024);
		}

		public static byte[] SHAHashXorSecret(HashAlgorithm hasher, DiffieHellman dh, byte[] keyEx, byte[] encMacKey) {
			byte[] dhShared = dh.DecryptKeyExchange(keyEx);
			byte[] shaDhShared = hasher.ComputeHash(ensurePositive(dhShared));
			if (shaDhShared.Length != encMacKey.Length) {
				throw new ArgumentOutOfRangeException(string.Format(CultureInfo.CurrentUICulture,
					"encMacKey's length ({0}) does not match the length of the hashing algorithm ({1}).",
					encMacKey.Length, shaDhShared.Length));
			}

			byte[] secret = new byte[encMacKey.Length];
			for (int i = 0; i < encMacKey.Length; i++) {
				secret[i] = (byte)(encMacKey[i] ^ shaDhShared[i]);
			}
			return secret;
		}

		/// <summary>
		/// Ensures that the big integer represented by a given series of bytes
		/// is a positive integer.
		/// </summary>
		/// <returns>A byte array (possibly new if a change was required) whose
		/// integer is guaranteed to be positive.</returns>
		/// <remarks>
		/// This is to be consistent with OpenID spec section 4.2.
		/// </remarks>
		static byte[] ensurePositive(byte[] inputBytes) {
			if (inputBytes.Length == 0) throw new ArgumentException("Invalid input passed to EnsurePositive. Array must have something in it.", "inputBytes");

			int i = (int)inputBytes[0];
			if (i > 127) {
				byte[] temp = new byte[inputBytes.Length + 1];
				temp[0] = 0;
				inputBytes.CopyTo(temp, 1);
				inputBytes = temp;
			}
			return inputBytes;
		}
	}
}
