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
		/// Tries to get an OpenID extension that may be present in the response.
		/// </summary>
		/// <returns>The extension, if it is found.  Null otherwise.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		T GetExtension<T>() where T : IExtensionResponse, new();
		/// <summary>
		/// Tries to get an OpenID extension that may be present in the response.
		/// </summary>
		/// <returns>The extension, if it is found.  Null otherwise.</returns>
		IExtensionResponse GetExtension(Type extensionType);
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
