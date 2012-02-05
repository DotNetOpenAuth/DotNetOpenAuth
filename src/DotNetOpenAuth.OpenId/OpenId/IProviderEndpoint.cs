//-----------------------------------------------------------------------
// <copyright file="IProviderEndpoint.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.ObjectModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
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
	[ContractClass(typeof(IProviderEndpointContract))]
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

	/// <summary>
	/// Code contract for the <see cref="IProviderEndpoint"/> type.
	/// </summary>
	[ContractClassFor(typeof(IProviderEndpoint))]
	internal abstract class IProviderEndpointContract : IProviderEndpoint {
		/// <summary>
		/// Prevents a default instance of the <see cref="IProviderEndpointContract"/> class from being created.
		/// </summary>
		private IProviderEndpointContract() {
		}

		#region IProviderEndpoint Members

		/// <summary>
		/// Gets the detected version of OpenID implemented by the Provider.
		/// </summary>
		Version IProviderEndpoint.Version {
			get {
				Contract.Ensures(Contract.Result<Version>() != null);
				throw new System.NotImplementedException();
			}
		}

		/// <summary>
		/// Gets the URL that the OpenID Provider receives authentication requests at.
		/// </summary>
		Uri IProviderEndpoint.Uri {
			get {
				Contract.Ensures(Contract.Result<Uri>() != null);
				throw new System.NotImplementedException();
			}
		}

		/// <summary>
		/// Checks whether the OpenId Identifier claims support for a given extension.
		/// </summary>
		/// <typeparam name="T">The extension whose support is being queried.</typeparam>
		/// <returns>
		/// True if support for the extension is advertised.  False otherwise.
		/// </returns>
		/// <remarks>
		/// Note that a true or false return value is no guarantee of a Provider's
		/// support for or lack of support for an extension.  The return value is
		/// determined by how the authenticating user filled out his/her XRDS document only.
		/// The only way to be sure of support for a given extension is to include
		/// the extension in the request and see if a response comes back for that extension.
		/// </remarks>
		bool IProviderEndpoint.IsExtensionSupported<T>() {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Checks whether the OpenId Identifier claims support for a given extension.
		/// </summary>
		/// <param name="extensionType">The extension whose support is being queried.</param>
		/// <returns>
		/// True if support for the extension is advertised.  False otherwise.
		/// </returns>
		/// <remarks>
		/// Note that a true or false return value is no guarantee of a Provider's
		/// support for or lack of support for an extension.  The return value is
		/// determined by how the authenticating user filled out his/her XRDS document only.
		/// The only way to be sure of support for a given extension is to include
		/// the extension in the request and see if a response comes back for that extension.
		/// </remarks>
		bool IProviderEndpoint.IsExtensionSupported(Type extensionType) {
			Requires.NotNullSubtype<IOpenIdMessageExtension>(extensionType, "extensionType");
			throw new NotImplementedException();
		}

		#endregion
	}
}
