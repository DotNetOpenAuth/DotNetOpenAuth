//-----------------------------------------------------------------------
// <copyright file="IRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;
	using Validation;

	/// <summary>
	/// Represents an incoming OpenId authentication request.
	/// </summary>
	/// <remarks>
	/// Requests may be infrastructural to OpenID and allow auto-responses, or they may
	/// be authentication requests where the Provider site has to make decisions based
	/// on its own user database and policies.
	/// </remarks>
	public interface IRequest {
		/// <summary>
		/// Gets a value indicating whether the response is ready to be sent to the user agent.
		/// </summary>
		/// <remarks>
		/// This property returns false if there are properties that must be set on this
		/// request instance before the response can be sent.
		/// </remarks>
		bool IsResponseReady { get; }

		/// <summary>
		/// Gets or sets the security settings that apply to this request.
		/// </summary>
		/// <value>Defaults to the OpenIdProvider.SecuritySettings on the OpenIdProvider.</value>
		ProviderSecuritySettings SecuritySettings { get; set; }

		/// <summary>
		/// Adds an extension to the response to send to the relying party.
		/// </summary>
		/// <param name="extension">The extension to add to the response message.</param>
		void AddResponseExtension(IOpenIdMessageExtension extension);

		/// <summary>
		/// Removes any response extensions previously added using <see cref="IRequest.AddResponseExtension"/>.
		/// </summary>
		/// <remarks>
		/// This should be called before sending a negative response back to the relying party
		/// if extensions were already added, since negative responses cannot carry extensions.
		/// </remarks>
		void ClearResponseExtensions();

		/// <summary>
		/// Gets an extension sent from the relying party.
		/// </summary>
		/// <typeparam name="T">The type of the extension.</typeparam>
		/// <returns>An instance of the extension initialized with values passed in with the request.</returns>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "No parameter to make of type T.")]
		T GetExtension<T>() where T : IOpenIdMessageExtension, new();

		/// <summary>
		/// Gets an extension sent from the relying party.
		/// </summary>
		/// <param name="extensionType">The type of the extension.</param>
		/// <returns>An instance of the extension initialized with values passed in with the request.</returns>
		IOpenIdMessageExtension GetExtension(Type extensionType);
	}
}
