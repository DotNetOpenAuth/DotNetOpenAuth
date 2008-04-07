using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace DotNetOpenId.Provider {
	static class AssertionMessage {
		public static void CreatePositiveAssertion(EncodableResponse message,
			OpenIdProvider provider, Identifier localIdentifier, Identifier claimedIdentifier) {
			if (message == null) throw new ArgumentNullException("message");
			Protocol protocol = message.Protocol;

			message.Fields[protocol.openidnp.mode] = protocol.Args.Mode.id_res;
			message.Fields[protocol.openidnp.identity] = localIdentifier;
			message.Fields[protocol.openidnp.return_to] = message.RedirectUrl.AbsoluteUri;
			message.Signed.AddRange(new[]{
					protocol.openidnp.return_to,
					protocol.openidnp.identity,
				});
			if (protocol.Version.Major >= 2) {
				message.Fields[protocol.openidnp.claimed_id] = claimedIdentifier;
				message.Fields[protocol.openidnp.op_endpoint] = provider.Endpoint.AbsoluteUri;
				message.Fields[protocol.openidnp.response_nonce] = new Nonce().Code;
				message.Signed.AddRange(new[]{
						protocol.openidnp.claimed_id,
						protocol.openidnp.op_endpoint,
						protocol.openidnp.response_nonce,
					});
			}

			Debug.Assert(!message.Signed.Contains(protocol.openidnp.mode), "openid.mode must not be signed because it changes in check_authentication requests.");
			// The assoc_handle, signed, sig and invalidate_handle fields are added
			// as appropriate by the Signatory.Sign method.
		}

		public static void CreateNegativeAssertion(EncodableResponse message,
			bool immediateMode, Uri setupUrl) {
			if (message == null) throw new ArgumentNullException("message");
			Protocol protocol = message.Protocol;
			if (immediateMode) {
				if (protocol.Version.Major >= 2) {
					message.Fields[protocol.openidnp.mode] = protocol.Args.Mode.setup_needed;
				} else {
					message.Fields[protocol.openidnp.mode] = protocol.Args.Mode.id_res;
					message.Fields[protocol.openidnp.user_setup_url] = setupUrl.AbsoluteUri;
				}
			} else {
				message.Fields[protocol.openidnp.mode] = protocol.Args.Mode.cancel;
			}
		}

		public static EncodableResponse CreateAssertion(CheckIdRequest request) {
			if (request == null) throw new ArgumentNullException("request");
			if (!request.IsAuthenticated.HasValue) throw new InvalidOperationException();
			EncodableResponse response = EncodableResponse.PrepareIndirectMessage(
				request.Protocol, request.ReturnTo, request.AssociationHandle);
			if (request.IsAuthenticated.Value)
				AssertionMessage.CreatePositiveAssertion(response, request.Provider,
					request.LocalIdentifier, request.ClaimedIdentifier);
			else
				AssertionMessage.CreateNegativeAssertion(response, request.Immediate, request.SetupUrl);
			return response;
		}
	}
}
