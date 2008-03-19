using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.RelyingParty {
	public interface IAuthenticationResponse {
		/// <summary>
		/// Gets the key/value pairs of a provider's response for a given OpenID extension.
		/// </summary>
		/// <param name="extensionTypeUri">
		/// The Type URI of the OpenID extension whose arguments are being sought.
		/// </param>
		/// <returns>
		/// Returns key/value pairs where the keys do not include the 
		/// 'openid.' or the <paramref name="extensionPrefix"/>.
		/// </returns>
		IDictionary<string, string> GetExtensionArguments(string extensionTypeUri);
		/// <summary>
		/// An Identifier that the end user claims to own.
		/// </summary>
		Identifier ClaimedIdentifier { get; }
		/// <summary>
		/// The detailed success or failure status of the authentication attempt.
		/// </summary>
		AuthenticationStatus Status { get; }
	}
}
