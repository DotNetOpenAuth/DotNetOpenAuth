//-----------------------------------------------------------------------
// <copyright file="IServiceProviderTokenManager.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// A token manager for use by a web site in its role as a
	/// service provider.
	/// </summary>
	public interface IServiceProviderTokenManager : ITokenManager {
		/// <summary>
		/// Gets the Consumer Secret for a given a Consumer Key.
		/// </summary>
		/// <param name="consumerKey">The Consumer Key.</param>
		/// <returns>The Consumer Secret.</returns>
		/// <exception cref="ArgumentException">Thrown if the consumer key cannot be found.</exception>
		/// <exception cref="InvalidOperationException">May be thrown if called when the signature algorithm does not require a consumer secret, such as when RSA-SHA1 is used.</exception>
		string GetConsumerSecret(string consumerKey);

		/// <summary>
		/// Sets the verifier code associated with an authorized request token.
		/// </summary>
		/// <param name="requestToken">The request token.</param>
		/// <param name="verifier">The verification code.</param>
		void SetRequestTokenVerifier(string requestToken, string verifier);

		/// <summary>
		/// Gets the verifier code associated with an authorized request token.
		/// </summary>
		/// <param name="requestToken">The request token that the Consumer is exchanging for an access token.</param>
		/// <returns>The verifier code that was generated when previously authorizing the request token.</returns>
		string GetRequestTokenVerifier(string requestToken);

		/// <summary>
		/// Sets the request token consumer callback.
		/// </summary>
		/// <param name="requestToken">The request token.</param>
		/// <param name="callback">The callback.</param>
		void SetRequestTokenCallback(string requestToken, Uri callback);

		/// <summary>
		/// Gets the request token consumer callback.
		/// </summary>
		/// <param name="requestToken">The request token.</param>
		/// <returns>The callback Uri.  May be <c>null</c>.</returns>
		Uri GetRequestTokenCallback(string requestToken);

		/// <summary>
		/// Sets the OAuth version used by the Consumer to request a token.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <param name="version">The OAuth version.</param>
		void SetTokenConsumerVersion(string token, Version version);

		/// <summary>
		/// Gets the OAuth version used by the Consumer to request a token.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <returns>The OAuth version</returns>
		Version GetTokenConsumerVersion(string token);
	}
}
