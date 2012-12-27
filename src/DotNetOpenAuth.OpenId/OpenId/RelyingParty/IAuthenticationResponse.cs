//-----------------------------------------------------------------------
// <copyright file="IAuthenticationResponse.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.Text;
	using System.Web;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Messages;
	using Validation;

	/// <summary>
	/// An instance of this interface represents an identity assertion 
	/// from an OpenID Provider.  It may be in response to an authentication 
	/// request previously put to it by a Relying Party site or it may be an
	/// unsolicited assertion.
	/// </summary>
	/// <remarks>
	/// Relying party web sites should handle both solicited and unsolicited 
	/// assertions.  This interface does not offer a way to discern between
	/// solicited and unsolicited assertions as they should be treated equally.
	/// </remarks>
	public interface IAuthenticationResponse {
		/// <summary>
		/// Gets the Identifier that the end user claims to own.  For use with user database storage and lookup.
		/// May be null for some failed authentications (i.e. failed directed identity authentications).
		/// </summary>
		/// <remarks>
		/// <para>
		/// This is the secure identifier that should be used for database storage and lookup.
		/// It is not always friendly (i.e. =Arnott becomes =!9B72.7DD1.50A9.5CCD), but it protects
		/// user identities against spoofing and other attacks.  
		/// </para>
		/// <para>
		/// For user-friendly identifiers to display, use the 
		/// <see cref="FriendlyIdentifierForDisplay"/> property.
		/// </para>
		/// </remarks>
		Identifier ClaimedIdentifier { get; }

		/// <summary>
		/// Gets a user-friendly OpenID Identifier for display purposes ONLY.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This <i>should</i> be put through <see cref="HttpUtility.HtmlEncode(string)"/> before
		/// sending to a browser to secure against javascript injection attacks.
		/// </para>
		/// <para>
		/// This property retains some aspects of the user-supplied identifier that get lost
		/// in the <see cref="ClaimedIdentifier"/>.  For example, XRIs used as user-supplied
		/// identifiers (i.e. =Arnott) become unfriendly unique strings (i.e. =!9B72.7DD1.50A9.5CCD).
		/// For display purposes, such as text on a web page that says "You're logged in as ...",
		/// this property serves to provide the =Arnott string, or whatever else is the most friendly
		/// string close to what the user originally typed in.
		/// </para>
		/// <para>
		/// If the user-supplied identifier is a URI, this property will be the URI after all 
		/// redirects, and with the protocol and fragment trimmed off.
		/// If the user-supplied identifier is an XRI, this property will be the original XRI.
		/// If the user-supplied identifier is an OpenID Provider identifier (i.e. yahoo.com), 
		/// this property will be the Claimed Identifier, with the protocol stripped if it is a URI.
		/// </para>
		/// <para>
		/// It is <b>very</b> important that this property <i>never</i> be used for database storage
		/// or lookup to avoid identity spoofing and other security risks.  For database storage
		/// and lookup please use the <see cref="ClaimedIdentifier"/> property.
		/// </para>
		/// </remarks>
		string FriendlyIdentifierForDisplay { get; }

		/// <summary>
		/// Gets the detailed success or failure status of the authentication attempt.
		/// </summary>
		AuthenticationStatus Status { get; }

		/// <summary>
		/// Gets information about the OpenId Provider, as advertised by the
		/// OpenID discovery documents found at the <see cref="ClaimedIdentifier"/>
		/// location, if available.
		/// </summary>
		/// <value>
		/// The Provider endpoint that issued the positive assertion;
		/// or <c>null</c> if information about the Provider is unavailable.
		/// </value>
		IProviderEndpoint Provider { get; }

		/// <summary>
		/// Gets the details regarding a failed authentication attempt, if available.
		/// This will be set if and only if <see cref="Status"/> is <see cref="AuthenticationStatus.Failed"/>.
		/// </summary>
		Exception Exception { get; }

		/// <summary>
		/// Gets a callback argument's value that was previously added using
		/// <see cref="IAuthenticationRequest.AddCallbackArguments(string, string)"/>.
		/// </summary>
		/// <param name="key">The name of the parameter whose value is sought.</param>
		/// <returns>
		/// The value of the argument, or null if the named parameter could not be found.
		/// </returns>
		/// <remarks>
		/// Callback parameters are only available if they are complete and untampered with
		/// since the original request message (as proven by a signature).
		/// If the relying party is operating in stateless mode <c>null</c> is always
		/// returned since the callback arguments could not be signed to protect against
		/// tampering.
		/// </remarks>
		string GetCallbackArgument(string key);

		/// <summary>
		/// Gets a callback argument's value that was previously added using
		/// <see cref="IAuthenticationRequest.AddCallbackArguments(string, string)"/>.
		/// </summary>
		/// <param name="key">The name of the parameter whose value is sought.</param>
		/// <returns>
		/// The value of the argument, or null if the named parameter could not be found.
		/// </returns>
		/// <remarks>
		/// Callback parameters are only available even if the RP is in stateless mode,
		/// or the callback parameters are otherwise unverifiable as untampered with.
		/// Therefore, use this method only when the callback argument is not to be
		/// used to make a security-sensitive decision.
		/// </remarks>
		string GetUntrustedCallbackArgument(string key);

		/// <summary>
		/// Gets all the callback arguments that were previously added using 
		/// <see cref="IAuthenticationRequest.AddCallbackArguments(string, string)"/> or as a natural part
		/// of the return_to URL.
		/// </summary>
		/// <returns>A name-value dictionary.  Never null.</returns>
		/// <remarks>
		/// Callback parameters are only available if they are complete and untampered with
		/// since the original request message (as proven by a signature).
		/// If the relying party is operating in stateless mode an empty dictionary is always
		/// returned since the callback arguments could not be signed to protect against
		/// tampering.
		/// </remarks>
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Historically an expensive operation.")]
		IDictionary<string, string> GetCallbackArguments();

		/// <summary>
		/// Gets all the callback arguments that were previously added using 
		/// <see cref="IAuthenticationRequest.AddCallbackArguments(string, string)"/> or as a natural part
		/// of the return_to URL.
		/// </summary>
		/// <returns>A name-value dictionary.  Never null.</returns>
		/// <remarks>
		/// Callback parameters are only available even if the RP is in stateless mode,
		/// or the callback parameters are otherwise unverifiable as untampered with.
		/// Therefore, use this method only when the callback argument is not to be
		/// used to make a security-sensitive decision.
		/// </remarks>
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Historically an expensive operation.")]
		IDictionary<string, string> GetUntrustedCallbackArguments();

		/// <summary>
		/// Tries to get an OpenID extension that may be present in the response.
		/// </summary>
		/// <typeparam name="T">The type of extension to look for in the response message.</typeparam>
		/// <returns>
		/// The extension, if it is found.  Null otherwise.
		/// </returns>
		/// <remarks>
		/// <para>Extensions are returned only if the Provider signed them. 
		/// Relying parties that do not care if the values were modified in
		/// transit should use the <see cref="GetUntrustedExtension&lt;T&gt;"/> method
		/// in order to allow the Provider to not sign the extension. </para>
		/// <para>Unsigned extensions are completely unreliable and should be
		/// used only to prefill user forms since the user or any other third
		/// party may have tampered with the data carried by the extension.</para>
		/// <para>Signed extensions are only reliable if the relying party
		/// trusts the OpenID Provider that signed them.  Signing does not mean
		/// the relying party can trust the values -- it only means that the values
		/// have not been tampered with since the Provider sent the message.</para>
		/// </remarks>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "No parameter at all is required.  T is used for return type.")]
		T GetExtension<T>() where T : IOpenIdMessageExtension;

		/// <summary>
		/// Tries to get an OpenID extension that may be present in the response.
		/// </summary>
		/// <param name="extensionType">Type of the extension to look for in the response.</param>
		/// <returns>
		/// The extension, if it is found.  Null otherwise.
		/// </returns>
		/// <remarks>
		/// <para>Extensions are returned only if the Provider signed them. 
		/// Relying parties that do not care if the values were modified in
		/// transit should use the <see cref="GetUntrustedExtension"/> method
		/// in order to allow the Provider to not sign the extension. </para>
		/// <para>Unsigned extensions are completely unreliable and should be
		/// used only to prefill user forms since the user or any other third
		/// party may have tampered with the data carried by the extension.</para>
		/// <para>Signed extensions are only reliable if the relying party
		/// trusts the OpenID Provider that signed them.  Signing does not mean
		/// the relying party can trust the values -- it only means that the values
		/// have not been tampered with since the Provider sent the message.</para>
		/// </remarks>
		IOpenIdMessageExtension GetExtension(Type extensionType);

		/// <summary>
		/// Tries to get an OpenID extension that may be present in the response, without
		/// requiring it to be signed by the Provider.
		/// </summary>
		/// <typeparam name="T">The type of extension to look for in the response message.</typeparam>
		/// <returns>
		/// The extension, if it is found.  Null otherwise.
		/// </returns>
		/// <remarks>
		/// <para>Extensions are returned whether they are signed or not.  
		/// Use the <see cref="GetExtension&lt;T&gt;"/> method to retrieve
		/// extension responses only if they are signed by the Provider to
		/// protect against tampering. </para>
		/// <para>Unsigned extensions are completely unreliable and should be
		/// used only to prefill user forms since the user or any other third
		/// party may have tampered with the data carried by the extension.</para>
		/// <para>Signed extensions are only reliable if the relying party
		/// trusts the OpenID Provider that signed them.  Signing does not mean
		/// the relying party can trust the values -- it only means that the values
		/// have not been tampered with since the Provider sent the message.</para>
		/// </remarks>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "No parameter at all is required.  T is used for return type.")]
		T GetUntrustedExtension<T>() where T : IOpenIdMessageExtension;

		/// <summary>
		/// Tries to get an OpenID extension that may be present in the response, without
		/// requiring it to be signed by the Provider.
		/// </summary>
		/// <param name="extensionType">Type of the extension to look for in the response.</param>
		/// <returns>
		/// The extension, if it is found.  Null otherwise.
		/// </returns>
		/// <remarks>
		/// <para>Extensions are returned whether they are signed or not.  
		/// Use the <see cref="GetExtension"/> method to retrieve
		/// extension responses only if they are signed by the Provider to
		/// protect against tampering. </para>
		/// <para>Unsigned extensions are completely unreliable and should be
		/// used only to prefill user forms since the user or any other third
		/// party may have tampered with the data carried by the extension.</para>
		/// <para>Signed extensions are only reliable if the relying party
		/// trusts the OpenID Provider that signed them.  Signing does not mean
		/// the relying party can trust the values -- it only means that the values
		/// have not been tampered with since the Provider sent the message.</para>
		/// </remarks>
		IOpenIdMessageExtension GetUntrustedExtension(Type extensionType);
	}
}
