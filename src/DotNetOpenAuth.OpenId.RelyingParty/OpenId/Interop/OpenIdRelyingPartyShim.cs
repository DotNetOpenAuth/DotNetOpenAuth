//-----------------------------------------------------------------------
// <copyright file="OpenIdRelyingPartyShim.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Interop {
	using System;
	using System.Collections.Specialized;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.RelyingParty;

	/// <summary>
	/// Implementation of <see cref="IOpenIdRelyingParty"/>, providing a subset of the
	/// functionality available to .NET clients.
	/// </summary>
	[Guid("8F97A798-B4C5-4da5-9727-EE7DD96A8CD9")]
	[ProgId("DotNetOpenAuth.OpenId.RelyingParty.OpenIdRelyingParty")]
	[ComVisible(true), Obsolete("This class acts as a COM Server and should not be called directly from .NET code.", true)]
	[ClassInterface(ClassInterfaceType.None)]
	public sealed class OpenIdRelyingPartyShim : IOpenIdRelyingParty {
		/// <summary>
		/// The OpenIdRelyingParty instance to use for requests.
		/// </summary>
		private static OpenIdRelyingParty relyingParty;

		/// <summary>
		/// Initializes static members of the <see cref="OpenIdRelyingPartyShim"/> class.
		/// </summary>
		static OpenIdRelyingPartyShim() {
			relyingParty = new OpenIdRelyingParty(null);
			relyingParty.Behaviors.Add(new RelyingParty.Behaviors.AXFetchAsSregTransform());
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdRelyingPartyShim"/> class.
		/// </summary>
		public OpenIdRelyingPartyShim() {
			Reporting.RecordFeatureUse(this);
		}

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
		[SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "COM requires primitive types")]
		public string CreateRequest(string userSuppliedIdentifier, string realm, string returnToUrl) {
			var request = relyingParty.CreateRequest(userSuppliedIdentifier, realm, new Uri(returnToUrl));
			return request.RedirectingResponse.GetDirectUriRequest(relyingParty.Channel).AbsoluteUri;
		}

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
		[SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "COM requires primitive types")]
		public string CreateRequestWithSimpleRegistration(string userSuppliedIdentifier, string realm, string returnToUrl, string optionalSreg, string requiredSreg) {
			var request = relyingParty.CreateRequest(userSuppliedIdentifier, realm, new Uri(returnToUrl));

			ClaimsRequest sreg = new ClaimsRequest();
			if (!string.IsNullOrEmpty(optionalSreg)) {
				sreg.SetProfileRequestFromList(optionalSreg.Split(','), DemandLevel.Request);
			}
			if (!string.IsNullOrEmpty(requiredSreg)) {
				sreg.SetProfileRequestFromList(requiredSreg.Split(','), DemandLevel.Require);
			}
			request.AddExtension(sreg);
			return request.RedirectingResponse.GetDirectUriRequest(relyingParty.Channel).AbsoluteUri;
		}

		/// <summary>
		/// Gets the result of a user agent's visit to his OpenId provider in an
		/// authentication attempt.  Null if no response is available.
		/// </summary>
		/// <param name="url">The incoming request URL.</param>
		/// <param name="form">The form data that may have been included in the case of a POST request.</param>
		/// <returns>The Provider's response to a previous authentication request, or null if no response is present.</returns>
		public AuthenticationResponseShim ProcessAuthentication(string url, string form) {
			string method = "GET";
			NameValueCollection formMap = null;
			if (!string.IsNullOrEmpty(form)) {
				method = "POST";
				formMap = HttpUtility.ParseQueryString(form);
			}

			HttpRequestBase requestInfo = new HttpRequestInfo(method, new Uri(url), form: formMap);
			var response = relyingParty.GetResponse(requestInfo);
			if (response != null) {
				return new AuthenticationResponseShim(response);
			}

			return null;
		}
	}
}
