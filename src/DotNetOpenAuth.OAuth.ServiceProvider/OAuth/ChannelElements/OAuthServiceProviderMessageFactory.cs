//-----------------------------------------------------------------------
// <copyright file="OAuthServiceProviderMessageFactory.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth.Messages;
	using Validation;

	/// <summary>
	/// An OAuth-protocol specific implementation of the <see cref="IMessageFactory"/>
	/// interface.
	/// </summary>
	public class OAuthServiceProviderMessageFactory : IMessageFactory {
		/// <summary>
		/// The token manager to use for discerning between request and access tokens.
		/// </summary>
		private IServiceProviderTokenManager tokenManager;

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthServiceProviderMessageFactory"/> class.
		/// </summary>
		/// <param name="tokenManager">The token manager instance to use.</param>
		public OAuthServiceProviderMessageFactory(IServiceProviderTokenManager tokenManager) {
			Requires.NotNull(tokenManager, "tokenManager");

			this.tokenManager = tokenManager;
		}

		#region IMessageFactory Members

		/// <summary>
		/// Analyzes an incoming request message payload to discover what kind of
		/// message is embedded in it and returns the type, or null if no match is found.
		/// </summary>
		/// <param name="recipient">The intended or actual recipient of the request message.</param>
		/// <param name="fields">The name/value pairs that make up the message payload.</param>
		/// <returns>
		/// A newly instantiated <see cref="IProtocolMessage"/>-derived object that this message can
		/// deserialize to.  Null if the request isn't recognized as a valid protocol message.
		/// </returns>
		/// <remarks>
		/// The request messages are:
		/// UnauthorizedTokenRequest
		/// AuthorizedTokenRequest
		/// UserAuthorizationRequest
		/// AccessProtectedResourceRequest
		/// </remarks>
		public virtual IDirectedProtocolMessage GetNewRequestMessage(MessageReceivingEndpoint recipient, IDictionary<string, string> fields) {
			MessageBase message = null;
			Protocol protocol = Protocol.V10; // default to assuming the less-secure 1.0 instead of 1.0a until we prove otherwise.
			string token;
			fields.TryGetValue("oauth_token", out token);

			try {
				if (fields.ContainsKey("oauth_consumer_key") && !fields.ContainsKey("oauth_token")) {
					protocol = fields.ContainsKey("oauth_callback") ? Protocol.V10a : Protocol.V10;
					message = new UnauthorizedTokenRequest(recipient, protocol.Version);
				} else if (fields.ContainsKey("oauth_consumer_key") && fields.ContainsKey("oauth_token")) {
					// Discern between RequestAccessToken and AccessProtectedResources,
					// which have all the same parameters, by figuring out what type of token
					// is in the token parameter.
					bool tokenTypeIsAccessToken = this.tokenManager.GetTokenType(token) == TokenType.AccessToken;

					if (tokenTypeIsAccessToken) {
						message = (MessageBase)new AccessProtectedResourceRequest(recipient, protocol.Version);
					} else {
						// Discern between 1.0 and 1.0a requests by checking on the consumer version we stored
						// when the consumer first requested an unauthorized token.
						protocol = Protocol.Lookup(this.tokenManager.GetRequestToken(token).ConsumerVersion);
						message = new AuthorizedTokenRequest(recipient, protocol.Version);
					}
				} else {
					// fail over to the message with no required fields at all.
					if (token != null) {
						protocol = Protocol.Lookup(this.tokenManager.GetRequestToken(token).ConsumerVersion);
					}

					// If a callback parameter is included, that suggests either the consumer
					// is following OAuth 1.0 instead of 1.0a, or that a hijacker is trying
					// to attack.  Either way, if the consumer started out as a 1.0a, keep it
					// that way, and we'll just ignore the oauth_callback included in this message
					// by virtue of the UserAuthorizationRequest message not including it in its
					// 1.0a payload.
					message = new UserAuthorizationRequest(recipient, protocol.Version);
				}

				if (message != null) {
					message.SetAsIncoming();
				}

				return message;
			} catch (KeyNotFoundException ex) {
				throw ErrorUtilities.Wrap(ex, OAuthStrings.TokenNotFound);
			}
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
		public virtual IDirectResponseProtocolMessage GetNewResponseMessage(IDirectedProtocolMessage request, IDictionary<string, string> fields) {
			Logger.OAuth.Error("Service Providers are not expected to ever receive responses.");
			return null;
		}

		#endregion
	}
}
