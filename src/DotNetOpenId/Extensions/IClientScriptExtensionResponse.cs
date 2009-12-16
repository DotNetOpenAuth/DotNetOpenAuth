using System.Collections.Generic;
using DotNetOpenId.RelyingParty;

namespace DotNetOpenId.Extensions {
	/// <summary>
	/// An interface that OpenID extensions can implement to allow authentication response
	/// messages with included extensions to be processed by Javascript on the user agent.
	/// </summary>
	public interface IClientScriptExtensionResponse : IExtension {
		/// <summary>
		/// Reads the extension information on an authentication response from the provider.
		/// </summary>
		/// <param name="fields">The fields belonging to the extension.</param>
		/// <param name="response">The incoming OpenID response carrying the extension.</param>
		/// <param name="typeUri">The actual extension TypeUri that was recognized in the message.</param>
		/// <returns>
		/// A Javascript snippet that when executed on the user agent returns an object with
		/// the information deserialized from the extension response.
		/// </returns>
		/// <remarks>
		/// This method is called <b>before</b> the signature on the assertion response has been
		/// verified.  Therefore all information in these fields should be assumed unreliable
		/// and potentially falsified.
		/// </remarks>
		string InitializeJavaScriptData(IDictionary<string, string> fields, IAuthenticationResponse response, string typeUri);
	}
}
