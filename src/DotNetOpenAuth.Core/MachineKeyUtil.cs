//-----------------------------------------------------------------------
// <copyright file="MachineKeyUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using System.Net;
	using System.Reflection;
	using System.Security.Cryptography;
	using System.Text;
	using System.Web;
	using System.Web.Security;

	/// <summary>
	/// Provides helpers that mimic the ASP.NET 4.5 MachineKey.Protect / Unprotect APIs,
	/// even when running on ASP.NET 4.0. Consumers are expected to follow the same
	/// conventions used by the MachineKey.Protect / Unprotect APIs (consult MSDN docs
	/// for how these are meant to be used). Additionally, since this helper class
	/// dynamically switches between the two based on whether the current application is
	/// .NET 4.0 or 4.5, consumers should never persist output from the Protect method
	/// since the implementation will change when upgrading 4.0 -> 4.5. This should be
	/// used for transient data only.
	/// </summary>
	internal static class MachineKeyUtil {
		/// <summary>
		/// MachineKey implementation depending on the target .NET framework version
		/// </summary>
		private static readonly IMachineKey MachineKeyImpl = GetMachineKeyImpl();

		/// <summary>
		/// ProtectUnprotect delegate.
		/// </summary>
		/// <param name="data">The data.</param>
		/// <param name="purposes">The purposes.</param>
		/// <returns>Result of either Protect or Unprotect methods.</returns>
		private delegate byte[] ProtectUnprotect(byte[] data, string[] purposes);

		/// <summary>
		/// Abstract the MachineKey implementation in .NET 4.0 and 4.5
		/// </summary>
		private interface IMachineKey {
			/// <summary>
			/// Protects the specified user data.
			/// </summary>
			/// <param name="userData">The user data.</param>
			/// <param name="purposes">The purposes.</param>
			/// <returns>The protected data.</returns>
			byte[] Protect(byte[] userData, string[] purposes);

			/// <summary>
			/// Unprotects the specified protected data.
			/// </summary>
			/// <param name="protectedData">The protected data.</param>
			/// <param name="purposes">The purposes.</param>
			/// <returns>The unprotected data.</returns>
			byte[] Unprotect(byte[] protectedData, string[] purposes);
		}

		/// <summary>
		/// Protects the specified user data.
		/// </summary>
		/// <param name="userData">The user data.</param>
		/// <param name="purposes">The purposes.</param>
		/// <returns>The encrypted data</returns>
		public static byte[] Protect(byte[] userData, params string[] purposes) {
			return MachineKeyImpl.Protect(userData, purposes);
		}

		/// <summary>
		/// Unprotects the specified protected data.
		/// </summary>
		/// <param name="protectedData">The protected data.</param>
		/// <param name="purposes">The purposes.</param>
		/// <returns>The unencrypted data</returns>
		public static byte[] Unprotect(byte[] protectedData, params string[] purposes) {
			return MachineKeyImpl.Unprotect(protectedData, purposes);
		}

		/// <summary>
		/// Gets the machine key implementation based on the runtime framework version.
		/// </summary>
		/// <returns>The machine key implementation</returns>
		private static IMachineKey GetMachineKeyImpl() {
			// Late bind to the MachineKey.Protect / Unprotect methods only if <httpRuntime targetFramework="4.5" />.
			// This helps ensure that round-tripping the payloads continues to work even if the application is
			// deployed to a mixed 4.0 / 4.5 farm environment.
			PropertyInfo targetFrameworkProperty = typeof(HttpRuntime).GetProperty("TargetFramework", typeof(Version));
			Version targetFramework = (targetFrameworkProperty != null) ? targetFrameworkProperty.GetValue(null, null) as Version : null;
			if (targetFramework != null && targetFramework >= new Version(4, 5)) {
				ProtectUnprotect protectThunk = (ProtectUnprotect)Delegate.CreateDelegate(typeof(ProtectUnprotect), typeof(MachineKey), "Protect", ignoreCase: false, throwOnBindFailure: false);
				ProtectUnprotect unprotectThunk = (ProtectUnprotect)Delegate.CreateDelegate(typeof(ProtectUnprotect), typeof(MachineKey), "Unprotect", ignoreCase: false, throwOnBindFailure: false);
				if (protectThunk != null && unprotectThunk != null) {
					return new MachineKey45(protectThunk, unprotectThunk); // ASP.NET 4.5
				}
			}

			return new MachineKey40(); // ASP.NET 4.0
		}

		/// <summary>
		/// On ASP.NET 4.0, we perform some transforms which mimic the behaviors of MachineKey.Protect
		/// and Unprotect.
		/// </summary>
		private sealed class MachineKey40 : IMachineKey {
			/// <summary>
			/// This is the magic header that identifies a MachineKey40 payload.
			/// It helps differentiate this from other encrypted payloads.</summary>
			private const uint MagicHeader = 0x8519140c;

			/// <summary>
			/// The SHA-256 factory to be used.
			/// </summary>
			private static readonly Func<SHA256> sha256Factory = GetSHA256Factory();

			/// <summary>
			/// Protects the specified user data.
			/// </summary>
			/// <param name="userData">The user data.</param>
			/// <param name="purposes">The purposes.</param>
			/// <returns>The protected data</returns>
			public byte[] Protect(byte[] userData, string[] purposes) {
				if (userData == null) {
					throw new ArgumentNullException("userData");
				}

				// dataWithHeader = {magic header} .. {purposes} .. {userData}
				byte[] dataWithHeader = new byte[checked(4 /* magic header */ + (256 / 8) /* purposes */ + userData.Length)];
				unchecked {
					dataWithHeader[0] = (byte)(MagicHeader >> 24);
					dataWithHeader[1] = (byte)(MagicHeader >> 16);
					dataWithHeader[2] = (byte)(MagicHeader >> 8);
					dataWithHeader[3] = (byte)MagicHeader;
				}
				byte[] purposeHash = ComputeSHA256(purposes);
				Buffer.BlockCopy(purposeHash, 0, dataWithHeader, 4, purposeHash.Length);
				Buffer.BlockCopy(userData, 0, dataWithHeader, 4 + (256 / 8), userData.Length);

				// encrypt + sign
				string hexValue = MachineKey.Encode(dataWithHeader, MachineKeyProtection.All);

				// convert hex -> binary
				byte[] binary = HexToBinary(hexValue);
				return binary;
			}

			/// <summary>
			/// Unprotects the specified protected data.
			/// </summary>
			/// <param name="protectedData">The protected data.</param>
			/// <param name="purposes">The purposes.</param>
			/// <returns>The unprotected data</returns>
			public byte[] Unprotect(byte[] protectedData, string[] purposes) {
				if (protectedData == null) {
					throw new ArgumentNullException("protectedData");
				}

				// convert binary -> hex and calculate what the purpose should read
				string hexEncodedData = BinaryToHex(protectedData);
				byte[] purposeHash = ComputeSHA256(purposes);

				try {
					// decrypt / verify signature
					byte[] dataWithHeader = MachineKey.Decode(hexEncodedData, MachineKeyProtection.All);

					// validate magic header and purpose string
					if (dataWithHeader != null
						&& dataWithHeader.Length >= (4 + (256 / 8))
						&& (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(dataWithHeader, 0)) == MagicHeader
						&& AreByteArraysEqual(new ArraySegment<byte>(purposeHash), new ArraySegment<byte>(dataWithHeader, 4, 256 / 8))) {
						// validation succeeded
						byte[] userData = new byte[dataWithHeader.Length - 4 - (256 / 8)];
						Buffer.BlockCopy(dataWithHeader, 4 + (256 / 8), userData, 0, userData.Length);
						return userData;
					}
				}
				catch {
					// swallow since will be rethrown immediately below
				}

				// if we reached this point, some cryptographic operation failed
				throw new CryptographicException(Strings.Generic_CryptoFailure);
			}

			/// <summary>
			/// Convert bytes to hex string.
			/// </summary>
			/// <param name="binary">The input array.</param>
			/// <returns>Hex string</returns>
			internal static string BinaryToHex(byte[] binary) {
				StringBuilder builder = new StringBuilder(checked(binary.Length * 2));
				for (int i = 0; i < binary.Length; i++) {
					byte b = binary[i];
					builder.Append(HexDigit(b >> 4));
					builder.Append(HexDigit(b & 0x0F));
				}
				string result = builder.ToString();
				return result;
			}

			/// <summary>
			/// This method is specially written to take the same amount of time
			/// regardless of where 'a' and 'b' differ. Please do not optimize it.</summary>
			/// <param name="a">first array.</param>
			/// <param name="b">second array.</param>
			/// <returns><c href="true" /> if equal, others <c href="false" /></returns>
			private static bool AreByteArraysEqual(ArraySegment<byte> a, ArraySegment<byte> b) {
				if (a.Count != b.Count) {
					return false;
				}

				bool areEqual = true;
				for (int i = 0; i < a.Count; i++) {
					areEqual &= a.Array[a.Offset + i] == b.Array[b.Offset + i];
				}
				return areEqual;
			}

			/// <summary>
			/// Computes a SHA256 hash over all of the input parameters.
			/// Each parameter is UTF8 encoded and preceded by a 7-bit encoded</summary>
			/// integer describing the encoded byte length of the string.
			/// <param name="parameters">The parameters.</param>
			/// <returns>The output hash</returns>
			[SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "MemoryStream is resilient to double-Dispose")]
			private static byte[] ComputeSHA256(IList<string> parameters) {
				using (MemoryStream ms = new MemoryStream()) {
					using (BinaryWriter bw = new BinaryWriter(ms)) {
						if (parameters != null) {
							foreach (string parameter in parameters) {
								bw.Write(parameter); // also writes the length as a prefix; unambiguous
							}
							bw.Flush();
						}

						using (SHA256 sha256 = sha256Factory()) {
							byte[] retVal = sha256.ComputeHash(ms.GetBuffer(), 0, checked((int)ms.Length));
							return retVal;
						}
					}
				}
			}

			/// <summary>
			/// Gets the SHA-256 factory.
			/// </summary>
			/// <returns>SHA256 factory</returns>
			private static Func<SHA256> GetSHA256Factory() {
				// Note: ASP.NET 4.5 always prefers CNG, but the CNG algorithms are not that
				// performant on 4.0 and below. The following list is optimized for speed
				// given our scenarios.
				if (!CryptoConfig.AllowOnlyFipsAlgorithms) {
					// This provider is not FIPS-compliant, so we can't use it if FIPS compliance
					// is mandatory.
					return () => new SHA256Managed();
				}

				try {
					using (SHA256Cng sha256 = new SHA256Cng()) {
						return () => new SHA256Cng();
					}
				}
				catch (PlatformNotSupportedException) {
					// CNG not supported (perhaps because we're not on Windows Vista or above); move on
				}

				// If all else fails, fall back to CAPI.
				return () => new SHA256CryptoServiceProvider();
			}

			/// <summary>
			/// Convert to hex character
			/// </summary>
			/// <param name="value">The value to be converted.</param>
			/// <returns>Hex character</returns>
			private static char HexDigit(int value) {
				return (char)(value > 9 ? value + '7' : value + '0');
			}

			/// <summary>
			/// Convert hdex string to bytes.
			/// </summary>
			/// <param name="hex">Input hex string.</param>
			/// <returns>The bytes</returns>
			private static byte[] HexToBinary(string hex) {
				int size = hex.Length / 2;
				byte[] bytes = new byte[size];
				for (int idx = 0; idx < size; idx++) {
					bytes[idx] = (byte)((HexValue(hex[idx * 2]) << 4) + HexValue(hex[(idx * 2) + 1]));
				}
				return bytes;
			}

			/// <summary>
			/// Convert hex digit to byte.
			/// </summary>
			/// <param name="digit">The hex digit.</param>
			/// <returns>The byte</returns>
			private static int HexValue(char digit) {
				return digit > '9' ? digit - '7' : digit - '0';
			}
		}

		/// <summary>
		/// On ASP.NET 4.5, we can just delegate to MachineKey.Protect and MachineKey.Unprotect directly,
		/// which contain optimized code paths.
		/// </summary>
		private sealed class MachineKey45 : IMachineKey {
			/// <summary>
			/// Protect thunk
			/// </summary>
			private readonly ProtectUnprotect protectThunk;

			/// <summary>
			/// Unprotect thunk
			/// </summary>
			private readonly ProtectUnprotect unprotectThunk;

			/// <summary>
			/// Initializes a new instance of the <see cref="MachineKey45"/> class.
			/// </summary>
			/// <param name="protectThunk">The protect thunk.</param>
			/// <param name="unprotectThunk">The unprotect thunk.</param>
			public MachineKey45(ProtectUnprotect protectThunk, ProtectUnprotect unprotectThunk) {
				this.protectThunk = protectThunk;
				this.unprotectThunk = unprotectThunk;
			}

			/// <summary>
			/// Protects the specified user data.
			/// </summary>
			/// <param name="userData">The user data.</param>
			/// <param name="purposes">The purposes.</param>
			/// <returns>The protected data</returns>
			public byte[] Protect(byte[] userData, string[] purposes) {
				return this.protectThunk(userData, purposes);
			}

			/// <summary>
			/// Unprotects the specified protected data.
			/// </summary>
			/// <param name="protectedData">The protected data.</param>
			/// <param name="purposes">The purposes.</param>
			/// <returns>The unprotected data</returns>
			public byte[] Unprotect(byte[] protectedData, string[] purposes) {
				return this.unprotectThunk(protectedData, purposes);
			}
		}
	}
}
