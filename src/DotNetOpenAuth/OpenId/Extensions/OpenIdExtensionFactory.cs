//-----------------------------------------------------------------------
// <copyright file="OpenIdExtensionFactory.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;

	internal class OpenIdExtensionFactory : IOpenIdExtensionFactory {
		/// <summary>
		/// A delegate that individual extensions may register with this factory.
		/// </summary>
		internal delegate IOpenIdMessageExtension CreateDelegate(string typeUri, IDictionary<string, string> data, IProtocolMessageWithExtensions baseMessage);

		/// <summary>
		/// A collection of the registered OpenID extensions.
		/// </summary>
		private List<CreateDelegate> registeredExtensions = new List<CreateDelegate>();

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdExtensionFactory"/> class.
		/// </summary>
		internal OpenIdExtensionFactory() {
			this.RegisterExtension(ClaimsRequest.Factory);
			this.RegisterExtension(ClaimsResponse.Factory);
		}

		#region IOpenIdExtensionFactory Members

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
		public IOpenIdMessageExtension Create(string typeUri, IDictionary<string, string> data, IProtocolMessageWithExtensions baseMessage) {
			foreach (var factoryMethod in registeredExtensions) {
				IOpenIdMessageExtension result = factoryMethod(typeUri, data, baseMessage);
				if (result != null) {
					return result;
				}
			}

			return null;
		}

		#endregion

		/// <summary>
		/// Registers a new extension delegate.
		/// </summary>
		/// <param name="creator">The factory method that can create the extension.</param>
		internal void RegisterExtension(CreateDelegate creator) {
			this.registeredExtensions.Add(creator);
		}
	}
}
