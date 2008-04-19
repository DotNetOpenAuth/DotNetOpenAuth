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
		/// Adds extension arguments to the response to send to the relying party.
		/// </summary>
		/// <param name="extensionTypeUri">The extension's Type URI.</param>
		/// <param name="arguments">
		/// The key/value pairs for this extension to add to the response.
		/// The keys should not include any 'openid.&lt;namespace&gt;.' prefix.
		/// </param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#")]
		[Obsolete("Use AddExtension instead.")]
		void AddResponseExtensionArguments(string extensionTypeUri, IDictionary<string, string> arguments);
		/// <summary>
		/// Adds an extension to the response to send to the relying party.
		/// </summary>
		void AddResponseExtension(IExtensionResponse extension);
		/// <summary>
		/// Gets the extension arguments sent from the relying party.
		/// </summary>
		/// <param name="extensionTypeUri">The extension's Type URI.</param>
		/// <returns>A dictionary where the keys are the extension's
		/// keys without the 'openid.&lt;namespace&gt;.' prefix.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#")]
		[Obsolete("Use GetExtension instead.")]
		IDictionary<string, string> GetExtensionArguments(string extensionTypeUri);
		/// <summary>
		/// Gets an extension sent from the relying party.
		/// </summary>
		/// <typeparam name="T">The type of the extension.</typeparam>
		T GetExtension<T>() where T : IExtensionRequest, new();
	}
}
