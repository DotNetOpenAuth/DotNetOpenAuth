//-----------------------------------------------------------------------
// <copyright file="OpenIdProviderChannel.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Provider;
	using Validation;

	/// <summary>
	/// The messaging channel for OpenID Providers.
	/// </summary>
	internal class OpenIdProviderChannel : OpenIdChannel {
		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdProviderChannel" /> class.
		/// </summary>
		/// <param name="cryptoKeyStore">The OpenID Provider's association store or handle encoder.</param>
		/// <param name="nonceStore">The nonce store to use.</param>
		/// <param name="securitySettings">The security settings.</param>
		/// <param name="hostFactories">The host factories.</param>
		internal OpenIdProviderChannel(IProviderAssociationStore cryptoKeyStore, INonceStore nonceStore, ProviderSecuritySettings securitySettings, IHostFactories hostFactories)
			: this(cryptoKeyStore, nonceStore, new OpenIdProviderMessageFactory(), securitySettings, hostFactories) {
			Requires.NotNull(cryptoKeyStore, "cryptoKeyStore");
			Requires.NotNull(securitySettings, "securitySettings");
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdProviderChannel" /> class.
		/// </summary>
		/// <param name="cryptoKeyStore">The association store to use.</param>
		/// <param name="nonceStore">The nonce store to use.</param>
		/// <param name="messageTypeProvider">An object that knows how to distinguish the various OpenID message types for deserialization purposes.</param>
		/// <param name="securitySettings">The security settings.</param>
		/// <param name="hostFactories">The host factories.</param>
		private OpenIdProviderChannel(IProviderAssociationStore cryptoKeyStore, INonceStore nonceStore, IMessageFactory messageTypeProvider, ProviderSecuritySettings securitySettings, IHostFactories hostFactories)
			: base(messageTypeProvider, InitializeBindingElements(cryptoKeyStore, nonceStore, securitySettings), hostFactories) {
			Requires.NotNull(cryptoKeyStore, "cryptoKeyStore");
			Requires.NotNull(messageTypeProvider, "messageTypeProvider");
			Requires.NotNull(securitySettings, "securitySettings");
		}

		/// <summary>
		/// Initializes the binding elements.
		/// </summary>
		/// <param name="cryptoKeyStore">The OpenID Provider's crypto key store.</param>
		/// <param name="nonceStore">The nonce store to use.</param>
		/// <param name="securitySettings">The security settings to apply.  Must be an instance of either RelyingPartySecuritySettings or ProviderSecuritySettings.</param>
		/// <returns>
		/// An array of binding elements which may be used to construct the channel.
		/// </returns>
		private static IChannelBindingElement[] InitializeBindingElements(IProviderAssociationStore cryptoKeyStore, INonceStore nonceStore, ProviderSecuritySettings securitySettings) {
			Requires.NotNull(cryptoKeyStore, "cryptoKeyStore");
			Requires.NotNull(securitySettings, "securitySettings");
			Requires.NotNull(nonceStore, "nonceStore");

			SigningBindingElement signingElement;
			signingElement = new ProviderSigningBindingElement(cryptoKeyStore, securitySettings);

			var extensionFactory = OpenIdExtensionFactoryAggregator.LoadFromConfiguration();

			List<IChannelBindingElement> elements = new List<IChannelBindingElement>(8);
			elements.Add(new ExtensionsBindingElement(extensionFactory, securitySettings, true));
			elements.Add(new StandardReplayProtectionBindingElement(nonceStore, true));
			elements.Add(new StandardExpirationBindingElement());
			elements.Add(signingElement);

			return elements.ToArray();
		}
	}
}
