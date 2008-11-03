using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Web;
using DotNetOpenId.RelyingParty;

namespace DotNetOpenId.Interop {
	/// <summary>
	/// The COM interface describing the DotNetOpenId functionality available to
	/// COM client relying parties.
	/// </summary>
	[Guid("00462F34-21BE-456c-B986-B6DDE4DC5CA8")]
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
		/// <exception cref="OpenIdException">Thrown if no OpenID endpoint could be found.</exception>
		string CreateRequest(string userSuppliedIdentifier, string realm, string returnToUrl);

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

	/// <summary>
	/// Implementation of <see cref="IOpenIdRelyingParty"/>, providing a subset of the
	/// functionality available to .NET clients.
	/// </summary>
	[Guid("4D6FB236-1D66-4311-B761-972C12BB85E8")]
	[ProgId("DotNetOpenId.RelyingParty.OpenIdRelyingParty")]
	[ComVisible(true), Obsolete("This class acts as a COM Server and should not be called directly from .NET code.", true)]
	[ClassInterface(ClassInterfaceType.None)]
	[ComSourceInterfaces(typeof(IOpenIdRelyingParty))]
	public class OpenIdRelyingPartyShim : IOpenIdRelyingParty {
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
		/// <exception cref="OpenIdException">Thrown if no OpenID endpoint could be found.</exception>
		[SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings")]
		public string CreateRequest(string userSuppliedIdentifier, string realm, string returnToUrl) {
			OpenIdRelyingParty rp = new OpenIdRelyingParty(null, null, null);
			Response response = (Response)rp.CreateRequest(userSuppliedIdentifier, realm, new Uri(returnToUrl)).RedirectingResponse;
			return response.IndirectMessageAsRequestUri.AbsoluteUri;
		}

		/// <summary>
		/// Gets the result of a user agent's visit to his OpenId provider in an
		/// authentication attempt.  Null if no response is available.
		/// </summary>
		/// <param name="url">The incoming request URL .</param>
		/// <param name="form">The form data that may have been included in the case of a POST request.</param>
		/// <returns>The Provider's response to a previous authentication request, or null if no response is present.</returns>
		public AuthenticationResponseShim ProcessAuthentication(string url, string form) {
			Uri uri = new Uri(url);
			OpenIdRelyingParty rp = new OpenIdRelyingParty(null, uri, HttpUtility.ParseQueryString(uri.Query));
			if (rp.Response != null) {
				return new AuthenticationResponseShim(rp.Response);
			}

			return null;
		}
	}
}
