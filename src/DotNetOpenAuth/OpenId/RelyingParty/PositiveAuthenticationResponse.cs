//-----------------------------------------------------------------------
// <copyright file="PositiveAuthenticationResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;

	[DebuggerDisplay("Status: {Status}, ClaimedIdentifier: {ClaimedIdentifier}")]
	internal class PositiveAuthenticationResponse : IAuthenticationResponse {
		private readonly PositiveAssertionResponse response;
		private readonly OpenIdRelyingParty relyingParty;

		/// <summary>
		/// The OpenID service endpoint reconstructed from the assertion message.
		/// </summary>
		/// <remarks>
		/// This information is straight from the Provider, and therefore must not
		/// be trusted until verified as matching the discovery information for
		/// the claimed identifier to avoid a Provider asserting an Identifier
		/// for which it has no authority. 
		/// </remarks>
		private readonly ServiceEndpoint endpoint;

		internal PositiveAuthenticationResponse(PositiveAssertionResponse response, OpenIdRelyingParty relyingParty) {
			ErrorUtilities.VerifyArgumentNotNull(response, "response");
			ErrorUtilities.VerifyArgumentNotNull(relyingParty, "relyingParty");

			this.response = response;
			this.relyingParty = relyingParty;

			this.endpoint = ServiceEndpoint.CreateForClaimedIdentifier(
				this.response.ClaimedIdentifier,
				this.response.GetReturnToArgument(AuthenticationRequest.UserSuppliedIdentifierParameterName),
				this.response.LocalIdentifier,
				new ProviderEndpointDescription(this.response.ProviderEndpoint, this.response.Version),
				null,
				null);

			this.VerifyDiscoveryMatchesAssertion();
		}

		#region IAuthenticationResponse Members

		public Identifier ClaimedIdentifier {
			get { return this.endpoint.ClaimedIdentifier; }
		}

		public string FriendlyIdentifierForDisplay {
			get { return this.endpoint.FriendlyIdentifierForDisplay; }
		}

		public AuthenticationStatus Status {
			get { return AuthenticationStatus.Authenticated; }
		}

		public Exception Exception {
			get { return null; }
		}

		public string GetCallbackArgument(string key) {
			return this.response.GetReturnToArgument(key);
		}

		public IDictionary<string, string> GetCallbackArguments() {
			var args = new Dictionary<string, string>();

			// Return all the return_to arguments, except for the OpenID-supporting ones.
			// The only arguments that should be returned here are the ones that the host
			// web site adds explicitly.
			foreach (string key in this.response.GetReturnToParameterNames().Where(key => !OpenIdRelyingParty.IsOpenIdSupportingParameter(key))) {
				args[key] = this.response.GetReturnToArgument(key);
			}

			return args;
		}

		public T GetExtension<T>() where T : IOpenIdMessageExtension, new() {
			return this.response.Extensions.OfType<T>().FirstOrDefault();
		}

		public IOpenIdMessageExtension GetExtension(Type extensionType) {
			ErrorUtilities.VerifyArgumentNotNull(extensionType, "extensionType");
			return this.response.Extensions.OfType<IOpenIdMessageExtension>().Where(ext => extensionType.IsInstanceOfType(ext)).FirstOrDefault();
		}

		#endregion

		/// <summary>
		/// Verifies that the positive assertion data matches the results of
		/// discovery on the Claimed Identifier.
		/// </summary>
		/// <exception cref="ProtocolException">
		/// Thrown when the Provider is asserting that a user controls an Identifier
		/// when discovery on that Identifier contradicts what the Provider says.
		/// This would be an indication of either a misconfigured Provider or
		/// an attempt by someone to spoof another user's identity with a rogue Provider.
		/// </exception>
		private void VerifyDiscoveryMatchesAssertion() {
			Logger.Debug("Verifying assertion matches identifier discovery results...");

			// TODO: optimize this to not perform a second discovery when we could cache it
			// either through the return_to URL or application state.
			// PROPOSAL: sign the discovered information in the request so that when it
			// comes back in the assertion we can verify that it hasn't changed, without
			// sending two copies of all the data in the request.
			var discoveryResults = this.response.ClaimedIdentifier.Discover(this.relyingParty.WebRequestHandler);
			ErrorUtilities.VerifyProtocol(discoveryResults.Contains(this.endpoint), OpenIdStrings.IssuedAssertionFailsIdentifierDiscovery);
		}
	}
}
