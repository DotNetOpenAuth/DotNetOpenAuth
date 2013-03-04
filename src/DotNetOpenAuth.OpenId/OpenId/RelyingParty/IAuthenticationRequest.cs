//-----------------------------------------------------------------------
// <copyright file="IAuthenticationRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// Instances of this interface represent relying party authentication 
	/// requests that may be queried/modified in specific ways before being
	/// routed to the OpenID Provider.
	/// </summary>
	public interface IAuthenticationRequest {
		/// <summary>
		/// Gets or sets the mode the Provider should use during authentication.
		/// </summary>
		AuthenticationRequestMode Mode { get; set; }

		/// <summary>
		/// Gets the URL that the user agent will return to after authentication
		/// completes or fails at the Provider.
		/// </summary>
		Uri ReturnToUrl { get; }

		/// <summary>
		/// Gets the URL that identifies this consumer web application that
		/// the Provider will display to the end user.
		/// </summary>
		Realm Realm { get; }

		/// <summary>
		/// Gets the Claimed Identifier that the User Supplied Identifier
		/// resolved to.  Null if the user provided an OP Identifier 
		/// (directed identity).
		/// </summary>
		/// <remarks>
		/// Null is returned if the user is using the directed identity feature
		/// of OpenID 2.0 to make it nearly impossible for a relying party site
		/// to improperly store the reserved OpenID URL used for directed identity
		/// as a user's own Identifier.  
		/// However, to test for the Directed Identity feature, please test the
		/// <see cref="IsDirectedIdentity"/> property rather than testing this 
		/// property for a null value.
		/// </remarks>
		Identifier ClaimedIdentifier { get; }

		/// <summary>
		/// Gets a value indicating whether the authenticating user has chosen to let the Provider
		/// determine and send the ClaimedIdentifier after authentication.
		/// </summary>
		bool IsDirectedIdentity { get; }

		/// <summary>
		/// Gets or sets a value indicating whether this request only carries extensions
		/// and is not a request to verify that the user controls some identifier.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this request is merely a carrier of extensions and is not
		/// about an OpenID identifier; otherwise, <c>false</c>.
		/// </value>
		/// <remarks>
		/// 	<para>Although OpenID is first and primarily an authentication protocol, its extensions
		/// can be interesting all by themselves.  For instance, a relying party might want
		/// to know that its user is over 21 years old, or perhaps a member of some organization.
		/// OpenID extensions can provide this, without any need for asserting the identity of the user.</para>
		/// 	<para>Constructing an OpenID request for only extensions can be done by calling
		/// OpenIdRelyingParty.CreateRequest with any valid OpenID identifier
		/// (claimed identifier or OP identifier).  But once this property is set to <c>true</c>,
		/// the claimed identifier value in the request is not included in the transmitted message.</para>
		/// 	<para>It is anticipated that an RP would only issue these types of requests to OPs that
		/// trusts to make assertions regarding the individual holding an account at that OP, so it
		/// is not likely that the RP would allow the user to type in an arbitrary claimed identifier
		/// without checking that it resolved to an OP endpoint the RP has on a trust whitelist.</para>
		/// </remarks>
		bool IsExtensionOnly { get; set; }

		/// <summary>
		/// Gets information about the OpenId Provider, as advertised by the
		/// OpenID discovery documents found at the <see cref="ClaimedIdentifier"/>
		/// location.
		/// </summary>
		IProviderEndpoint Provider { get; }

		/// <summary>
		/// Gets the discovery result leading to the formulation of this request.
		/// </summary>
		/// <value>The discovery result.</value>
		IdentifierDiscoveryResult DiscoveryResult { get; }

		/// <summary>
		/// Makes a dictionary of key/value pairs available when the authentication is completed.
		/// </summary>
		/// <param name="arguments">The arguments to add to the request's return_to URI.  Values must not be null.</param>
		/// <remarks>
		/// 	<para>Note that these values are NOT protected against eavesdropping in transit.  No
		/// privacy-sensitive data should be stored using this method.</para>
		/// 	<para>The values stored here can be retrieved using
		/// <see cref="IAuthenticationResponse.GetCallbackArgument"/>, which will only return the value
		/// if it can be verified as untampered with in transit.</para>
		/// 	<para>Since the data set here is sent in the querystring of the request and some
		/// servers place limits on the size of a request URL, this data should be kept relatively
		/// small to ensure successful authentication.  About 1.5KB is about all that should be stored.</para>
		/// </remarks>
		void AddCallbackArguments(IDictionary<string, string> arguments);

		/// <summary>
		/// Makes a key/value pair available when the authentication is completed.
		/// </summary>
		/// <param name="key">The parameter name.</param>
		/// <param name="value">The value of the argument.  Must not be null.</param>
		/// <remarks>
		/// 	<para>Note that these values are NOT protected against eavesdropping in transit.  No
		/// privacy-sensitive data should be stored using this method.</para>
		/// 	<para>The value stored here can be retrieved using
		/// <see cref="IAuthenticationResponse.GetCallbackArgument"/>, which will only return the value
		/// if it can be verified as untampered with in transit.</para>
		/// 	<para>Since the data set here is sent in the querystring of the request and some
		/// servers place limits on the size of a request URL, this data should be kept relatively
		/// small to ensure successful authentication.  About 1.5KB is about all that should be stored.</para>
		/// </remarks>
		void AddCallbackArguments(string key, string value);

		/// <summary>
		/// Makes a key/value pair available when the authentication is completed.
		/// </summary>
		/// <param name="key">The parameter name.</param>
		/// <param name="value">The value of the argument.  Must not be null.</param>
		/// <remarks>
		/// 	<para>Note that these values are NOT protected against eavesdropping in transit.  No
		/// security-sensitive data should be stored using this method.</para>
		/// 	<para>The value stored here can be retrieved using
		/// <see cref="IAuthenticationResponse.GetCallbackArgument"/>.</para>
		/// 	<para>Since the data set here is sent in the querystring of the request and some
		/// servers place limits on the size of a request URL, this data should be kept relatively
		/// small to ensure successful authentication.  About 1.5KB is about all that should be stored.</para>
		/// </remarks>
		void SetCallbackArgument(string key, string value);

		/// <summary>
		/// Makes a key/value pair available when the authentication is completed without
		/// requiring a return_to signature to protect against tampering of the callback argument.
		/// </summary>
		/// <param name="key">The parameter name.</param>
		/// <param name="value">The value of the argument.  Must not be null.</param>
		/// <remarks>
		/// 	<para>Note that these values are NOT protected against eavesdropping or tampering in transit.  No
		/// security-sensitive data should be stored using this method. </para>
		/// 	<para>The value stored here can be retrieved using
		/// <see cref="IAuthenticationResponse.GetCallbackArgument"/>.</para>
		/// 	<para>Since the data set here is sent in the querystring of the request and some
		/// servers place limits on the size of a request URL, this data should be kept relatively
		/// small to ensure successful authentication.  About 1.5KB is about all that should be stored.</para>
		/// </remarks>
		void SetUntrustedCallbackArgument(string key, string value);

		/// <summary>
		/// Adds an OpenID extension to the request directed at the OpenID provider.
		/// </summary>
		/// <param name="extension">The initialized extension to add to the request.</param>
		void AddExtension(IOpenIdMessageExtension extension);

		/// <summary>
		/// Gets the HTTP response the relying party should send to the user agent
		/// to redirect it to the OpenID Provider to start the OpenID authentication process.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The response message that will cause the client to redirect to the Provider.</returns>
		Task<HttpResponseMessage> GetRedirectingResponseAsync(CancellationToken cancellationToken = default(CancellationToken));
	}
}
