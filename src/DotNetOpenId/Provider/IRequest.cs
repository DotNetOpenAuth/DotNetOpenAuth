using System;
using System.Collections.Generic;
using System.Text;
using DotNetOpenId.Extensions;

namespace DotNetOpenId.Provider {
	/// <summary>
	/// Represents an incoming OpenId authentication request.
	/// </summary>
	/// <remarks>
	/// Requests may be infrastructural to OpenID and allow auto-responses, or they may
	/// be authentication requests where the Provider site has to make decisions based
	/// on its own user database and policies.
	/// </remarks>
	public interface IRequest {
		/// <summary>
		/// Gets the version of OpenID being used by the relying party that sent the request.
		/// </summary>
		ProtocolVersion RelyingPartyVersion { get; }
		/// <summary>
		/// Returns true if the Response is ready to be sent to the user agent.
		/// Returns false if there are properties that must be set on this
		/// request instance before the response can be sent.
		/// </summary>
		bool IsResponseReady { get; }
		/// <summary>
		/// Gets the response to send to the user agent.
		/// </summary>
		IResponse Response { get; }
		/// <summary>
		/// Adds an extension to the response to send to the relying party.
		/// </summary>
		void AddResponseExtension(IExtensionResponse extension);
		/// <summary>
		/// Gets an extension sent from the relying party.
		/// </summary>
		/// <typeparam name="T">The type of the extension.</typeparam>
		/// <returns>An instance of the extension initialized with values passed in with the request.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		T GetExtension<T>() where T : IExtensionRequest, new();
		/// <summary>
		/// Gets an extension sent from the relying party.
		/// </summary>
		/// <param name="extensionType">The type of the extension.</param>
		/// <returns>An instance of the extension initialized with values passed in with the request.</returns>
		IExtensionRequest GetExtension(Type extensionType);
	}
}
