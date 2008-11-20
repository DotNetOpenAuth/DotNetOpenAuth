//-----------------------------------------------------------------------
// <copyright file="OAuthServiceProviderMessageFactory.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth.Messages;

	/// <summary>
	/// An OAuth-protocol specific implementation of the <see cref="IMessageFactory"/>
	/// interface.
	/// </summary>
	public class OAuthServiceProviderMessageFactory : IMessageFactory {
		/// <summary>
		/// The token manager to use for discerning between request and access tokens.
		/// </summary>
		private ITokenManager tokenManager;

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthServiceProviderMessageFactory"/> class.
		/// </summary>
		/// <param name="tokenManager">The token manager instance to use.</param>
		protected internal OAuthServiceProviderMessageFactory(ITokenManager tokenManager) {
			ErrorUtilities.VerifyArgumentNotNull(tokenManager, "tokenManager");

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
			ErrorUtilities.VerifyArgumentNotNull(recipient, "recipient");
			ErrorUtilities.VerifyArgumentNotNull(fields, "fields");

			MessageBase message = null;

			if (fields.ContainsKey("oauth_consumer_key") &&
				!fields.ContainsKey("oauth_token")) {
				message = new UnauthorizedTokenRequest(recipient);
			} else if (fields.ContainsKey("oauth_consumer_key") &&
				fields.ContainsKey("oauth_token")) {
				// Discern between RequestAccessToken and AccessProtectedResources,
				// which have all the same parameters, by figuring out what type of token
				// is in the token parameter.
				bool tokenTypeIsAccessToken = this.tokenManager.GetTokenType(fields["oauth_token"]) == TokenType.AccessToken;

				message = tokenTypeIsAccessToken ? (MessageBase)new AccessProtectedResourceRequest(recipient) :
					new AuthorizedTokenRequest(recipient);
			} else {
				// fail over to the message with no required fields at all.
				message = new UserAuthorizationRequest(recipient);
			}

			if (message != null) {
				message.SetAsIncoming();
			}

			return message;
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
			ErrorUtilities.VerifyArgumentNotNull(request, "request");
			ErrorUtilities.VerifyArgumentNotNull(fields, "fields");

			Logger.Error("Service Providers are not expected to ever receive responses.");
			return null;
		}

		#endregion
	}
}
