using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.Consumer {
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
		/// Adds extra query parameters to the request directed at the OpenID provider.
		/// </summary>
		/// <param name="extensionPrefix">
		/// The extension-specific prefix associated with these arguments.
		/// This should not include the 'openid.' part of the prefix.
		/// For example, the extension field openid.sreg.fullname would receive
		/// 'sreg' for this value.
		/// </param>
		/// <param name="arguments">
		/// The key/value pairs of parameters and values to pass to the provider.
		/// The keys should NOT have the 'openid.ext.' prefix.
		/// </param>
		void AddExtensionArguments(string extensionPrefix, IDictionary<string, string> arguments);
		/// <summary>
		/// Redirects the user agent to the provider for authentication.
		/// </summary>
		/// <remarks>
		/// This method requires an ASP.NET HttpContext.
		/// </remarks>
		void RedirectToProvider();
		/// <summary>
		/// Redirects the user agent to the provider for authentication.
		/// </summary>
		/// <param name="endResponse">
		/// Whether execution of this response should cease after this call.
		/// </param>
		/// <remarks>
		/// This method requires an ASP.NET HttpContext.
		/// </remarks>
		void RedirectToProvider(bool endResponse);
		/// <summary>
		/// Gets/sets the mode the Provider should use during authentication.
		/// </summary>
		AuthenticationRequestMode Mode { get; set; }
		/// <summary>
		/// Gets the URL the user agent should be redirected to to begin the 
		/// OpenID authentication process.
		/// </summary>
		Uri RedirectToProviderUrl { get; }
		/// <summary>
		/// Gets the URL that the user agent will return to after authentication
		/// completes or fails at the Provider.
		/// </summary>
		Uri ReturnToUrl { get; }
		/// <summary>
		/// Gets the URL that identifies this consumer web application that
		/// the Provider will display to the end user.
		/// </summary>
		TrustRoot TrustRoot { get; }
	}
}
