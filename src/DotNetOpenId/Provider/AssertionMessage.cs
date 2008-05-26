using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;

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
			if (request.IsAuthenticated.Value) {
				AssertionMessage.CreatePositiveAssertion(response, request.Provider,
					request.LocalIdentifier, request.ClaimedIdentifier);
				if (TraceUtil.Switch.TraceInfo)
					Trace.TraceInformation("Created positive assertion for {0}.", request.ClaimedIdentifier);
			} else {
				AssertionMessage.CreateNegativeAssertion(response, request.Immediate, request.SetupUrl);
				if (TraceUtil.Switch.TraceInfo)
					Trace.TraceInformation("Created negative assertion for {0}.", request.ClaimedIdentifier);
			}
			return response;
		}

		/// <summary>
		/// Creates a message that can be sent to a user agent to redirect them to a 
		/// relying party web site complete with authentication information to 
		/// automatically log them into that web site.
		/// </summary>
		public static IResponse CreateUnsolicitedAssertion(OpenIdProvider provider,
			Realm relyingParty, Identifier claimedIdentifier, Identifier localIdentifier) {
			if (relyingParty == null) throw new ArgumentNullException("relyingParty");
			if (claimedIdentifier == null) throw new ArgumentNullException("claimedIdentifier");
			if (localIdentifier == null) throw new ArgumentNullException("localIdentifier");

			var discoveredEndpoints = new List<RelyingPartyReceivingEndpoint>(relyingParty.Discover(true));
			if (discoveredEndpoints.Count == 0) throw new OpenIdException(
				string.Format(CultureInfo.CurrentCulture, Strings.NoRelyingPartyEndpointDiscovered,
				relyingParty.NoWildcardUri));
			var selectedEndpoint = discoveredEndpoints[0];

			EncodableResponse message = EncodableResponse.PrepareIndirectMessage(
				selectedEndpoint.Protocol, selectedEndpoint.RelyingPartyEndpoint, null);
			CreatePositiveAssertion(message, provider, localIdentifier, claimedIdentifier);
			return provider.Encoder.Encode(message);
		}
	}
}
