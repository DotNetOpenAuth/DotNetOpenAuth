//-----------------------------------------------------------------------
// <copyright file="OAuthServiceProviderMessageTypeProvider.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth.Messages;

	/// <summary>
	/// An OAuth-protocol specific implementation of the <see cref="IMessageTypeProvider"/>
	/// interface.
	/// </summary>
	public class OAuthServiceProviderMessageTypeProvider : IMessageTypeProvider {
		/// <summary>
		/// The token manager to use for discerning between request and access tokens.
		/// </summary>
		private ITokenManager tokenManager;

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthServiceProviderMessageTypeProvider"/> class.
		/// </summary>
		/// <param name="tokenManager">The token manager instance to use.</param>
		protected internal OAuthServiceProviderMessageTypeProvider(ITokenManager tokenManager) {
			if (tokenManager == null) {
				throw new ArgumentNullException("tokenManager");
			}

			this.tokenManager = tokenManager;
		}

		#region IMessageTypeProvider Members

		/// <summary>
		/// Analyzes an incoming request message payload to discover what kind of 
		/// message is embedded in it and returns the type, or null if no match is found.
		/// </summary>
		/// <param name="fields">The name/value pairs that make up the message payload.</param>
		/// <remarks>
		/// The request messages are:
		/// UnauthorizedTokenRequest
		/// AuthorizedTokenRequest
		/// UserAuthorizationRequest
		/// AccessProtectedResourceRequest
		/// </remarks>
		/// <returns>
		/// The <see cref="IProtocolMessage"/>-derived concrete class that this message can
		/// deserialize to.  Null if the request isn't recognized as a valid protocol message.
		/// </returns>
		public virtual Type GetRequestMessageType(IDictionary<string, string> fields) {
			if (fields == null) {
				throw new ArgumentNullException("fields");
			}

			if (fields.ContainsKey("oauth_consumer_key") &&
				!fields.ContainsKey("oauth_token")) {
				return typeof(UnauthorizedTokenRequest);
			}

			if (fields.ContainsKey("oauth_consumer_key") &&
				fields.ContainsKey("oauth_token")) {
				// Discern between RequestAccessToken and AccessProtectedResources,
				// which have all the same parameters, by figuring out what type of token
				// is in the token parameter.
				bool tokenTypeIsAccessToken = this.tokenManager.GetTokenType(fields["oauth_token"]) == TokenType.AccessToken;

				return tokenTypeIsAccessToken ? typeof(AccessProtectedResourceRequest) :
					typeof(AuthorizedTokenRequest);
			}

			// fail over to the message with no required fields at all.
			return typeof(UserAuthorizationRequest);
		}

		/// <summary>
		/// Analyzes an incoming request message payload to discover what kind of 
		/// message is embedded in it and returns the type, or null if no match is found.
		/// </summary>
		/// <param name="request">
		/// The message that was sent as a request that resulted in the response.
		/// Null on a Consumer site that is receiving an indirect message from the Service Provider.
		/// </param>
		/// <param name="fields">The name/value pairs that make up the message payload.</param>
		/// <returns>
		/// The <see cref="IProtocolMessage"/>-derived concrete class that this message can
		/// deserialize to.  Null if the request isn't recognized as a valid protocol message.
		/// </returns>
		/// <remarks>
		/// The response messages are:
		/// None.
		/// </remarks>
		public virtual Type GetResponseMessageType(IProtocolMessage request, IDictionary<string, string> fields) {
			if (fields == null) {
				throw new ArgumentNullException("fields");
			}

			Logger.Error("Service Providers are not expected to ever receive responses.");
			return null;
		}

		#endregion
	}
}
