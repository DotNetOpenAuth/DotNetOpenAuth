using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId.RelyingParty {
	/// <summary>
	/// Information published about an OpenId Provider by the
	/// OpenId discovery documents found at a user's Claimed Identifier.
	/// </summary>
	/// <remarks>
	/// Because information provided by this interface is suppplied by a 
	/// user's individually published documents, it may be incomplete or inaccurate.
	/// </remarks>
	public interface IProviderEndpoint {
		/// <summary>
		/// Checks whether the OpenId Identifier claims support for a given extension.
		/// </summary>
		/// <typeparam name="T">The extension whose support is being queried.</typeparam>
		/// <returns>True if support for the extension is advertised.  False otherwise.</returns>
		/// <remarks>
		/// Note that a true or false return value is no guarantee of a Provider's 
		/// support for or lack of support for an extension.  The return value is
		/// determined by how the authenticating user filled out his/her XRDS document only.
		/// The only way to be sure of support for a given extension is to include
		/// the extension in the request and see if a response comes back for that extension.
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		bool IsExtensionSupported<T>() where T : Extensions.IExtension, new();
		/// <summary>
		/// Checks whether the OpenId Identifier claims support for a given extension.
		/// </summary>
		/// <param name="extensionType">The extension whose support is being queried.</param>
		/// <returns>True if support for the extension is advertised.  False otherwise.</returns>
		/// <remarks>
		/// Note that a true or false return value is no guarantee of a Provider's 
		/// support for or lack of support for an extension.  The return value is
		/// determined by how the authenticating user filled out his/her XRDS document only.
		/// The only way to be sure of support for a given extension is to include
		/// the extension in the request and see if a response comes back for that extension.
		/// </remarks>
		bool IsExtensionSupported(Type extensionType);
		/// <summary>
		/// The detected version of OpenID implemented by the Provider.
		/// </summary>
		Version Version { get; }

	}
}
