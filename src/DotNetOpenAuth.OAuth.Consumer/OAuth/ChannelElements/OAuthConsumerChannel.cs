//-----------------------------------------------------------------------
// <copyright file="OAuthConsumerChannel.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;

	/// <summary>
	/// The messaging channel for OAuth 1.0(a) Consumers.
	/// </summary>
	internal class OAuthConsumerChannel : OAuthChannel {
		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthConsumerChannel"/> class.
		/// </summary>
		/// <param name="signingBindingElement">The binding element to use for signing.</param>
		/// <param name="store">The web application store to use for nonces.</param>
		/// <param name="tokenManager">The token manager instance to use.</param>
		/// <param name="securitySettings">The security settings.</param>
		/// <param name="messageFactory">The message factory.</param>
		[SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Diagnostics.Contracts.__ContractsRuntime.Requires<System.ArgumentNullException>(System.Boolean,System.String,System.String)", Justification = "Code contracts"), SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "securitySettings", Justification = "Code contracts")]
		internal OAuthConsumerChannel(ITamperProtectionChannelBindingElement signingBindingElement, INonceStore store, IConsumerTokenManager tokenManager, ConsumerSecuritySettings securitySettings, IMessageFactory messageFactory = null)
			: base(
			signingBindingElement,
			tokenManager,
			securitySettings,
			messageFactory ?? new OAuthConsumerMessageFactory(),
			InitializeBindingElements(signingBindingElement, store)) {
			Requires.NotNull(tokenManager, "tokenManager");
			Requires.NotNull(securitySettings, "securitySettings");
			Requires.NotNull(signingBindingElement, "signingBindingElement");
		}

		/// <summary>
		/// Gets the consumer secret for a given consumer key.
		/// </summary>
		/// <param name="consumerKey">The consumer key.</param>
		/// <returns>The consumer secret.</returns>
		protected override string GetConsumerSecret(string consumerKey) {
			var consumerTokenManager = (IConsumerTokenManager)this.TokenManager;
			ErrorUtilities.VerifyInternal(consumerKey == consumerTokenManager.ConsumerKey, "The token manager consumer key and the consumer key set earlier do not match!");
			return consumerTokenManager.ConsumerSecret;
		}

		/// <summary>
		/// Initializes the binding elements for the OAuth channel.
		/// </summary>
		/// <param name="signingBindingElement">The signing binding element.</param>
		/// <param name="store">The nonce store.</param>
		/// <returns>
		/// An array of binding elements used to initialize the channel.
		/// </returns>
		private static new IChannelBindingElement[] InitializeBindingElements(ITamperProtectionChannelBindingElement signingBindingElement, INonceStore store) {
			return OAuthChannel.InitializeBindingElements(signingBindingElement, store).ToArray();
		}
	}
}
