//-----------------------------------------------------------------------
// <copyright file="AuthenticationResponseShim.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Interop {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Runtime.InteropServices;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.RelyingParty;

	/// <summary>
	/// The COM type used to provide details of an authentication result to a relying party COM client.
	/// </summary>
	[SuppressMessage("Microsoft.Interoperability", "CA1409:ComVisibleTypesShouldBeCreatable", Justification = "It's only creatable on the inside.  It must be ComVisible for ASP to see it.")]
	[ComVisible(true), Obsolete("This class acts as a COM Server and should not be called directly from .NET code.")]
	public sealed class AuthenticationResponseShim {
		/// <summary>
		/// The response read in by the Relying Party.
		/// </summary>
		private readonly IAuthenticationResponse response;

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthenticationResponseShim"/> class.
		/// </summary>
		/// <param name="response">The response.</param>
		internal AuthenticationResponseShim(IAuthenticationResponse response) {
			Requires.NotNull(response, "response");

			this.response = response;
			var claimsResponse = this.response.GetExtension<ClaimsResponse>();
			if (claimsResponse != null) {
				this.ClaimsResponse = new ClaimsResponseShim(claimsResponse);
			}
		}

		/// <summary>
		/// Gets an Identifier that the end user claims to own.  For use with user database storage and lookup.
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
		/// Gets the provider endpoint that sent the assertion.
		/// </summary>
		public string ProviderEndpoint {
			get { return this.response.Provider != null ? this.response.Provider.Uri.AbsoluteUri : null;  }
		}

		/// <summary>
		/// Gets a value indicating whether the authentication attempt succeeded.
		/// </summary>
		public bool Successful {
			get { return this.response.Status == AuthenticationStatus.Authenticated; }
		}

		/// <summary>
		/// Gets the Simple Registration response.
		/// </summary>
		public ClaimsResponseShim ClaimsResponse { get; private set; }

		/// <summary>
		/// Gets details regarding a failed authentication attempt, if available.
		/// </summary>
		public string ExceptionMessage {
			get { return this.response.Exception != null ? this.response.Exception.Message : null; }
		}
	}
}
