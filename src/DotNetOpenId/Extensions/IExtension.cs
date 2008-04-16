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
		/// Adds the properties of this Attribute Exchange request to an outgoing
		/// OpenID authentication request.
		/// </summary>
		void AddToRequest(RelyingParty.IAuthenticationRequest authenticationRequest);
		/// <summary>
		/// Reads the extension information on an authentication request to the provider.
		/// </summary>
		/// <returns>True if the extension found any of its parameters in the request.</returns>
		bool ReadFromRequest(Provider.IRequest request);
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
		/// Adds the values of this struct to an authentication response being prepared
		/// by an OpenID Provider.
		/// </summary>
		void AddToResponse(Provider.IRequest authenticationRequest);
		/// <summary>
		/// Reads a Provider's response for extension values.
		/// </summary>
		/// <returns>True if the extension found any of its parameters in the response.</returns>
		bool ReadFromResponse(RelyingParty.IAuthenticationResponse response);
	}
}
