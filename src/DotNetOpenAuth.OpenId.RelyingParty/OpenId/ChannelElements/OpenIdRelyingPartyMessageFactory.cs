namespace DotNetOpenAuth.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;

	internal class OpenIdRelyingPartyMessageFactory : OpenIdMessageFactory {
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
		public override IDirectResponseProtocolMessage GetNewResponseMessage(IDirectedProtocolMessage request, IDictionary<string, string> fields) {
			DirectResponseBase message = null;

			// Discern the OpenID version of the message.
			Protocol protocol = Protocol.V11;
			string ns;
			if (fields.TryGetValue(Protocol.V20.openidnp.ns, out ns)) {
				ErrorUtilities.VerifyProtocol(string.Equals(ns, Protocol.OpenId2Namespace, StringComparison.Ordinal), MessagingStrings.UnexpectedMessagePartValue, Protocol.V20.openidnp.ns, ns);
				protocol = Protocol.V20;
			}

			// Handle error messages generally.
			if (fields.ContainsKey(protocol.openidnp.error)) {
				message = new DirectErrorResponse(protocol.Version, request);
			}

			var associateRequest = request as AssociateRequest;
			if (associateRequest != null) {
				if (protocol.Version.Major >= 2 && fields.ContainsKey(protocol.openidnp.error_code)) {
					// This is a special recognized error case that we create a special message for.
					message = new AssociateUnsuccessfulResponse(protocol.Version, associateRequest);
				} else if (message == null) {
					var associateDiffieHellmanRequest = request as AssociateDiffieHellmanRequest;
					var associateUnencryptedRequest = request as AssociateUnencryptedRequest;

					if (associateDiffieHellmanRequest != null) {
						message = new AssociateDiffieHellmanRelyingPartyResponse(protocol.Version, associateDiffieHellmanRequest);
					}

					if (associateUnencryptedRequest != null) {
						message = new AssociateUnencryptedResponse(protocol.Version, associateUnencryptedRequest);
					}
				}
			}

			var checkAuthenticationRequest = request as CheckAuthenticationRequest;
			if (checkAuthenticationRequest != null && message == null) {
				message = new CheckAuthenticationResponse(protocol.Version, checkAuthenticationRequest);
			}

			if (message != null) {
				message.SetAsIncoming();
			}

			return message;
		}
	}
}
