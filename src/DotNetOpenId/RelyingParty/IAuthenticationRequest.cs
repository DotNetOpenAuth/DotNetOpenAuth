using System;
using System.Collections.Generic;
using System.Text;
using DotNetOpenId.Extensions;

namespace DotNetOpenId.RelyingParty {
	/// <summary>
	/// Instances of this interface represent relying party authentication 
	/// requests that may be queried/modified in specific ways before being
	/// routed to the OpenID Provider.
	/// </summary>
	public interface IAuthenticationRequest {
		/// <summary>
		/// Adds given key/value pairs to the query that the provider will use in
		/// the request to return to the consumer web site.
		/// </summary>
		void AddCallbackArguments(IDictionary<string, string> arguments);
		/// <summary>
		/// Adds a given key/value pair to the query that the provider will use in
		/// the request to return to the consumer web site.
		/// </summary>
		void AddCallbackArguments(string key, string value);
		/// <summary>
		/// Adds an OpenID extension to the request directed at the OpenID provider.
		/// </summary>
		void AddExtension(IExtensionRequest extension);
		/// <summary>
		/// Checks whether the OpenId Identifier claims support for a given extension.
		/// </summary>
		/// <typeparam name="T">The extension whose support is being queried.</typeparam>
		/// <returns>True if support for the extension is advertised.  False otherwise.</returns>
		/// <remarks>
		/// Note that a true or false return value is no guarantee of a Provider's 
		/// support for or lack of support for an extension.  The return value is
		/// determined by how the authenticating user filled out his/her XRDS document only.
		/// The only way to be sure of support for a given extension is to include
		/// the extension in the request and see if a response comes back for that extension.
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		bool IsExtensionAdvertisedAsSupported<T>() where T : Extensions.IExtension, new();
		/// <summary>
		/// Checks whether the OpenId Identifier claims support for a given extension.
		/// </summary>
		/// <param name="extensionType">The extension whose support is being queried.</param>
		/// <returns>True if support for the extension is advertised.  False otherwise.</returns>
		/// <remarks>
		/// Note that a true or false return value is no guarantee of a Provider's 
		/// support for or lack of support for an extension.  The return value is
		/// determined by how the authenticating user filled out his/her XRDS document only.
		/// The only way to be sure of support for a given extension is to include
		/// the extension in the request and see if a response comes back for that extension.
		/// </remarks>
		bool IsExtensionAdvertisedAsSupported(Type extensionType);
		/// <summary>
		/// Redirects the user agent to the provider for authentication.
		/// Execution of the current page terminates after this call.
		/// </summary>
		/// <remarks>
		/// This method requires an ASP.NET HttpContext.
		/// </remarks>
		void RedirectToProvider();
		/// <summary>
		/// Gets/sets the mode the Provider should use during authentication.
		/// </summary>
		AuthenticationRequestMode Mode { get; set; }
		/// <summary>
		/// Gets the HTTP response the relying party should send to the user agent 
		/// to redirect it to the OpenID Provider to start the OpenID authentication process.
		/// </summary>
		IResponse RedirectingResponse { get; }
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
		/// Gets whether the authenticating user has chosen to let the Provider
		/// determine and send the ClaimedIdentifier after authentication.
		/// </summary>
		bool IsDirectedIdentity { get; }
		/// <summary>
		/// The detected version of OpenID implemented by the Provider.
		/// </summary>
		Version ProviderVersion { get; }
	}
}
