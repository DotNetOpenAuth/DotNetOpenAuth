using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.Consumer {
	public interface IAuthenticationResponse {
		/// <summary>
		/// Gets the key/value pairs of a provider's response for a given OpenID extension.
		/// </summary>
		/// <param name="extensionPrefix">
		/// The prefix used by the extension, not including the 'openid.' start.
		/// For example, simple registration key/values can be retrieved by passing 
		/// 'sreg' as this argument.
		/// </param>
		/// <returns>
		/// Returns key/value pairs where the keys do not include the 
		/// 'openid.' or the <paramref name="extensionPrefix"/>.
		/// </returns>
		IDictionary<string, string> GetExtensionArguments(string extensionPrefix);
		Uri IdentityUrl { get; }
		/// <summary>
		/// The detailed success or failure status of the authentication attempt.
		/// </summary>
		AuthenticationStatus Status { get; }
	}
}
