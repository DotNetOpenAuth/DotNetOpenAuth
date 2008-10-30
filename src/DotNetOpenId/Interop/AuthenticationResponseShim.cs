using System;
using System.Runtime.InteropServices;
using System.Web;
using DotNetOpenId.RelyingParty;

namespace DotNetOpenId.Interop {
	/// <summary>
	/// The COM type used to provide details of an authentication result to a relying party COM client.
	/// </summary>
	[ComVisible(true)]
	public class AuthenticationResponseShim {
		private readonly IAuthenticationResponse response;

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthenticationResponseShim"/> class.
		/// </summary>
		/// <param name="response">The response.</param>
		internal AuthenticationResponseShim(IAuthenticationResponse response) {
			if (response == null) throw new ArgumentNullException("response");
			this.response = response;
		}

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
		public string ClaimedIdentifier {
			get { return this.response.ClaimedIdentifier; }
		}

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
		public string FriendlyIdentifierForDisplay {
			get { return this.response.FriendlyIdentifierForDisplay; }
		}

		/// <summary>
		/// A value indicating whether the authentication attempt succeeded.
		/// </summary>
		public bool Successful {
			get { return this.response.Status == AuthenticationStatus.Authenticated; }
		}

		/// <summary>
		/// Details regarding a failed authentication attempt, if available.
		/// </summary>
		public string ExceptionMessage {
			get { return this.response.Exception != null ? this.response.Exception.Message : null; }
		}
	}
}
