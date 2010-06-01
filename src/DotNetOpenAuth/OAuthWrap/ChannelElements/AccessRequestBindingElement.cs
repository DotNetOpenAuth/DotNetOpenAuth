namespace DotNetOpenAuth.OAuthWrap.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;

	/// <summary>
	/// Decodes verification codes, refresh tokens and access tokens on incoming messages.
	/// </summary>
	/// <remarks>
	/// This binding element also ensures that the code/token coming in is issued to
	/// the same client that is sending the code/token and that the authorization has
	/// not been revoked and that an access token has not expired.
	/// </remarks>
	internal class AccessRequestBindingElement : AuthServerBindingElementBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessRequestBindingElement"/> class.
		/// </summary>
		internal AccessRequestBindingElement() {
		}

		public override MessageProtections Protection {
			get { return MessageProtections.None; }
		}

		protected TimeSpan AccessTokenLifetime {
			get { return TimeSpan.FromHours(1); }
		}

		public override MessageProtections? ProcessOutgoingMessage(IProtocolMessage message) {
			var tokenRequest = message as ITokenCarryingRequest;
			if (tokenRequest != null) {
				var tokenBag = (AuthorizationDataBag)tokenRequest.AuthorizationDescription;
				tokenRequest.CodeOrToken = tokenBag.Encode();

				return MessageProtections.None;
			}

			return null;
		}

		public override MessageProtections? ProcessIncomingMessage(IProtocolMessage message) {
			var tokenRequest = message as ITokenCarryingRequest;
			if (tokenRequest != null) {
				try {
					switch (tokenRequest.CodeOrTokenType) {
						case CodeOrTokenType.VerificationCode:
							tokenRequest.AuthorizationDescription = VerificationCode.Decode(this.OAuthChannel, tokenRequest.CodeOrToken, message);
							break;
						case CodeOrTokenType.RefreshToken:
							tokenRequest.AuthorizationDescription = RefreshToken.Decode(this.OAuthChannel, tokenRequest.CodeOrToken, message);
							break;
						case CodeOrTokenType.AccessToken:
							tokenRequest.AuthorizationDescription = AccessToken.Decode(this.OAuthChannel, tokenRequest.CodeOrToken, this.AccessTokenLifetime, message);
							break;
						default:
							throw ErrorUtilities.ThrowInternal("Unexpected value for CodeOrTokenType: " + tokenRequest.CodeOrTokenType);
					}
				} catch (ExpiredMessageException ex) {
					throw ErrorUtilities.Wrap(ex, Protocol.authorization_expired);
				}

				var accessRequest = message as IAccessTokenRequest;
				if (accessRequest != null) {
					// Make sure the client sending us this token is the client we issued the token to.
					ErrorUtilities.VerifyProtocol(string.Equals(accessRequest.ClientIdentifier, accessRequest.AuthorizationDescription.ClientIdentifier, StringComparison.Ordinal), Protocol.incorrect_client_credentials);

					// Check that the client secret is correct.
					var client = this.AuthorizationServer.GetClientOrThrow(accessRequest.ClientIdentifier);
					ErrorUtilities.VerifyProtocol(string.Equals(client.Secret, accessRequest.ClientSecret, StringComparison.Ordinal), Protocol.incorrect_client_credentials);
				}

				// Make sure the authorization this token represents hasn't already been revoked.
				ErrorUtilities.VerifyProtocol(this.AuthorizationServer.IsAuthorizationValid(tokenRequest.AuthorizationDescription), Protocol.authorization_expired);

				return MessageProtections.None;
			}

			return null;
		}
	}
}
