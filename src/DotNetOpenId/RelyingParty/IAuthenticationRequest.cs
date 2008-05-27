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
		/// resolved to.
		/// </summary>
		Identifier ClaimedIdentifier { get; }
		/// <summary>
		/// The detected version of OpenID implemented by the Provider.
		/// </summary>
		Version ProviderVersion { get; }
	}
}
