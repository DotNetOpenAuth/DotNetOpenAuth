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
	}
}
