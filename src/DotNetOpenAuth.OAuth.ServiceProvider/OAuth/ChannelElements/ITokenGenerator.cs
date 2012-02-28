//-----------------------------------------------------------------------
// <copyright file="ITokenGenerator.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	/// <summary>
	/// An interface allowing OAuth hosts to inject their own algorithm for generating tokens and secrets.
	/// </summary>
	public interface ITokenGenerator {
		/// <summary>
		/// Generates a new token to represent a not-yet-authorized request to access protected resources.
		/// </summary>
		/// <param name="consumerKey">The consumer that requested this token.</param>
		/// <returns>The newly generated token.</returns>
		/// <remarks>
		/// This method should not store the newly generated token in any persistent store.
		/// This will be done in <see cref="ITokenManager.StoreNewRequestToken"/>.
		/// </remarks>
		string GenerateRequestToken(string consumerKey);

		/// <summary>
		/// Generates a new token to represent an authorized request to access protected resources.
		/// </summary>
		/// <param name="consumerKey">The consumer that requested this token.</param>
		/// <returns>The newly generated token.</returns>
		/// <remarks>
		/// This method should not store the newly generated token in any persistent store.
		/// This will be done in <see cref="ITokenManager.ExpireRequestTokenAndStoreNewAccessToken"/>.
		/// </remarks>
		string GenerateAccessToken(string consumerKey);

		/// <summary>
		/// Returns a cryptographically strong random string for use as a token secret.
		/// </summary>
		/// <returns>The generated string.</returns>
		string GenerateSecret();
	}
}
