//-----------------------------------------------------------------------
// <copyright file="HmacAlgorithms.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using Validation;

	/// <summary>
	/// HMAC-SHA algorithm names that can be passed to the <see cref="HMAC.Create(string)"/> method.
	/// </summary>
	internal static class HmacAlgorithms {
		/// <summary>
		/// The name of the HMAC-SHA1 algorithm.
		/// </summary>
		internal const string HmacSha1 = "HMACSHA1";

		/// <summary>
		/// The name of the HMAC-SHA256 algorithm.
		/// </summary>
		internal const string HmacSha256 = "HMACSHA256";

		/// <summary>
		/// The name of the HMAC-SHA384 algorithm.
		/// </summary>
		internal const string HmacSha384 = "HMACSHA384";

		/// <summary>
		/// The name of the HMAC-SHA512 algorithm.
		/// </summary>
		internal const string HmacSha512 = "HMACSHA512";

		/// <summary>
		/// Creates an HMAC-SHA algorithm with the specified name and key.
		/// </summary>
		/// <param name="algorithmName">A name from the available choices in the static const members of this class.</param>
		/// <param name="key">The secret key used as the HMAC.</param>
		/// <returns>The HMAC algorithm instance.</returns>
		internal static HMAC Create(string algorithmName, byte[] key) {
			Requires.NotNullOrEmpty(algorithmName, "algorithmName");
			Requires.NotNull(key, "key");

			HMAC hmac = HMAC.Create(algorithmName);
			try {
				hmac.Key = key;
				return hmac;
			} catch {
				hmac.Dispose();
				throw;
			}
		}
	}
}
