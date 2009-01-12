//-----------------------------------------------------------------------
// <copyright file="OpenIdProvider.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Web;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// Offers services for a web page that is acting as an OpenID identity server.
	/// </summary>
	public sealed class OpenIdProvider {
		/// <summary>
		/// Backing field for the <see cref="SecuritySettings"/> property.
		/// </summary>
		private ProviderSecuritySettings securitySettings;

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdProvider"/> class.
		/// </summary>
		/// <param name="applicationStore">The application store to use.  Cannot be null.</param>
		public OpenIdProvider(IProviderApplicationStore applicationStore)
			: this(applicationStore, applicationStore) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdProvider"/> class.
		/// </summary>
		/// <param name="associationStore">The association store to use.  Cannot be null.</param>
		/// <param name="nonceStore">The nonce store to use.  Cannot be null.</param>
		private OpenIdProvider(IAssociationStore<AssociationRelyingPartyType> associationStore, INonceStore nonceStore) {
			ErrorUtilities.VerifyArgumentNotNull(associationStore, "associationStore");
			ErrorUtilities.VerifyArgumentNotNull(nonceStore, "nonceStore");

			this.Channel = new OpenIdChannel(associationStore, nonceStore);
			this.AssociationStore = associationStore;
			this.SecuritySettings = ProviderSection.Configuration.SecuritySettings.CreateSecuritySettings();
		}

		/// <summary>
		/// Gets the channel to use for sending/receiving messages.
		/// </summary>
		public Channel Channel { get; internal set; }

		/// <summary>
		/// Gets the security settings used by this Provider.
		/// </summary>
		public ProviderSecuritySettings SecuritySettings {
			get {
				return this.securitySettings;
			}

			internal set {
				if (value == null) {
					throw new ArgumentNullException("value");
				}

				this.securitySettings = value;
			}
		}

		/// <summary>
		/// Gets the association store.
		/// </summary>
		internal IAssociationStore<AssociationRelyingPartyType> AssociationStore { get; private set; }

		/// <summary>
		/// Gets the web request handler to use for discovery and the part of
		/// authentication where direct messages are sent to an untrusted remote party.
		/// </summary>
		internal IDirectWebRequestHandler WebRequestHandler {
			get { return this.Channel.WebRequestHandler; }
		}

		/// <summary>
		/// Gets the incoming OpenID request if there is one, or null if none was detected.
		/// </summary>
		/// <returns>The request that the hosting Provider should possibly process and then transmit the response for.</returns>
		/// <remarks>
		/// <para>Requests may be infrastructural to OpenID and allow auto-responses, or they may
		/// be authentication requests where the Provider site has to make decisions based
		/// on its own user database and policies.</para>
		/// <para>Requires an <see cref="HttpContext.Current">HttpContext.Current</see> context.</para>
		/// </remarks>
		/// <exception cref="InvalidOperationException">Thrown if <see cref="HttpContext.Current">HttpContext.Current</see> == <c>null</c>.</exception>
		/// <exception cref="ProtocolException">Thrown if the incoming message is recognized but deviates from the protocol specification irrecoverably.</exception>
		public IRequest GetRequest() {
			return this.GetRequest(this.Channel.GetRequestFromContext());
		}

		/// <summary>
		/// Gets the incoming OpenID request if there is one, or null if none was detected.
		/// </summary>
		/// <param name="httpRequestInfo">The incoming HTTP request to extract the message from.</param>
		/// <returns>
		/// The request that the hosting Provider should process and then transmit the response for.
		/// Null if no valid OpenID request was detected in the given HTTP request.
		/// </returns>
		/// <remarks>
		/// Requests may be infrastructural to OpenID and allow auto-responses, or they may
		/// be authentication requests where the Provider site has to make decisions based
		/// on its own user database and policies.
		/// </remarks>
		/// <exception cref="ProtocolException">Thrown if the incoming message is recognized but deviates from the protocol specification irrecoverably.</exception>
		public IRequest GetRequest(HttpRequestInfo httpRequestInfo) {
			ErrorUtilities.VerifyArgumentNotNull(httpRequestInfo, "httpRequestInfo");

			IDirectedProtocolMessage incomingMessage = this.Channel.ReadFromRequest(httpRequestInfo);
			if (incomingMessage == null) {
				return null;
			}

			var checkIdMessage = incomingMessage as CheckIdRequest;
			if (checkIdMessage != null) {
				return new AuthenticationRequest(this, checkIdMessage);
			}

			var checkAuthMessage = incomingMessage as CheckAuthenticationRequest;
			if (checkAuthMessage != null) {
				return new AutoResponsiveRequest(this, incomingMessage, new CheckAuthenticationResponse(checkAuthMessage));
			}

			var associateMessage = incomingMessage as AssociateRequest;
			if (associateMessage != null) {
				return new AutoResponsiveRequest(this, incomingMessage, associateMessage.CreateResponse(this.AssociationStore));
			}

			throw ErrorUtilities.ThrowProtocol(MessagingStrings.UnexpectedMessageReceivedOfMany);
		}
	}
}
