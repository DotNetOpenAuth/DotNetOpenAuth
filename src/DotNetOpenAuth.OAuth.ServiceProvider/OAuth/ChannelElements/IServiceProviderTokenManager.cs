//-----------------------------------------------------------------------
// <copyright file="IServiceProviderTokenManager.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// A token manager for use by a web site in its role as a
	/// service provider.
	/// </summary>
	[ContractClass(typeof(IServiceProviderTokenManagerContract))]
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

	/// <summary>
	/// Code contract class for the <see cref="IServiceProviderTokenManager"/> interface.
	/// </summary>
	[ContractClassFor(typeof(IServiceProviderTokenManager))]
	internal abstract class IServiceProviderTokenManagerContract : IServiceProviderTokenManager {
		/// <summary>
		/// Prevents a default instance of the <see cref="IServiceProviderTokenManagerContract"/> class from being created.
		/// </summary>
		private IServiceProviderTokenManagerContract() {
		}

		#region IServiceProviderTokenManager Members

		/// <summary>
		/// Gets the Consumer description for a given a Consumer Key.
		/// </summary>
		/// <param name="consumerKey">The Consumer Key.</param>
		/// <returns>
		/// A description of the consumer.  Never null.
		/// </returns>
		/// <exception cref="KeyNotFoundException">Thrown if the consumer key cannot be found.</exception>
		IConsumerDescription IServiceProviderTokenManager.GetConsumer(string consumerKey) {
			Requires.NotNullOrEmpty(consumerKey, "consumerKey");
			Contract.Ensures(Contract.Result<IConsumerDescription>() != null);
			throw new NotImplementedException();
		}

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
		bool IServiceProviderTokenManager.IsRequestTokenAuthorized(string requestToken) {
			Requires.NotNullOrEmpty(requestToken, "requestToken");
			throw new NotImplementedException();
		}

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
		IServiceProviderRequestToken IServiceProviderTokenManager.GetRequestToken(string token) {
			Requires.NotNullOrEmpty(token, "token");
			Contract.Ensures(Contract.Result<IServiceProviderRequestToken>() != null);
			throw new NotImplementedException();
		}

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
		IServiceProviderAccessToken IServiceProviderTokenManager.GetAccessToken(string token) {
			Requires.NotNullOrEmpty(token, "token");
			Contract.Ensures(Contract.Result<IServiceProviderAccessToken>() != null);
			throw new NotImplementedException();
		}

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
		void IServiceProviderTokenManager.UpdateToken(IServiceProviderRequestToken token) {
			Requires.NotNull(token, "token");
			throw new NotImplementedException();
		}

		#endregion

		#region ITokenManager Members

		/// <summary>
		/// Gets the Token Secret given a request or access token.
		/// </summary>
		/// <param name="token">The request or access token.</param>
		/// <returns>
		/// The secret associated with the given token.
		/// </returns>
		/// <exception cref="ArgumentException">Thrown if the secret cannot be found for the given token.</exception>
		string ITokenManager.GetTokenSecret(string token) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Stores a newly generated unauthorized request token, secret, and optional
		/// application-specific parameters for later recall.
		/// </summary>
		/// <param name="request">The request message that resulted in the generation of a new unauthorized request token.</param>
		/// <param name="response">The response message that includes the unauthorized request token.</param>
		/// <exception cref="ArgumentException">Thrown if the consumer key is not registered, or a required parameter was not found in the parameters collection.</exception>
		/// <remarks>
		/// Request tokens stored by this method SHOULD NOT associate any user account with this token.
		/// It usually opens up security holes in your application to do so.  Instead, you associate a user
		/// account with access tokens (not request tokens) in the <see cref="ITokenManager.ExpireRequestTokenAndStoreNewAccessToken"/>
		/// method.
		/// </remarks>
		void ITokenManager.StoreNewRequestToken(DotNetOpenAuth.OAuth.Messages.UnauthorizedTokenRequest request, DotNetOpenAuth.OAuth.Messages.ITokenSecretContainingMessage response) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Deletes a request token and its associated secret and stores a new access token and secret.
		/// </summary>
		/// <param name="consumerKey">The Consumer that is exchanging its request token for an access token.</param>
		/// <param name="requestToken">The Consumer's request token that should be deleted/expired.</param>
		/// <param name="accessToken">The new access token that is being issued to the Consumer.</param>
		/// <param name="accessTokenSecret">The secret associated with the newly issued access token.</param>
		/// <remarks>
		/// 	<para>
		/// Any scope of granted privileges associated with the request token from the
		/// original call to <see cref="ITokenManager.StoreNewRequestToken"/> should be carried over
		/// to the new Access Token.
		/// </para>
		/// 	<para>
		/// To associate a user account with the new access token,
		/// <see cref="System.Web.HttpContext.User">HttpContext.Current.User</see> may be
		/// useful in an ASP.NET web application within the implementation of this method.
		/// Alternatively you may store the access token here without associating with a user account,
		/// and wait until WebConsumer.ProcessUserAuthorization or
		/// DesktopConsumer.ProcessUserAuthorization return the access
		/// token to associate the access token with a user account at that point.
		/// </para>
		/// </remarks>
		void ITokenManager.ExpireRequestTokenAndStoreNewAccessToken(string consumerKey, string requestToken, string accessToken, string accessTokenSecret) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Classifies a token as a request token or an access token.
		/// </summary>
		/// <param name="token">The token to classify.</param>
		/// <returns>
		/// Request or Access token, or invalid if the token is not recognized.
		/// </returns>
		TokenType ITokenManager.GetTokenType(string token) {
			throw new NotImplementedException();
		}

		#endregion
	}
}
