//-----------------------------------------------------------------------
// <copyright file="OAuthWrapMessageFactory.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuthWrap.Messages;

	/// <summary>
	/// The message factory for OAuth WRAP messages.
	/// </summary>
	internal class OAuthWrapMessageFactory : IMessageFactory {
		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthWrapMessageFactory"/> class.
		/// </summary>
		internal OAuthWrapMessageFactory() {
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
		public IDirectedProtocolMessage GetNewRequestMessage(MessageReceivingEndpoint recipient, IDictionary<string, string> fields) {
			Version version = Protocol.DefaultVersion;

			if (fields.ContainsKey(Protocol.wrap_client_id) && fields.ContainsKey(Protocol.wrap_callback)) {
				return new WebAppRequest(recipient.Location, version);
			}

			if (fields.ContainsKey(Protocol.wrap_client_id) && fields.ContainsKey(Protocol.wrap_verification_code)) {
				return new WebAppAccessTokenRequest(recipient.Location, version);
			}

			if (fields.ContainsKey(Protocol.wrap_name)) {
				return new ClientAccountUsernamePasswordRequest(recipient.Location, version);
			}

			if (fields.ContainsKey(Protocol.wrap_username)) {
				return new UserNamePasswordRequest(recipient.Location, version);
			}

			if (fields.ContainsKey(Protocol.wrap_verification_code)) {
				return new WebAppSuccessResponse(recipient.Location, version);
			}

			return null;
		}

		/// <summary>
		/// Analyzes an incoming request message payload to discover what kind of
		/// message is embedded in it and returns the type, or null if no match is found.
		/// </summary>
		/// <param name="request">The message that was sent as a request that resulted in the response.</param>
		/// <param name="fields">The name/value pairs that make up the message payload.</param>
		/// <returns>
		/// A newly instantiated <see cref="IProtocolMessage"/>-derived object that this message can
		/// deserialize to.  Null if the request isn't recognized as a valid protocol message.
		/// </returns>
		public IDirectResponseProtocolMessage GetNewResponseMessage(IDirectedProtocolMessage request, IDictionary<string, string> fields) {
			Version version = Protocol.DefaultVersion;

			var accessTokenRequest = request as WebAppAccessTokenRequest;
			if (accessTokenRequest != null) {
				if (fields.ContainsKey(Protocol.wrap_access_token)) {
					return new WebAppAccessTokenSuccessResponse(accessTokenRequest);
				} else {
					//return new AccessTokenWithVerificationCodeFailedResponse(accessTokenRequest);
				}
			}

			var userAuthorization = request as UserNamePasswordRequest;
			if (userAuthorization != null) {
				if (fields.ContainsKey(Protocol.wrap_verification_code)) {
					return new UserNamePasswordSuccessResponse(userAuthorization);
				} else {
					//return new UserAuthorizationViaUsernamePasswordFailedResponse(userAuthorization);
				}
			}

			return null;
		}

		#endregion
	}
}
