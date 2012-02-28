//-----------------------------------------------------------------------
// <copyright file="OAuthConsumerMessageFactory.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth.Messages;

	/// <summary>
	/// An OAuth-protocol specific implementation of the <see cref="IMessageFactory"/>
	/// interface.
	/// </summary>
	public class OAuthConsumerMessageFactory : IMessageFactory {
		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthConsumerMessageFactory"/> class.
		/// </summary>
		protected internal OAuthConsumerMessageFactory() {
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
		/// UserAuthorizationResponse
		/// </remarks>
		public virtual IDirectedProtocolMessage GetNewRequestMessage(MessageReceivingEndpoint recipient, IDictionary<string, string> fields) {
			MessageBase message = null;

			if (fields.ContainsKey("oauth_token")) {
				Protocol protocol = fields.ContainsKey("oauth_verifier") ? Protocol.V10a : Protocol.V10;
				message = new UserAuthorizationResponse(recipient.Location, protocol.Version);
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
		/// A newly instantiated <see cref="IProtocolMessage"/>-derived object that this message can
		/// deserialize to.  Null if the request isn't recognized as a valid protocol message.
		/// </returns>
		/// <remarks>
		/// The response messages are:
		/// UnauthorizedTokenResponse
		/// AuthorizedTokenResponse
		/// </remarks>
		public virtual IDirectResponseProtocolMessage GetNewResponseMessage(IDirectedProtocolMessage request, IDictionary<string, string> fields) {
			MessageBase message = null;

			// All response messages have the oauth_token field.
			if (!fields.ContainsKey("oauth_token")) {
				return null;
			}

			// All direct message responses should have the oauth_token_secret field.
			if (!fields.ContainsKey("oauth_token_secret")) {
				Logger.OAuth.Error("An OAuth message was expected to contain an oauth_token_secret but didn't.");
				return null;
			}

			var unauthorizedTokenRequest = request as UnauthorizedTokenRequest;
			var authorizedTokenRequest = request as AuthorizedTokenRequest;
			if (unauthorizedTokenRequest != null) {
				Protocol protocol = fields.ContainsKey("oauth_callback_confirmed") ? Protocol.V10a : Protocol.V10;
				message = new UnauthorizedTokenResponse(unauthorizedTokenRequest, protocol.Version);
			} else if (authorizedTokenRequest != null) {
				message = new AuthorizedTokenResponse(authorizedTokenRequest);
			} else {
				Logger.OAuth.ErrorFormat("Unexpected response message given the request type {0}", request.GetType().Name);
				throw new ProtocolException(OAuthStrings.InvalidIncomingMessage);
			}

			if (message != null) {
				message.SetAsIncoming();
			}

			return message;
		}

		#endregion
	}
}
