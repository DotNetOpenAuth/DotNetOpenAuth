//-----------------------------------------------------------------------
// <copyright file="OAuthMessageTypeProvider.cs" company="Andrew Arnott">
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
	internal class OAuthMessageTypeProvider : IMessageTypeProvider {
		#region IMessageTypeProvider Members

		/// <summary>
		/// Analyzes an incoming request message payload to discover what kind of 
		/// message is embedded in it and returns the type, or null if no match is found.
		/// </summary>
		/// <param name="fields">The name/value pairs that make up the message payload.</param>
		/// <remarks>
		/// The request messages are:
		/// RequestTokenMessage
		/// RequestAccessTokenMessage
		/// DirectUserToServiceProviderMessage
		/// AccessProtectedResourcesMessage
		/// </remarks>
		/// <returns>
		/// The <see cref="IProtocolMessage"/>-derived concrete class that this message can
		/// deserialize to.  Null if the request isn't recognized as a valid protocol message.
		/// </returns>
		public Type GetRequestMessageType(IDictionary<string, string> fields) {
			if (fields == null) {
				throw new ArgumentNullException("fields");
			}

			if (fields.ContainsKey("oauth_consumer_key") &&
				!fields.ContainsKey("oauth_token")) {
				return typeof(RequestTokenMessage);
			}

			if (fields.ContainsKey("oauth_consumer_key") &&
				fields.ContainsKey("oauth_token")) {
				// Discern between RequestAccessToken and AccessProtectedResources,
				// which have all the same parameters, by figuring out what type of token
				// is in the token parameter.
				bool tokenTypeIsAccessToken = false; // TODO

				return tokenTypeIsAccessToken ? typeof(AccessProtectedResourcesMessage) :
					typeof(RequestAccessTokenMessage);
			}

			// fail over to the message with no required fields at all.
			return typeof(DirectUserToServiceProviderMessage);
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
		/// UnauthorizedRequestTookenMessage
		/// DirectUserToConsumerMessage
		/// GrantAccessTokenMessage
		/// </remarks>
		public Type GetResponseMessageType(IProtocolMessage request, IDictionary<string, string> fields) {
			if (fields == null) {
				throw new ArgumentNullException("fields");
			}

			// All response messages have the oauth_token field.
			if (!fields.ContainsKey("oauth_token")) {
				return null;
			}

			if (request == null) {
				return typeof(DirectUserToConsumerMessage);
			}

			// All direct message responses should haev the oauth_token_secret field.
			if (!fields.ContainsKey("oauth_token_secret")) {
				Logger.Error("An OAuth message was expected to contain an oauth_token_secret but didn't.");
				return null;
			}

			if (request is RequestTokenMessage) {
				return typeof(UnauthorizedRequestTokenMessage);
			} else if (request is RequestAccessTokenMessage) {
				return typeof(GrantAccessTokenMessage);
			} else {
				Logger.ErrorFormat("Unexpected response message given the request type {0}", request.GetType().Name);
				throw new ProtocolException(Strings.InvalidIncomingMessage);
			}
		}

		#endregion
	}
}
