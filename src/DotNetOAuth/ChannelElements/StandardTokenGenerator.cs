//-----------------------------------------------------------------------
// <copyright file="StandardTokenGenerator.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.ChannelElements {
	using System;
	using System.Security.Cryptography;

	/// <summary>
	/// A cryptographically strong random string generator for tokens and secrets.
	/// </summary>
	internal class StandardTokenGenerator : ITokenGenerator {
		/// <summary>
		/// The cryptographically strong random string generator for tokens and secrets.
		/// </summary>
		private RandomNumberGenerator cryptoProvider = new RNGCryptoServiceProvider();

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
			return this.GenerateCryptographicallyStrongString();
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
			return this.GenerateCryptographicallyStrongString();
		}

		/// <summary>
		/// Returns a cryptographically strong random string for use as a token secret.
		/// </summary>
		/// <returns>The generated string.</returns>
		public string GenerateSecret() {
			return this.GenerateCryptographicallyStrongString();
		}

		#endregion

		/// <summary>
		/// Returns a new random string.
		/// </summary>
		/// <returns>The new random string.</returns>
		private string GenerateCryptographicallyStrongString() {
			byte[] buffer = new byte[20];
			this.cryptoProvider.GetBytes(buffer);
			return Convert.ToBase64String(buffer);
		}
	}
}
