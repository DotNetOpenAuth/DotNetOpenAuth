//-----------------------------------------------------------------------
// <copyright file="IExtension.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions {
	using System;
	using System.Collections.Generic;
	using System.Text;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// The contract an OpenID extension can implement for messages from relying party to provider
	/// to make handling extensions generally easier.  
	/// Extensions are not required to implement this interface, however.
	/// </summary>
	internal interface IExtensionRequest : IOpenIdProtocolMessageExtension {
		/// <summary>
		/// Returns the fields this extension should add to an authentication request.
		/// </summary>
		IDictionary<string, string> Serialize(RelyingParty.IAuthenticationRequest authenticationRequest);

		/// <summary>
		/// Reads the extension information on an authentication request to the provider.
		/// </summary>
		/// <param name="fields">The fields belonging to the extension.</param>
		/// <param name="request">The incoming OpenID request carrying the extension.</param>
		/// <param name="typeUri">The actual extension TypeUri that was recognized in the message.</param>
		/// <returns>
		/// True if the extension found a valid set of recognized parameters in the request, 
		/// false otherwise.
		/// </returns>
		bool Deserialize(IDictionary<string, string> fields, Provider.IRequest request, string typeUri);
	}

	/// <summary>
	/// The contract an OpenID extension can implement for messages from provider to relying party
	/// to make handling extensions generally easier.  
	/// Extensions are not required to implement this interface, however.
	/// </summary>
	internal interface IExtensionResponse : IOpenIdProtocolMessageExtension {
		/// <summary>
		/// Returns the fields this extension should add to an authentication response.
		/// </summary>
		IDictionary<string, string> Serialize(Provider.IRequest authenticationRequest);

		/// <summary>
		/// Reads a Provider's response for extension values.
		/// </summary>
		/// <param name="fields">The fields belonging to the extension.</param>
		/// <param name="response">The incoming OpenID response carrying the extension.</param>
		/// <param name="typeUri">The actual extension TypeUri that was recognized in the message.</param>
		/// <returns>
		/// True if the extension found a valid set of recognized parameters in the response, 
		/// false otherwise.
		/// </returns>
		bool Deserialize(IDictionary<string, string> fields, RelyingParty.IAuthenticationResponse response, string typeUri);
	}
}
