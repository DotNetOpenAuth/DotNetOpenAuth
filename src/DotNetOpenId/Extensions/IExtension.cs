using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.Extensions {
	/// <summary>
	/// The contract an OpenID extension can implement for messages from relying party to provider
	/// to make handling extensions generally easier.  
	/// Extensions are not required to implement this interface, however.
	/// </summary>
	public interface IExtensionRequest {
		/// <summary>
		/// Gets the TypeURI the extension uses in the OpenID protocol and in XRDS advertisements.
		/// </summary>
		string TypeUri { get; }
		/// <summary>
		/// Returns the fields this extension should add to an authentication request.
		/// </summary>
		IDictionary<string, string> GetFields(RelyingParty.IAuthenticationRequest authenticationRequest);
		/// <summary>
		/// Reads the extension information on an authentication request to the provider.
		/// </summary>
		/// <returns>True if the extension found any of its parameters in the request, false otherwise.</returns>
		bool SetFields(IDictionary<string, string> fields, Provider.IRequest request);
	}

	/// <summary>
	/// The contract an OpenID extension can implement for messages from provider to relying party
	/// to make handling extensions generally easier.  
	/// Extensions are not required to implement this interface, however.
	/// </summary>
	public interface IExtensionResponse {
		/// <summary>
		/// Gets the TypeURI the extension uses in the OpenID protocol and in XRDS advertisements.
		/// </summary>
		string TypeUri { get; }
		/// <summary>
		/// Returns the fields this extension should add to an authentication response.
		/// </summary>
		Dictionary<string, string> GetFields(Provider.IRequest authenticationRequest);
		/// <summary>
		/// Reads a Provider's response for extension values.
		/// </summary>
		/// <returns>True if the extension found any of its parameters in the response.</returns>
		bool SetFields(IDictionary<string, string> fields, RelyingParty.IAuthenticationResponse response);
	}
}
