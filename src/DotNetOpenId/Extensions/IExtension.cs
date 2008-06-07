using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.Extensions {
	/// <summary>
	/// The contract any OpenID extension for DotNetOpenId must implement.
	/// </summary>
	public interface IExtension {
		/// <summary>
		/// Gets the TypeURI the extension uses in the OpenID protocol and in XRDS advertisements.
		/// </summary>
		string TypeUri { get; }
		/// <summary>
		/// Additional TypeURIs that are supported by this extension, in preferred order.
		/// May be empty if none other than <see cref="TypeUri"/> is supported, but
		/// should not be null.
		/// </summary>
		/// <remarks>
		/// Useful for reading in messages with an older version of an extension.
		/// The value in the <see cref="TypeUri"/> property is always checked before
		/// trying this list.
		/// If you do support multiple versions of an extension using this method,
		/// consider adding a CreateResponse method to your request extension class
		/// so that the response can have the context it needs to remain compatible
		/// given the version of the extension in the request message.
		/// The <see cref="SimpleRegistration.ClaimsRequest.CreateResponse"/> for an example.
		/// </remarks>
		IEnumerable<string> AdditionalSupportedTypeUris { get; }
	}

	/// <summary>
	/// The contract an OpenID extension can implement for messages from relying party to provider
	/// to make handling extensions generally easier.  
	/// Extensions are not required to implement this interface, however.
	/// </summary>
	public interface IExtensionRequest : IExtension {
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
	public interface IExtensionResponse : IExtension {
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
