//-----------------------------------------------------------------------
// <copyright file="PositiveAuthenticationResponseSnapshot.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;
	using Validation;

	/// <summary>
	/// A serializable snapshot of a verified authentication message.
	/// </summary>
	[Serializable]
	internal class PositiveAuthenticationResponseSnapshot : IAuthenticationResponse {
		/// <summary>
		/// The callback arguments that came with the authentication response.
		/// </summary>
		private IDictionary<string, string> callbackArguments;

		/// <summary>
		/// The untrusted callback arguments that came with the authentication response.
		/// </summary>
		private IDictionary<string, string> untrustedCallbackArguments;

		/// <summary>
		/// Initializes a new instance of the <see cref="PositiveAuthenticationResponseSnapshot"/> class.
		/// </summary>
		/// <param name="copyFrom">The authentication response to copy from.</param>
		internal PositiveAuthenticationResponseSnapshot(IAuthenticationResponse copyFrom) {
			Requires.NotNull(copyFrom, "copyFrom");

			this.ClaimedIdentifier = copyFrom.ClaimedIdentifier;
			this.FriendlyIdentifierForDisplay = copyFrom.FriendlyIdentifierForDisplay;
			this.Status = copyFrom.Status;
			this.Provider = copyFrom.Provider;
			this.untrustedCallbackArguments = copyFrom.GetUntrustedCallbackArguments();

			// Do this special check to avoid logging a warning for trying to clone a dictionary.
			var anonResponse = copyFrom as PositiveAnonymousResponse;
			if (anonResponse == null || anonResponse.TrustedCallbackArgumentsAvailable) {
				this.callbackArguments = copyFrom.GetCallbackArguments();
			} else {
				this.callbackArguments = EmptyDictionary<string, string>.Instance;
			}
		}

		#region IAuthenticationResponse Members

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
		public Identifier ClaimedIdentifier { get; private set; }

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
		public string FriendlyIdentifierForDisplay { get; private set; }

		/// <summary>
		/// Gets the detailed success or failure status of the authentication attempt.
		/// </summary>
		/// <value></value>
		public AuthenticationStatus Status { get; private set; }

		/// <summary>
		/// Gets information about the OpenId Provider, as advertised by the
		/// OpenID discovery documents found at the <see cref="ClaimedIdentifier"/>
		/// location.
		/// </summary>
		/// <value>
		/// The Provider endpoint that issued the positive assertion;
		/// or <c>null</c> if information about the Provider is unavailable.
		/// </value>
		public IProviderEndpoint Provider { get; private set; }

		/// <summary>
		/// Gets the details regarding a failed authentication attempt, if available.
		/// This will be set if and only if <see cref="Status"/> is <see cref="AuthenticationStatus.Failed"/>.
		/// </summary>
		/// <value></value>
		public Exception Exception {
			get { throw new NotSupportedException(OpenIdStrings.NotSupportedByAuthenticationSnapshot); }
		}

		/// <summary>
		/// Tries to get an OpenID extension that may be present in the response.
		/// </summary>
		/// <typeparam name="T">The type of extension to look for in the response message.</typeparam>
		/// <returns>
		/// The extension, if it is found.  Null otherwise.
		/// </returns>
		/// <remarks>
		/// 	<para>Extensions are returned only if the Provider signed them.
		/// Relying parties that do not care if the values were modified in
		/// transit should use the <see cref="GetUntrustedExtension&lt;T&gt;"/> method
		/// in order to allow the Provider to not sign the extension. </para>
		/// 	<para>Unsigned extensions are completely unreliable and should be
		/// used only to prefill user forms since the user or any other third
		/// party may have tampered with the data carried by the extension.</para>
		/// 	<para>Signed extensions are only reliable if the relying party
		/// trusts the OpenID Provider that signed them.  Signing does not mean
		/// the relying party can trust the values -- it only means that the values
		/// have not been tampered with since the Provider sent the message.</para>
		/// </remarks>
		public T GetExtension<T>() where T : IOpenIdMessageExtension {
			throw new NotSupportedException(OpenIdStrings.NotSupportedByAuthenticationSnapshot);
		}

		/// <summary>
		/// Tries to get an OpenID extension that may be present in the response.
		/// </summary>
		/// <param name="extensionType">Type of the extension to look for in the response.</param>
		/// <returns>
		/// The extension, if it is found.  Null otherwise.
		/// </returns>
		/// <remarks>
		/// 	<para>Extensions are returned only if the Provider signed them.
		/// Relying parties that do not care if the values were modified in
		/// transit should use the <see cref="GetUntrustedExtension"/> method
		/// in order to allow the Provider to not sign the extension. </para>
		/// 	<para>Unsigned extensions are completely unreliable and should be
		/// used only to prefill user forms since the user or any other third
		/// party may have tampered with the data carried by the extension.</para>
		/// 	<para>Signed extensions are only reliable if the relying party
		/// trusts the OpenID Provider that signed them.  Signing does not mean
		/// the relying party can trust the values -- it only means that the values
		/// have not been tampered with since the Provider sent the message.</para>
		/// </remarks>
		public IOpenIdMessageExtension GetExtension(Type extensionType) {
			throw new NotSupportedException(OpenIdStrings.NotSupportedByAuthenticationSnapshot);
		}

		/// <summary>
		/// Tries to get an OpenID extension that may be present in the response, without
		/// requiring it to be signed by the Provider.
		/// </summary>
		/// <typeparam name="T">The type of extension to look for in the response message.</typeparam>
		/// <returns>
		/// The extension, if it is found.  Null otherwise.
		/// </returns>
		/// <remarks>
		/// 	<para>Extensions are returned whether they are signed or not.
		/// Use the <see cref="GetExtension&lt;T&gt;"/> method to retrieve
		/// extension responses only if they are signed by the Provider to
		/// protect against tampering. </para>
		/// 	<para>Unsigned extensions are completely unreliable and should be
		/// used only to prefill user forms since the user or any other third
		/// party may have tampered with the data carried by the extension.</para>
		/// 	<para>Signed extensions are only reliable if the relying party
		/// trusts the OpenID Provider that signed them.  Signing does not mean
		/// the relying party can trust the values -- it only means that the values
		/// have not been tampered with since the Provider sent the message.</para>
		/// </remarks>
		public T GetUntrustedExtension<T>() where T : IOpenIdMessageExtension {
			throw new NotSupportedException(OpenIdStrings.NotSupportedByAuthenticationSnapshot);
		}

		/// <summary>
		/// Tries to get an OpenID extension that may be present in the response.
		/// </summary>
		/// <param name="extensionType">Type of the extension to look for in the response.</param>
		/// <returns>
		/// The extension, if it is found.  Null otherwise.
		/// </returns>
		/// <remarks>
		/// 	<para>Extensions are returned whether they are signed or not.
		/// Use the <see cref="GetExtension"/> method to retrieve
		/// extension responses only if they are signed by the Provider to
		/// protect against tampering. </para>
		/// 	<para>Unsigned extensions are completely unreliable and should be
		/// used only to prefill user forms since the user or any other third
		/// party may have tampered with the data carried by the extension.</para>
		/// 	<para>Signed extensions are only reliable if the relying party
		/// trusts the OpenID Provider that signed them.  Signing does not mean
		/// the relying party can trust the values -- it only means that the values
		/// have not been tampered with since the Provider sent the message.</para>
		/// </remarks>
		public IOpenIdMessageExtension GetUntrustedExtension(Type extensionType) {
			throw new NotSupportedException(OpenIdStrings.NotSupportedByAuthenticationSnapshot);
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
			// Return a copy so that the caller cannot change the contents.
			return new Dictionary<string, string>(this.callbackArguments);
		}

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
		public IDictionary<string, string> GetUntrustedCallbackArguments() {
			// Return a copy so that the caller cannot change the contents.
			return new Dictionary<string, string>(this.untrustedCallbackArguments);
		}

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
			string value;
			this.callbackArguments.TryGetValue(key, out value);
			return value;
		}

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
		public string GetUntrustedCallbackArgument(string key) {
			string value;
			this.untrustedCallbackArguments.TryGetValue(key, out value);
			return value;
		}

		#endregion
	}
}
