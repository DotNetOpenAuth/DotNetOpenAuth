//-----------------------------------------------------------------------
// <copyright file="IProviderEndpoint.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.ObjectModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;

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
		/// Gets the detected version of OpenID implemented by the Provider.
		/// </summary>
		Version Version { get; }

		/// <summary>
		/// Gets the URL that the OpenID Provider receives authentication requests at.
		/// </summary>
		/// <value>
		/// This value MUST be an absolute HTTP or HTTPS URL.
		/// </value>
		Uri Uri { get; }

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
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "No parameter at all.")]
		[Obsolete("Use IAuthenticationRequest.DiscoveryResult.IsExtensionSupported instead.")]
		bool IsExtensionSupported<T>() where T : IOpenIdMessageExtension, new();

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
		[Obsolete("Use IAuthenticationRequest.DiscoveryResult.IsExtensionSupported instead.")]
		bool IsExtensionSupported(Type extensionType);
	}
}
