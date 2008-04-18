using System;
using System.Collections.Generic;
using System.Text;
using DotNetOpenId.Extensions;

namespace DotNetOpenId.RelyingParty {
	/// <summary>
	/// An instance of this interface represents an identity assertion 
	/// from an OpenID Provider.  It may be in response to an authentication 
	/// request previously put to it by a Relying Party site or it may be an
	/// unsolicited assertion.
	/// </summary>
	/// <remarks>
	/// Relying party web sites should handle both solicited and unsolicited 
	/// assertions.  This interface does not offer a way to discern between
	/// solicited and unsolicited assertions as they should be treated equally.
	/// </remarks>
	public interface IAuthenticationResponse {
		/// <summary>
		/// Gets the key/value pairs of a provider's response for a given OpenID extension.
		/// </summary>
		/// <param name="extensionTypeUri">
		/// The Type URI of the OpenID extension whose arguments are being sought.
		/// </param>
		/// <returns>
		/// Returns key/value pairs found for the given extension.
		/// </returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#")]
		[Obsolete("Use GetExtension instead.")]
		IDictionary<string, string> GetExtensionArguments(string extensionTypeUri);
		/// <summary>
		/// Tries to get an OpenID extension that may be present in the response.
		/// </summary>
		/// <param name="extensionTypeUri">The type URI the extension is known by.</param>
		/// <returns>The extension, if it is found.  Null otherwise.</returns>
		T GetExtension<T>() where T : IExtensionResponse, new();
		/// <summary>
		/// An Identifier that the end user claims to own.
		/// </summary>
		Identifier ClaimedIdentifier { get; }
		/// <summary>
		/// The detailed success or failure status of the authentication attempt.
		/// </summary>
		AuthenticationStatus Status { get; }
		/// <summary>
		/// Details regarding a failed authentication attempt, if available.
		/// This will be set if and only if <see cref="Status"/> is <see cref="AuthenticationStatus.Failed"/>.
		/// </summary>
		Exception Exception { get; }
	}
}
