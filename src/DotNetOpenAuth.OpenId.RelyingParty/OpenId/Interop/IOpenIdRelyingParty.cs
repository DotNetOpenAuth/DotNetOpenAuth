//-----------------------------------------------------------------------
// <copyright file="IOpenIdRelyingParty.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Interop {
	using System.Diagnostics.CodeAnalysis;
	using System.Runtime.InteropServices;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// The COM interface describing the DotNetOpenAuth functionality available to
	/// COM client OpenID relying parties.
	/// </summary>
	[Guid("56BD3DB0-EE0D-4191-ADFC-1F3705CD2636")]
	[InterfaceType(ComInterfaceType.InterfaceIsDual)]
	public interface IOpenIdRelyingParty {
		/// <summary>
		/// Creates an authentication request to verify that a user controls
		/// some given Identifier.
		/// </summary>
		/// <param name="userSuppliedIdentifier">
		/// The Identifier supplied by the user.  This may be a URL, an XRI or i-name.
		/// </param>
		/// <param name="realm">
		/// The shorest URL that describes this relying party web site's address.
		/// For example, if your login page is found at https://www.example.com/login.aspx,
		/// your realm would typically be https://www.example.com/.
		/// </param>
		/// <param name="returnToUrl">
		/// The URL of the login page, or the page prepared to receive authentication 
		/// responses from the OpenID Provider.
		/// </param>
		/// <returns>
		/// An authentication request object that describes the HTTP response to
		/// send to the user agent to initiate the authentication.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown if no OpenID endpoint could be found.</exception>
		string CreateRequest(string userSuppliedIdentifier, string realm, string returnToUrl);

		/// <summary>
		/// Creates an authentication request to verify that a user controls
		/// some given Identifier.
		/// </summary>
		/// <param name="userSuppliedIdentifier">The Identifier supplied by the user.  This may be a URL, an XRI or i-name.</param>
		/// <param name="realm">The shorest URL that describes this relying party web site's address.
		/// For example, if your login page is found at https://www.example.com/login.aspx,
		/// your realm would typically be https://www.example.com/.</param>
		/// <param name="returnToUrl">The URL of the login page, or the page prepared to receive authentication
		/// responses from the OpenID Provider.</param>
		/// <param name="optionalSreg">A comma-delimited list of simple registration fields to request as optional.</param>
		/// <param name="requiredSreg">A comma-delimited list of simple registration fields to request as required.</param>
		/// <returns>
		/// An authentication request object that describes the HTTP response to
		/// send to the user agent to initiate the authentication.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown if no OpenID endpoint could be found.</exception>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sreg", Justification = "Accepted acronym")]
		string CreateRequestWithSimpleRegistration(string userSuppliedIdentifier, string realm, string returnToUrl, string optionalSreg, string requiredSreg);

		/// <summary>
		/// Gets the result of a user agent's visit to his OpenId provider in an
		/// authentication attempt.  Null if no response is available.
		/// </summary>
		/// <param name="url">The incoming request URL .</param>
		/// <param name="form">The form data that may have been included in the case of a POST request.</param>
		/// <returns>The Provider's response to a previous authentication request, or null if no response is present.</returns>
#pragma warning disable 0618 // we're using the COM type properly
		AuthenticationResponseShim ProcessAuthentication(string url, string form);
#pragma warning restore 0618
	}
}