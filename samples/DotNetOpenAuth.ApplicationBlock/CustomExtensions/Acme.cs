//-----------------------------------------------------------------------
// <copyright file="Acme.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.ApplicationBlock.CustomExtensions {
	using System;
	using System.Collections.Generic;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;

	/// <summary>
	/// A sample custom OpenID extension factory.
	/// </summary>
	/// <remarks>
	/// OpenID extension factories must be registered with the library.  This can be
	/// done by calling <see cref="Acme.Register"/>, or by adding a snippet
	/// such as the following to your web.config file:
	/// <example>
	///   &lt;dotNetOpenAuth&gt;
	///     &lt;openid&gt;
	///       &lt;extensionFactories&gt;
	///         &lt;add type="DotNetOpenAuth.ApplicationBlock.CustomExtensions.Acme, DotNetOpenAuth.ApplicationBlock" /&gt;
	///       &lt;/extensionFactories&gt;
	///     &lt;/openid&gt;
	///   &lt;/dotNetOpenAuth&gt;
	/// </example>
	/// </remarks>
	public class Acme : IOpenIdExtensionFactory {
		internal const string CustomExtensionTypeUri = "testextension";
		internal static readonly Version Version = new Version(1, 0);

		public static void Register(OpenIdRelyingParty relyingParty) {
			if (relyingParty == null) {
				throw new ArgumentNullException("relyingParty");
			}

			relyingParty.ExtensionFactories.Add(new Acme());
		}

		public static void Register(OpenIdProvider provider) {
			if (provider == null) {
				throw new ArgumentNullException("provider");
			}

			provider.ExtensionFactories.Add(new Acme());
		}

		#region IOpenIdExtensionFactory Members

		/// <summary>
		/// Creates a new instance of some extension based on the received extension parameters.
		/// </summary>
		/// <param name="typeUri">The type URI of the extension.</param>
		/// <param name="data">The parameters associated specifically with this extension.</param>
		/// <param name="baseMessage">The OpenID message carrying this extension.</param>
		/// <param name="isProviderRole">A value indicating whether this extension is being received at the OpenID Provider.</param>
		/// <returns>
		/// An instance of <see cref="IOpenIdMessageExtension"/> if the factory recognizes
		/// the extension described in the input parameters; <c>null</c> otherwise.
		/// </returns>
		/// <remarks>
		/// This factory method need only initialize properties in the instantiated extension object
		/// that are not bound using <see cref="MessagePartAttribute"/>.
		/// </remarks>
		public IOpenIdMessageExtension Create(string typeUri, IDictionary<string, string> data, IProtocolMessageWithExtensions baseMessage, bool isProviderRole) {
			if (typeUri == CustomExtensionTypeUri) {
				return isProviderRole ? (IOpenIdMessageExtension)new AcmeRequest() : new AcmeResponse();
			}

			return null;
		}

		#endregion
	}
}
