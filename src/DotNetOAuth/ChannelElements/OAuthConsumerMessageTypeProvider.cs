//-----------------------------------------------------------------------
// <copyright file="OAuthConsumerMessageTypeProvider.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using DotNetOAuth.Messages;
	using DotNetOAuth.Messaging;

	/// <summary>
	/// An OAuth-protocol specific implementation of the <see cref="IMessageTypeProvider"/>
	/// interface.
	/// </summary>
	public class OAuthConsumerMessageTypeProvider : IMessageTypeProvider {
		/// <summary>
		/// The token manager to use for discerning between request and access tokens.
		/// </summary>
		private ITokenManager tokenManager;

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthConsumerMessageTypeProvider"/> class.
		/// </summary>
		/// <param name="tokenManager">The token manager instance to use.</param>
		protected internal OAuthConsumerMessageTypeProvider(ITokenManager tokenManager) {
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
		/// DirectUserToConsumerMessage
		/// </remarks>
		/// <returns>
		/// The <see cref="IProtocolMessage"/>-derived concrete class that this message can
		/// deserialize to.  Null if the request isn't recognized as a valid protocol message.
		/// </returns>
		public virtual Type GetRequestMessageType(IDictionary<string, string> fields) {
			if (fields == null) {
				throw new ArgumentNullException("fields");
			}

			if (fields.ContainsKey("oauth_token")) {
				return typeof(DirectUserToConsumerMessage);
			}

			return null;
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
		/// GrantRequestTokenMessage
		/// GrantAccessTokenMessage
		/// </remarks>
		public virtual Type GetResponseMessageType(IProtocolMessage request, IDictionary<string, string> fields) {
			if (fields == null) {
				throw new ArgumentNullException("fields");
			}

			// All response messages have the oauth_token field.
			if (!fields.ContainsKey("oauth_token")) {
				return null;
			}

			// All direct message responses should have the oauth_token_secret field.
			if (!fields.ContainsKey("oauth_token_secret")) {
				Logger.Error("An OAuth message was expected to contain an oauth_token_secret but didn't.");
				return null;
			}

			if (request is GetRequestTokenMessage) {
				return typeof(GrantRequestTokenMessage);
			} else if (request is GetAccessTokenMessage) {
				return typeof(GrantAccessTokenMessage);
			} else {
				Logger.ErrorFormat("Unexpected response message given the request type {0}", request.GetType().Name);
				throw new ProtocolException(Strings.InvalidIncomingMessage);
			}
		}

		#endregion
	}
}
