//-----------------------------------------------------------------------
// <copyright file="PositiveAuthenticationResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System.Diagnostics;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// Wraps a positive assertion response in an <see cref="IAuthenticationResponse"/> instance
	/// for public consumption by the host web site.
	/// </summary>
	[DebuggerDisplay("Status: {Status}, ClaimedIdentifier: {ClaimedIdentifier}")]
	internal class PositiveAuthenticationResponse : PositiveAnonymousResponse {
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

		/// <summary>
		/// Initializes a new instance of the <see cref="PositiveAuthenticationResponse"/> class.
		/// </summary>
		/// <param name="response">The positive assertion response that was just received by the Relying Party.</param>
		/// <param name="relyingParty">The relying party.</param>
		internal PositiveAuthenticationResponse(PositiveAssertionResponse response, OpenIdRelyingParty relyingParty)
			: base(response) {
			Contract.Requires(relyingParty != null);
			ErrorUtilities.VerifyArgumentNotNull(relyingParty, "relyingParty");

			this.endpoint = ServiceEndpoint.CreateForClaimedIdentifier(
				this.Response.ClaimedIdentifier,
				this.Response.GetReturnToArgument(AuthenticationRequest.UserSuppliedIdentifierParameterName),
				this.Response.LocalIdentifier,
				new ProviderEndpointDescription(this.Response.ProviderEndpoint, this.Response.Version),
				null,
				null);

			this.VerifyDiscoveryMatchesAssertion(relyingParty);
		}

		#region IAuthenticationResponse Properties

		/// <summary>
		/// Gets the Identifier that the end user claims to own.  For use with user database storage and lookup.
		/// May be null for some failed authentications (i.e. failed directed identity authentications).
		/// </summary>
		/// <value></value>
		/// <remarks>
		/// 	<para>
		/// This is the secure identifier that should be used for database storage and lookup.
		/// It is not always friendly (i.e. =Arnott becomes =!9B72.7DD1.50A9.5CCD), but it protects
		/// user identities against spoofing and other attacks.
		/// </para>
		/// 	<para>
		/// For user-friendly identifiers to display, use the
		/// <see cref="FriendlyIdentifierForDisplay"/> property.
		/// </para>
		/// </remarks>
		public override Identifier ClaimedIdentifier {
			get { return this.endpoint.ClaimedIdentifier; }
		}

		/// <summary>
		/// Gets a user-friendly OpenID Identifier for display purposes ONLY.
		/// </summary>
		/// <remarks>
		/// 	<para>
		/// This <i>should</i> be put through <see cref="HttpUtility.HtmlEncode(string)"/> before
		/// sending to a browser to secure against javascript injection attacks.
		/// </para>
		/// 	<para>
		/// This property retains some aspects of the user-supplied identifier that get lost
		/// in the <see cref="ClaimedIdentifier"/>.  For example, XRIs used as user-supplied
		/// identifiers (i.e. =Arnott) become unfriendly unique strings (i.e. =!9B72.7DD1.50A9.5CCD).
		/// For display purposes, such as text on a web page that says "You're logged in as ...",
		/// this property serves to provide the =Arnott string, or whatever else is the most friendly
		/// string close to what the user originally typed in.
		/// </para>
		/// 	<para>
		/// If the user-supplied identifier is a URI, this property will be the URI after all
		/// redirects, and with the protocol and fragment trimmed off.
		/// If the user-supplied identifier is an XRI, this property will be the original XRI.
		/// If the user-supplied identifier is an OpenID Provider identifier (i.e. yahoo.com),
		/// this property will be the Claimed Identifier, with the protocol stripped if it is a URI.
		/// </para>
		/// 	<para>
		/// It is <b>very</b> important that this property <i>never</i> be used for database storage
		/// or lookup to avoid identity spoofing and other security risks.  For database storage
		/// and lookup please use the <see cref="ClaimedIdentifier"/> property.
		/// </para>
		/// </remarks>
		public override string FriendlyIdentifierForDisplay {
			get { return this.endpoint.FriendlyIdentifierForDisplay; }
		}

		/// <summary>
		/// Gets the detailed success or failure status of the authentication attempt.
		/// </summary>
		public override AuthenticationStatus Status {
			get { return AuthenticationStatus.Authenticated; }
		}

		#endregion

		/// <summary>
		/// Gets the positive assertion response message.
		/// </summary>
		protected internal new PositiveAssertionResponse Response {
			get { return (PositiveAssertionResponse)base.Response; }
		}

		/// <summary>
		/// Verifies that the positive assertion data matches the results of
		/// discovery on the Claimed Identifier.
		/// </summary>
		/// <param name="relyingParty">The relying party.</param>
		/// <exception cref="ProtocolException">
		/// Thrown when the Provider is asserting that a user controls an Identifier
		/// when discovery on that Identifier contradicts what the Provider says.
		/// This would be an indication of either a misconfigured Provider or
		/// an attempt by someone to spoof another user's identity with a rogue Provider.
		/// </exception>
		private void VerifyDiscoveryMatchesAssertion(OpenIdRelyingParty relyingParty) {
			Logger.OpenId.Debug("Verifying assertion matches identifier discovery results...");

			// While it LOOKS like we're performing discovery over HTTP again
			// Yadis.IdentifierDiscoveryCachePolicy is set to HttpRequestCacheLevel.CacheIfAvailable
			// which means that the .NET runtime is caching our discoveries for us.  This turns out
			// to be very fast and keeps our code clean and easily verifiable as correct and secure.
			// CAUTION: if this discovery is ever made to be skipped based on previous discovery
			// data that was saved to the return_to URL, be careful to verify that that information
			// is signed by the RP before it's considered reliable.  In 1.x stateless mode, this RP
			// doesn't (and can't) sign its own return_to URL, so its cached discovery information
			// is merely a hint that must be verified by performing discovery again here.
			var discoveryResults = this.Response.ClaimedIdentifier.Discover(relyingParty.WebRequestHandler);
			ErrorUtilities.VerifyProtocol(
				discoveryResults.Contains(this.endpoint),
				OpenIdStrings.IssuedAssertionFailsIdentifierDiscovery,
				this.endpoint,
				discoveryResults.ToStringDeferred(true));
		}
	}
}
