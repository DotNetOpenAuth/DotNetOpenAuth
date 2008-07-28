using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using DotNetOpenId.Extensions;

namespace DotNetOpenId.RelyingParty {
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
		/// Tries to get an OpenID extension that may be present in the response.
		/// </summary>
		/// <returns>The extension, if it is found.  Null otherwise.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		T GetExtension<T>() where T : IExtensionResponse, new();
		/// <summary>
		/// Tries to get an OpenID extension that may be present in the response.
		/// </summary>
		/// <returns>The extension, if it is found.  Null otherwise.</returns>
		IExtensionResponse GetExtension(Type extensionType);
		/// <summary>
		/// An Identifier that the end user claims to own.  For use with user database storage and lookup.
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
		/// The detailed success or failure status of the authentication attempt.
		/// </summary>
		AuthenticationStatus Status { get; }
		/// <summary>
		/// Details regarding a failed authentication attempt, if available.
		/// This will be set if and only if <see cref="Status"/> is <see cref="AuthenticationStatus.Failed"/>.
		/// </summary>
		Exception Exception { get; }
	}
}
