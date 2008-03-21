using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.Provider {
	/// <summary>
	/// Represents an incoming OpenId authentication request.
	/// </summary>
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
		/// Adds extension arguments to the response to send to the client.
		/// </summary>
		/// <param name="extensionTypeUri">The extension's Type URI.</param>
		/// <param name="arguments">
		/// The key/value pairs for this extension to add to the response.
		/// The keys should not include any 'openid.&lt;namespace&gt;.' prefix.
		/// </param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#")]
		void AddExtensionArguments(string extensionTypeUri, IDictionary<string, string> arguments);
		/// <summary>
		/// Gets the extension arguments sent from the relying party.
		/// </summary>
		/// <param name="extensionTypeUri">The extension's Type URI.</param>
		/// <returns>A dictionary where the keys are the extension's
		/// keys without the 'openid.&lt;namespace&gt;.' prefix.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#")]
		IDictionary<string, string> GetExtensionArguments(string extensionTypeUri);
	}
}
