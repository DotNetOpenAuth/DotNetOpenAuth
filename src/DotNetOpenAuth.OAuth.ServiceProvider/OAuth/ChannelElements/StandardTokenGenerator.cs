//-----------------------------------------------------------------------
// <copyright file="StandardTokenGenerator.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Security.Cryptography;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A cryptographically strong random string generator for tokens and secrets.
	/// </summary>
	internal class StandardTokenGenerator : ITokenGenerator {
		#region ITokenGenerator Members

		/// <summary>
		/// Generates a new token to represent a not-yet-authorized request to access protected resources.
		/// </summary>
		/// <param name="consumerKey">The consumer that requested this token.</param>
		/// <returns>The newly generated token.</returns>
		/// <remarks>
		/// This method should not store the newly generated token in any persistent store.
		/// This will be done in <see cref="ITokenManager.StoreNewRequestToken"/>.
		/// </remarks>
		public string GenerateRequestToken(string consumerKey) {
			return GenerateCryptographicallyStrongString();
		}

		/// <summary>
		/// Generates a new token to represent an authorized request to access protected resources.
		/// </summary>
		/// <param name="consumerKey">The consumer that requested this token.</param>
		/// <returns>The newly generated token.</returns>
		/// <remarks>
		/// This method should not store the newly generated token in any persistent store.
		/// This will be done in <see cref="ITokenManager.ExpireRequestTokenAndStoreNewAccessToken"/>.
		/// </remarks>
		public string GenerateAccessToken(string consumerKey) {
			return GenerateCryptographicallyStrongString();
		}

		/// <summary>
		/// Returns a cryptographically strong random string for use as a token secret.
		/// </summary>
		/// <returns>The generated string.</returns>
		public string GenerateSecret() {
			return GenerateCryptographicallyStrongString();
		}

		#endregion

		/// <summary>
		/// Returns a new random string.
		/// </summary>
		/// <returns>The new random string.</returns>
		private static string GenerateCryptographicallyStrongString() {
			byte[] buffer = new byte[20];
			MessagingUtilities.CryptoRandomDataGenerator.GetBytes(buffer);
			return Convert.ToBase64String(buffer);
		}
	}
}
