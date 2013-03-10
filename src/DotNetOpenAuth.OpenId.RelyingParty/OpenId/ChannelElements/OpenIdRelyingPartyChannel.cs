//-----------------------------------------------------------------------
// <copyright file="OpenIdRelyingPartyChannel.cs" company="Outercurve Foundation">
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
	using DotNetOpenAuth.OpenId.RelyingParty;
	using Validation;

	/// <summary>
	/// The messaging channel for OpenID relying parties.
	/// </summary>
	internal class OpenIdRelyingPartyChannel : OpenIdChannel {
		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdRelyingPartyChannel" /> class.
		/// </summary>
		/// <param name="cryptoKeyStore">The association store to use.</param>
		/// <param name="nonceStore">The nonce store to use.</param>
		/// <param name="securitySettings">The security settings to apply.</param>
		/// <param name="hostFactories">The host factories.</param>
		internal OpenIdRelyingPartyChannel(ICryptoKeyStore cryptoKeyStore, INonceStore nonceStore, RelyingPartySecuritySettings securitySettings, IHostFactories hostFactories)
			: this(cryptoKeyStore, nonceStore, new OpenIdRelyingPartyMessageFactory(), securitySettings, false, hostFactories) {
			Requires.NotNull(securitySettings, "securitySettings");
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdRelyingPartyChannel" /> class.
		/// </summary>
		/// <param name="cryptoKeyStore">The association store to use.</param>
		/// <param name="nonceStore">The nonce store to use.</param>
		/// <param name="messageTypeProvider">An object that knows how to distinguish the various OpenID message types for deserialization purposes.</param>
		/// <param name="securitySettings">The security settings to apply.</param>
		/// <param name="nonVerifying">A value indicating whether the channel is set up with no functional security binding elements.</param>
		/// <param name="hostFactories">The host factories.</param>
		private OpenIdRelyingPartyChannel(ICryptoKeyStore cryptoKeyStore, INonceStore nonceStore, IMessageFactory messageTypeProvider, RelyingPartySecuritySettings securitySettings, bool nonVerifying, IHostFactories hostFactories) :
			base(messageTypeProvider, InitializeBindingElements(cryptoKeyStore, nonceStore, securitySettings, nonVerifying), hostFactories) {
			Requires.NotNull(messageTypeProvider, "messageTypeProvider");
			Requires.NotNull(securitySettings, "securitySettings");
			Assumes.True(!nonVerifying || securitySettings is RelyingPartySecuritySettings);
		}

		/// <summary>
		/// A value indicating whether the channel is set up
		/// with no functional security binding elements.
		/// </summary>
		/// <returns>A new <see cref="OpenIdChannel"/> instance that will not perform verification on incoming messages or apply any security to outgoing messages.</returns>
		/// <remarks>
		/// 	<para>A value of <c>true</c> allows the relying party to preview incoming
		/// messages without invalidating nonces or checking signatures.</para>
		/// 	<para>Setting this to <c>true</c> poses a great security risk and is only
		/// present to support the OpenIdAjaxTextBox which needs to preview
		/// messages, and will validate them later.</para>
		/// </remarks>
		internal static OpenIdChannel CreateNonVerifyingChannel() {
			return new OpenIdRelyingPartyChannel(null, null, new OpenIdRelyingPartyMessageFactory(), new RelyingPartySecuritySettings(), true, new DefaultOpenIdHostFactories());
		}

		/// <summary>
		/// Initializes the binding elements.
		/// </summary>
		/// <param name="cryptoKeyStore">The crypto key store.</param>
		/// <param name="nonceStore">The nonce store to use.</param>
		/// <param name="securitySettings">The security settings to apply.  Must be an instance of either <see cref="RelyingPartySecuritySettings"/> or ProviderSecuritySettings.</param>
		/// <param name="nonVerifying">A value indicating whether the channel is set up with no functional security binding elements.</param>
		/// <returns>
		/// An array of binding elements which may be used to construct the channel.
		/// </returns>
		private static IChannelBindingElement[] InitializeBindingElements(ICryptoKeyStore cryptoKeyStore, INonceStore nonceStore, RelyingPartySecuritySettings securitySettings, bool nonVerifying) {
			Requires.NotNull(securitySettings, "securitySettings");

			SigningBindingElement signingElement;
			signingElement = nonVerifying ? null : new RelyingPartySigningBindingElement(new CryptoKeyStoreAsRelyingPartyAssociationStore(cryptoKeyStore ?? new MemoryCryptoKeyStore()));

			var extensionFactory = OpenIdExtensionFactoryAggregator.LoadFromConfiguration();

			List<IChannelBindingElement> elements = new List<IChannelBindingElement>(8);
			elements.Add(new ExtensionsBindingElementRelyingParty(extensionFactory, securitySettings));
			elements.Add(new RelyingPartySecurityOptions(securitySettings));
			elements.Add(new BackwardCompatibilityBindingElement());
			ReturnToNonceBindingElement requestNonceElement = null;

			if (cryptoKeyStore != null) {
				if (nonceStore != null) {
					// There is no point in having a ReturnToNonceBindingElement without
					// a ReturnToSignatureBindingElement because the nonce could be
					// artificially changed without it.
					requestNonceElement = new ReturnToNonceBindingElement(nonceStore, securitySettings);
					elements.Add(requestNonceElement);
				}

				// It is important that the return_to signing element comes last
				// so that the nonce is included in the signature.
				elements.Add(new ReturnToSignatureBindingElement(cryptoKeyStore));
			}

			ErrorUtilities.VerifyOperation(!securitySettings.RejectUnsolicitedAssertions || requestNonceElement != null, OpenIdStrings.UnsolicitedAssertionRejectionRequiresNonceStore);

			if (nonVerifying) {
				elements.Add(new SkipSecurityBindingElement());
			} else {
				if (nonceStore != null) {
					elements.Add(new StandardReplayProtectionBindingElement(nonceStore, true));
				}

				elements.Add(new StandardExpirationBindingElement());
				elements.Add(signingElement);
			}

			return elements.ToArray();
		}
	}
}
