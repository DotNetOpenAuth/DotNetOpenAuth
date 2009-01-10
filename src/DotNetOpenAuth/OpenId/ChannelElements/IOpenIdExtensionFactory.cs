//-----------------------------------------------------------------------
// <copyright file="IOpenIdExtensionFactory.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.ChannelElements {
	using System.Collections.Generic;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// OpenID extension factory class for creating extensions based on received Type URIs.
	/// </summary>
	internal interface IOpenIdExtensionFactory {
		/// <summary>
		/// Creates a new instance of some extension based on the received extension parameters.
		/// </summary>
		/// <param name="typeUri">The type URI of the extension.</param>
		/// <param name="data">The parameters associated specifically with this extension.</param>
		/// <param name="baseMessage">The OpenID message carrying this extension.</param>
		/// <returns>
		/// An instance of <see cref="IOpenIdMessageExtension"/> if the factory recognizes
		/// the extension described in the input parameters; <c>null</c> otherwise.
		/// </returns>
		/// <remarks>
		/// This factory method need only initialize properties in the instantiated extension object
		/// that are not bound using <see cref="MessagePartAttribute"/>.
		/// </remarks>
		IOpenIdMessageExtension Create(string typeUri, IDictionary<string, string> data, IProtocolMessageWithExtensions baseMessage);
	}
}
