//-----------------------------------------------------------------------
// <copyright file="SimpleXrdsProviderEndpoint.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// A very simple IXrdsProviderEndpoint implementation for verifying that all positive
	/// assertions (particularly unsolicited ones) are received from OP endpoints that
	/// are deemed permissible by the host RP.
	/// </summary>
	internal class SimpleXrdsProviderEndpoint : IXrdsProviderEndpoint {
		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleXrdsProviderEndpoint"/> class.
		/// </summary>
		/// <param name="positiveAssertion">The positive assertion.</param>
		internal SimpleXrdsProviderEndpoint(PositiveAssertionResponse positiveAssertion) {
			this.Uri = positiveAssertion.ProviderEndpoint;
			this.Version = positiveAssertion.Version;
		}

		#region IXrdsProviderEndpoint Properties

		/// <summary>
		/// Gets the priority associated with this service that may have been given
		/// in the XRDS document.
		/// </summary>
		public int? ServicePriority {
			get { return null; }
		}

		/// <summary>
		/// Gets the priority associated with the service endpoint URL.
		/// </summary>
		/// <remarks>
		/// When sorting by priority, this property should be considered second after
		/// <see cref="ServicePriority"/>.
		/// </remarks>
		public int? UriPriority {
			get { return null; }
		}

		#endregion

		#region IProviderEndpoint Members

		/// <summary>
		/// Gets the detected version of OpenID implemented by the Provider.
		/// </summary>
		public Version Version { get; private set; }

		/// <summary>
		/// Gets the URL that the OpenID Provider receives authentication requests at.
		/// </summary>
		/// <value></value>
		public Uri Uri { get; private set; }

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
		public bool IsExtensionSupported<T>() where T : DotNetOpenAuth.OpenId.Messages.IOpenIdMessageExtension, new() {
			throw new NotSupportedException();
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
		public bool IsExtensionSupported(Type extensionType) {
			throw new NotSupportedException();
		}

		#endregion

		#region IXrdsProviderEndpoint Methods

		/// <summary>
		/// Checks for the presence of a given Type URI in an XRDS service.
		/// </summary>
		/// <param name="typeUri">The type URI to check for.</param>
		/// <returns>
		/// 	<c>true</c> if the service type uri is present; <c>false</c> otherwise.
		/// </returns>
		public bool IsTypeUriPresent(string typeUri) {
			throw new NotSupportedException();
		}

		#endregion
	}
}
