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
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// Wraps a positive assertion response in an <see cref="IAuthenticationResponse"/> instance
	/// for public consumption by the host web site.
	/// </summary>
	[DebuggerDisplay("Status: {Status}, ClaimedIdentifier: {ClaimedIdentifier}")]
	internal class PositiveAuthenticationResponse : IAuthenticationResponse {
		/// <summary>
		/// The positive assertion message the Relying Party received that this instance wraps.
		/// </summary>
		private readonly PositiveAssertionResponse response;

		/// <summary>
		/// The relying party that created this request object.
		/// </summary>
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

		/// <summary>
		/// Initializes a new instance of the <see cref="PositiveAuthenticationResponse"/> class.
		/// </summary>
		/// <param name="response">The positive assertion response that was just received by the Relying Party.</param>
		/// <param name="relyingParty">The relying party.</param>
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
		public Identifier ClaimedIdentifier {
			get { return this.endpoint.ClaimedIdentifier; }
		}

		/// <summary>
		/// Gets a user-friendly OpenID Identifier for display purposes ONLY.
		/// </summary>
		/// <value></value>
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
		public string FriendlyIdentifierForDisplay {
			get { return this.endpoint.FriendlyIdentifierForDisplay; }
		}

		/// <summary>
		/// Gets the detailed success or failure status of the authentication attempt.
		/// </summary>
		/// <value></value>
		public AuthenticationStatus Status {
			get { return AuthenticationStatus.Authenticated; }
		}

		/// <summary>
		/// Gets the details regarding a failed authentication attempt, if available.
		/// This will be set if and only if <see cref="Status"/> is <see cref="AuthenticationStatus.Failed"/>.
		/// </summary>
		/// <value></value>
		public Exception Exception {
			get { return null; }
		}

		#endregion

		/// <summary>
		/// Gets the positive assertion response message.
		/// </summary>
		internal PositiveAssertionResponse Response {
			get { return this.response; }
		}

		#region IAuthenticationResponse methods

		/// <summary>
		/// Gets a callback argument's value that was previously added using
		/// <see cref="IAuthenticationRequest.AddCallbackArguments(string, string)"/>.
		/// </summary>
		/// <param name="key">The name of the parameter whose value is sought.</param>
		/// <returns>
		/// The value of the argument, or null if the named parameter could not be found.
		/// </returns>
		/// <remarks>
		/// 	<para>This may return any argument on the querystring that came with the authentication response,
		/// which may include parameters not explicitly added using
		/// <see cref="IAuthenticationRequest.AddCallbackArguments(string, string)"/>.</para>
		/// 	<para>Note that these values are NOT protected against tampering in transit.</para>
		/// </remarks>
		public string GetCallbackArgument(string key) {
			return this.response.GetReturnToArgument(key);
		}

		/// <summary>
		/// Gets all the callback arguments that were previously added using
		/// <see cref="IAuthenticationRequest.AddCallbackArguments(string, string)"/> or as a natural part
		/// of the return_to URL.
		/// </summary>
		/// <returns>A name-value dictionary.  Never null.</returns>
		/// <remarks>
		/// 	<para>This MAY return any argument on the querystring that came with the authentication response,
		/// which may include parameters not explicitly added using
		/// <see cref="IAuthenticationRequest.AddCallbackArguments(string, string)"/>.</para>
		/// 	<para>Note that these values are NOT protected against tampering in transit.</para>
		/// </remarks>
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

		/// <summary>
		/// Tries to get an OpenID extension that may be present in the response.
		/// </summary>
		/// <typeparam name="T">The type of extension to look for in the response message.</typeparam>
		/// <returns>
		/// The extension, if it is found.  Null otherwise.
		/// </returns>
		public T GetExtension<T>() where T : IOpenIdMessageExtension {
			return this.response.Extensions.OfType<T>().FirstOrDefault();
		}

		/// <summary>
		/// Tries to get an OpenID extension that may be present in the response.
		/// </summary>
		/// <param name="extensionType">Type of the extension to look for in the response.</param>
		/// <returns>
		/// The extension, if it is found.  Null otherwise.
		/// </returns>
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

			// While it LOOKS like we're performing discovery over HTTP again
			// Yadis.IdentifierDiscoveryCachePolicy is set to HttpRequestCacheLevel.CacheIfAvailable
			// which means that the .NET runtime is caching our discoveries for us.  This turns out
			// to be very fast and keeps our code clean and easily verifiable as correct and secure.
			var discoveryResults = this.response.ClaimedIdentifier.Discover(this.relyingParty.WebRequestHandler);
			ErrorUtilities.VerifyProtocol(discoveryResults.Contains(this.endpoint), OpenIdStrings.IssuedAssertionFailsIdentifierDiscovery, this.endpoint, discoveryResults.ToStringDeferred(true));
		}
	}
}
