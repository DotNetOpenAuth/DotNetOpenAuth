//-----------------------------------------------------------------------
// <copyright file="IServiceProviderTokenManager.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Validation;

	/// <summary>
	/// A token manager for use by a web site in its role as a
	/// service provider.
	/// </summary>
	public interface IServiceProviderTokenManager : ITokenManager {
		/// <summary>
		/// Gets the Consumer description for a given a Consumer Key.
		/// </summary>
		/// <param name="consumerKey">The Consumer Key.</param>
		/// <returns>A description of the consumer.  Never null.</returns>
		/// <exception cref="KeyNotFoundException">Thrown if the consumer key cannot be found.</exception>
		IConsumerDescription GetConsumer(string consumerKey);

		/// <summary>
		/// Checks whether a given request token has already been authorized
		/// by some user for use by the Consumer that requested it.
		/// </summary>
		/// <param name="requestToken">The Consumer's request token.</param>
		/// <returns>
		/// True if the request token has already been fully authorized by the user
		/// who owns the relevant protected resources.  False if the token has not yet
		/// been authorized, has expired or does not exist.
		/// </returns>
		bool IsRequestTokenAuthorized(string requestToken);

		/// <summary>
		/// Gets details on the named request token.
		/// </summary>
		/// <param name="token">The request token.</param>
		/// <returns>A description of the token.  Never null.</returns>
		/// <exception cref="KeyNotFoundException">Thrown if the token cannot be found.</exception>
		/// <remarks>
		/// It is acceptable for implementations to find the token, see that it has expired,
		/// delete it from the database and then throw <see cref="KeyNotFoundException"/>,
		/// or alternatively it can return the expired token anyway and the OAuth channel will
		/// log and throw the appropriate error.
		/// </remarks>
		IServiceProviderRequestToken GetRequestToken(string token);

		/// <summary>
		/// Gets details on the named access token.
		/// </summary>
		/// <param name="token">The access token.</param>
		/// <returns>A description of the token.  Never null.</returns>
		/// <exception cref="KeyNotFoundException">Thrown if the token cannot be found.</exception>
		/// <remarks>
		/// It is acceptable for implementations to find the token, see that it has expired,
		/// delete it from the database and then throw <see cref="KeyNotFoundException"/>,
		/// or alternatively it can return the expired token anyway and the OAuth channel will
		/// log and throw the appropriate error.
		/// </remarks>
		IServiceProviderAccessToken GetAccessToken(string token);

		/// <summary>
		/// Persists any changes made to the token.
		/// </summary>
		/// <param name="token">The token whose properties have been changed.</param>
		/// <remarks>
		/// This library will invoke this method after making a set
		/// of changes to the token as part of a web request to give the host
		/// the opportunity to persist those changes to a database.
		/// Depending on the object persistence framework the host site uses,
		/// this method MAY not need to do anything (if changes made to the token
		/// will automatically be saved without any extra handling).
		/// </remarks>
		void UpdateToken(IServiceProviderRequestToken token);
	}
}
