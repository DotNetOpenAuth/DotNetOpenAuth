//-----------------------------------------------------------------------
// <copyright file="OpenIdProvider.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Provider {
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Collections.Specialized;
	using System.ComponentModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Messages;
	using Validation;
	using RP = DotNetOpenAuth.OpenId.RelyingParty;

	/// <summary>
	/// Offers services for a web page that is acting as an OpenID identity server.
	/// </summary>
	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "By design")]
	public sealed class OpenIdProvider : IDisposable, IOpenIdHost {
		/// <summary>
		/// The name of the key to use in the HttpApplication cache to store the
		/// instance of <see cref="MemoryCryptoKeyAndNonceStore"/> to use.
		/// </summary>
		private const string ApplicationStoreKey = "DotNetOpenAuth.OpenId.Provider.OpenIdProvider.ApplicationStore";

		/// <summary>
		/// Backing store for the <see cref="Behaviors"/> property.
		/// </summary>
		private readonly ObservableCollection<IProviderBehavior> behaviors = new ObservableCollection<IProviderBehavior>();

		/// <summary>
		/// The discovery service used to perform discovery on identifiers being sent in
		/// unsolicited positive assertions.
		/// </summary>
		private readonly IdentifierDiscoveryServices discoveryServices;

		/// <summary>
		/// Backing field for the <see cref="SecuritySettings"/> property.
		/// </summary>
		private ProviderSecuritySettings securitySettings;

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdProvider"/> class.
		/// </summary>
		public OpenIdProvider()
			: this(OpenIdElement.Configuration.Provider.ApplicationStore.CreateInstance(GetHttpApplicationStore(), null)) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdProvider" /> class.
		/// </summary>
		/// <param name="applicationStore">The application store to use.  Cannot be null.</param>
		/// <param name="hostFactories">The host factories.</param>
		public OpenIdProvider(ICryptoKeyAndNonceStore applicationStore, IHostFactories hostFactories = null)
			: this((INonceStore)applicationStore, (ICryptoKeyStore)applicationStore, hostFactories) {
			Requires.NotNull(applicationStore, "applicationStore");
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdProvider" /> class.
		/// </summary>
		/// <param name="nonceStore">The nonce store to use.  Cannot be null.</param>
		/// <param name="cryptoKeyStore">The crypto key store.  Cannot be null.</param>
		/// <param name="hostFactories">The host factories.</param>
		private OpenIdProvider(INonceStore nonceStore, ICryptoKeyStore cryptoKeyStore, IHostFactories hostFactories) {
			Requires.NotNull(nonceStore, "nonceStore");
			Requires.NotNull(cryptoKeyStore, "cryptoKeyStore");

			this.SecuritySettings = OpenIdElement.Configuration.Provider.SecuritySettings.CreateSecuritySettings();
			this.behaviors.CollectionChanged += this.OnBehaviorsChanged;
			foreach (var behavior in OpenIdElement.Configuration.Provider.Behaviors.CreateInstances(false, null)) {
				this.behaviors.Add(behavior);
			}

			this.AssociationStore = new SwitchingAssociationStore(cryptoKeyStore, this.SecuritySettings);
			this.Channel = new OpenIdProviderChannel(this.AssociationStore, nonceStore, this.SecuritySettings, hostFactories);
			this.CryptoKeyStore = cryptoKeyStore;
			this.discoveryServices = new IdentifierDiscoveryServices(this);

			Reporting.RecordFeatureAndDependencyUse(this, nonceStore);
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
				Assumes.True(this.securitySettings != null);
				return this.securitySettings;
			}

			internal set {
				Requires.NotNull(value, "value");
				this.securitySettings = value;
			}
		}

		/// <summary>
		/// Gets the security settings.
		/// </summary>
		SecuritySettings IOpenIdHost.SecuritySettings {
			get { return this.SecuritySettings; }
		}

		/// <summary>
		/// Gets the extension factories.
		/// </summary>
		public IList<IOpenIdExtensionFactory> ExtensionFactories {
			get { return this.Channel.GetExtensionFactories(); }
		}

		/// <summary>
		/// Gets or sets the mechanism a host site can use to receive
		/// notifications of errors when communicating with remote parties.
		/// </summary>
		public IErrorReporting ErrorReporting { get; set; }

		/// <summary>
		/// Gets a list of custom behaviors to apply to OpenID actions.
		/// </summary>
		/// <remarks>
		/// Adding behaviors can impact the security settings of the <see cref="OpenIdProvider"/>
		/// in ways that subsequently removing the behaviors will not reverse.
		/// </remarks>
		public ICollection<IProviderBehavior> Behaviors {
			get { return this.behaviors; }
		}

		/// <summary>
		/// Gets the crypto key store.
		/// </summary>
		public ICryptoKeyStore CryptoKeyStore { get; private set; }

		/// <summary>
		/// Gets the factory for various dependencies.
		/// </summary>
		IHostFactories IOpenIdHost.HostFactories {
			get { return this.Channel.HostFactories; }
		}

		/// <summary>
		/// Gets the association store.
		/// </summary>
		internal IProviderAssociationStore AssociationStore { get; private set; }

		/// <summary>
		/// Gets the channel.
		/// </summary>
		internal OpenIdChannel OpenIdChannel {
			get { return (OpenIdChannel)this.Channel; }
		}

		/// <summary>
		/// Gets the list of services that can perform discovery on identifiers given.
		/// </summary>
		internal IList<IIdentifierDiscoveryService> DiscoveryServices {
			get { return this.discoveryServices.DiscoveryServices; }
		}

		/// <summary>
		/// Gets the standard state storage mechanism that uses ASP.NET's
		/// HttpApplication state dictionary to store associations and nonces.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns>The application store.</returns>
		public static ICryptoKeyAndNonceStore GetHttpApplicationStore(HttpContextBase context = null) {
			if (context == null) {
				ErrorUtilities.VerifyOperation(HttpContext.Current != null, Strings.StoreRequiredWhenNoHttpContextAvailable, typeof(ICryptoKeyAndNonceStore).Name);
				context = new HttpContextWrapper(HttpContext.Current);
			}

			var store = (ICryptoKeyAndNonceStore)context.Application[ApplicationStoreKey];
			if (store == null) {
				context.Application.Lock();
				try {
					if ((store = (ICryptoKeyAndNonceStore)context.Application[ApplicationStoreKey]) == null) {
						context.Application[ApplicationStoreKey] = store = new MemoryCryptoKeyAndNonceStore();
					}
				} finally {
					context.Application.UnLock();
				}
			}

			return store;
		}

		/// <summary>
		/// Gets the incoming OpenID request if there is one, or null if none was detected.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The request that the hosting Provider should possibly process and then transmit the response for.
		/// </returns>
		/// <exception cref="InvalidOperationException">Thrown if <see cref="HttpContext.Current">HttpContext.Current</see> == <c>null</c>.</exception>
		/// <exception cref="ProtocolException">Thrown if the incoming message is recognized but deviates from the protocol specification irrecoverably.</exception>
		/// <remarks>
		///   <para>Requests may be infrastructural to OpenID and allow auto-responses, or they may
		/// be authentication requests where the Provider site has to make decisions based
		/// on its own user database and policies.</para>
		///   <para>Requires an <see cref="HttpContext.Current">HttpContext.Current</see> context.</para>
		/// </remarks>
		public Task<IRequest> GetRequestAsync(HttpRequestBase request = null, CancellationToken cancellationToken = default(CancellationToken)) {
			request = request ?? this.Channel.GetRequestFromContext();
			return this.GetRequestAsync(request.AsHttpRequestMessage(), cancellationToken);
		}

		/// <summary>
		/// Gets the incoming OpenID request if there is one, or null if none was detected.
		/// </summary>
		/// <param name="request">The incoming HTTP request to extract the message from.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The request that the hosting Provider should process and then transmit the response for.
		/// Null if no valid OpenID request was detected in the given HTTP request.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown if the incoming message is recognized
		/// but deviates from the protocol specification irrecoverably.</exception>
		/// <remarks>
		/// Requests may be infrastructural to OpenID and allow auto-responses, or they may
		/// be authentication requests where the Provider site has to make decisions based
		/// on its own user database and policies.
		/// </remarks>
		public async Task<IRequest> GetRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken = default(CancellationToken)) {
			Requires.NotNull(request, "request");
			IDirectedProtocolMessage incomingMessage = null;

			try {
				incomingMessage = await this.Channel.ReadFromRequestAsync(request, cancellationToken);
				if (incomingMessage == null) {
					// If the incoming request does not resemble an OpenID message at all,
					// it's probably a user who just navigated to this URL, and we should
					// just return null so the host can display a message to the user.
					if (request.Method == HttpMethod.Get && !request.RequestUri.QueryStringContainPrefixedParameters(Protocol.Default.openid.Prefix)) {
						return null;
					}

					ErrorUtilities.ThrowProtocol(MessagingStrings.UnexpectedMessageReceivedOfMany);
				}

				IRequest result = null;

				var checkIdMessage = incomingMessage as CheckIdRequest;
				if (checkIdMessage != null) {
					result = new AuthenticationRequest(this, checkIdMessage);
				}

				if (result == null) {
					var extensionOnlyRequest = incomingMessage as SignedResponseRequest;
					if (extensionOnlyRequest != null) {
						result = new AnonymousRequest(this, extensionOnlyRequest);
					}
				}

				if (result == null) {
					var checkAuthMessage = incomingMessage as CheckAuthenticationRequest;
					if (checkAuthMessage != null) {
						result = new AutoResponsiveRequest(incomingMessage, new CheckAuthenticationResponseProvider(checkAuthMessage, this), this.SecuritySettings);
					}
				}

				if (result == null) {
					var associateMessage = incomingMessage as IAssociateRequestProvider;
					if (associateMessage != null) {
						result = new AutoResponsiveRequest(incomingMessage, AssociateRequestProviderTools.CreateResponse(associateMessage, this.AssociationStore, this.SecuritySettings), this.SecuritySettings);
					}
				}

				if (result != null) {
					foreach (var behavior in this.Behaviors) {
						if (await behavior.OnIncomingRequestAsync(result, cancellationToken)) {
							// This behavior matched this request.
							break;
						}
					}

					return result;
				}

				throw ErrorUtilities.ThrowProtocol(MessagingStrings.UnexpectedMessageReceivedOfMany);
			} catch (ProtocolException ex) {
				IRequest errorResponse = this.GetErrorResponse(ex, request, incomingMessage);
				if (errorResponse == null) {
					throw;
				}

				return errorResponse;
			}
		}

		/// <summary>
		/// Gets the response to a received request.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The response that should be sent to the client.
		/// </returns>
		/// <exception cref="InvalidOperationException">Thrown if <see cref="IRequest.IsResponseReady" /> is <c>false</c>.</exception>
		[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "Code Contract requires that we cast early.")]
		public async Task<HttpResponseMessage> PrepareResponseAsync(IRequest request, CancellationToken cancellationToken = default(CancellationToken)) {
			Requires.NotNull(request, "request");
			Requires.That(request.IsResponseReady, "request", OpenIdStrings.ResponseNotReady);

			await this.ApplyBehaviorsToResponseAsync(request, cancellationToken);
			Request requestInternal = (Request)request;
			var response = await requestInternal.GetResponseAsync(cancellationToken);
			return await this.Channel.PrepareResponseAsync(response, cancellationToken);
		}

		/// <summary>
		/// Prepares an identity assertion on behalf of one of this Provider's
		/// members in order to redirect the user agent to a relying party
		/// web site and log him/her in immediately in one uninterrupted step.
		/// </summary>
		/// <param name="providerEndpoint">The absolute URL on the Provider site that receives OpenID messages.</param>
		/// <param name="relyingPartyRealm">The URL of the Relying Party web site.
		/// This will typically be the home page, but may be a longer URL if
		/// that Relying Party considers the scope of its realm to be more specific.
		/// The URL provided here must allow discovery of the Relying Party's
		/// XRDS document that advertises its OpenID RP endpoint.</param>
		/// <param name="claimedIdentifier">The Identifier you are asserting your member controls.</param>
		/// <param name="localIdentifier">The Identifier you know your user by internally.  This will typically
		/// be the same as <paramref name="claimedIdentifier" />.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <param name="extensions">The extensions.</param>
		/// <returns>
		/// A <see cref="HttpResponseMessage" /> object describing the HTTP response to send
		/// the user agent to allow the redirect with assertion to happen.
		/// </returns>
		public async Task<HttpResponseMessage> PrepareUnsolicitedAssertionAsync(Uri providerEndpoint, Realm relyingPartyRealm, Identifier claimedIdentifier, Identifier localIdentifier, CancellationToken cancellationToken = default(CancellationToken), params IExtensionMessage[] extensions) {
			Requires.NotNull(providerEndpoint, "providerEndpoint");
			Requires.That(providerEndpoint.IsAbsoluteUri, "providerEndpoint", OpenIdStrings.AbsoluteUriRequired);
			Requires.NotNull(relyingPartyRealm, "relyingPartyRealm");
			Requires.NotNull(claimedIdentifier, "claimedIdentifier");
			Requires.NotNull(localIdentifier, "localIdentifier");
			RequiresEx.ValidState(this.Channel.HostFactories != null);

			// Although the RP should do their due diligence to make sure that this OP
			// is authorized to send an assertion for the given claimed identifier,
			// do due diligence by performing our own discovery on the claimed identifier
			// and make sure that it is tied to this OP and OP local identifier.
			if (this.SecuritySettings.UnsolicitedAssertionVerification != ProviderSecuritySettings.UnsolicitedAssertionVerificationLevel.NeverVerify) {
				var serviceEndpoint = IdentifierDiscoveryResult.CreateForClaimedIdentifier(claimedIdentifier, localIdentifier, new ProviderEndpointDescription(providerEndpoint, Protocol.Default.Version), null, null);
				var discoveredEndpoints = await this.discoveryServices.DiscoverAsync(claimedIdentifier, cancellationToken);
				if (!discoveredEndpoints.Contains(serviceEndpoint)) {
					Logger.OpenId.WarnFormat(
						"Failed to send unsolicited assertion for {0} because its discovered services did not include this endpoint: {1}{2}{1}Discovered endpoints: {1}{3}",
						claimedIdentifier,
						Environment.NewLine,
						serviceEndpoint,
						discoveredEndpoints.ToStringDeferred(true));

					// Only FAIL if the setting is set for it.
					if (this.securitySettings.UnsolicitedAssertionVerification == ProviderSecuritySettings.UnsolicitedAssertionVerificationLevel.RequireSuccess) {
						ErrorUtilities.ThrowProtocol(OpenIdStrings.UnsolicitedAssertionForUnrelatedClaimedIdentifier, claimedIdentifier);
					}
				}
			}

			Logger.OpenId.InfoFormat("Preparing unsolicited assertion for {0}", claimedIdentifier);
			RelyingPartyEndpointDescription returnToEndpoint = null;
			var returnToEndpoints = await relyingPartyRealm.DiscoverReturnToEndpointsAsync(this.Channel.HostFactories, true, cancellationToken);
			if (returnToEndpoints != null) {
				returnToEndpoint = returnToEndpoints.FirstOrDefault();
			}
			ErrorUtilities.VerifyProtocol(returnToEndpoint != null, OpenIdStrings.NoRelyingPartyEndpointDiscovered, relyingPartyRealm);

			var positiveAssertion = new PositiveAssertionResponse(returnToEndpoint) {
				ProviderEndpoint = providerEndpoint,
				ClaimedIdentifier = claimedIdentifier,
				LocalIdentifier = localIdentifier,
			};

			if (extensions != null) {
				foreach (IExtensionMessage extension in extensions) {
					positiveAssertion.Extensions.Add(extension);
				}
			}

			Reporting.RecordEventOccurrence(this, "PrepareUnsolicitedAssertion");
			return await this.Channel.PrepareResponseAsync(positiveAssertion, cancellationToken);
		}

		#region IDisposable Members

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing) {
			if (disposing) {
				// Tear off the instance member as a local variable for thread safety.
				IDisposable channel = this.Channel as IDisposable;
				if (channel != null) {
					channel.Dispose();
				}
			}
		}

		#endregion

		/// <summary>
		/// Applies all behaviors to the response message.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A task that completes with the asynchronous operation.
		/// </returns>
		private async Task ApplyBehaviorsToResponseAsync(IRequest request, CancellationToken cancellationToken) {
			var authRequest = request as IAuthenticationRequest;
			if (authRequest != null) {
				foreach (var behavior in this.Behaviors) {
					if (await behavior.OnOutgoingResponseAsync(authRequest, cancellationToken)) {
						// This behavior matched this request.
						break;
					}
				}
			}
		}

		/// <summary>
		/// Prepares the return value for the GetRequest method in the event of an exception.
		/// </summary>
		/// <param name="ex">The exception that forms the basis of the error response.  Must not be null.</param>
		/// <param name="request">The incoming HTTP request.  Must not be null.</param>
		/// <param name="incomingMessage">The incoming message.  May be null in the case that it was malformed.</param>
		/// <returns>
		/// Either the <see cref="IRequest"/> to return to the host site or null to indicate no response could be reasonably created and that the caller should rethrow the exception.
		/// </returns>
		private IRequest GetErrorResponse(ProtocolException ex, HttpRequestMessage request, IDirectedProtocolMessage incomingMessage) {
			Requires.NotNull(ex, "ex");
			Requires.NotNull(request, "request");

			Logger.OpenId.ErrorException("An exception was generated while processing an incoming OpenID request.", ex);
			IErrorMessage errorMessage;

			// We must create the appropriate error message type (direct vs. indirect)
			// based on what we see in the request.
			string returnTo = HttpUtility.ParseQueryString(request.RequestUri.Query)[Protocol.Default.openid.return_to];
			if (returnTo != null) {
				// An indirect request message from the RP
				// We need to return an indirect response error message so the RP can consume it.
				// Consistent with OpenID 2.0 section 5.2.3.
				var indirectRequest = incomingMessage as SignedResponseRequest;
				if (indirectRequest != null) {
					errorMessage = new IndirectErrorResponse(indirectRequest);
				} else {
					errorMessage = new IndirectErrorResponse(Protocol.Default.Version, new Uri(returnTo));
				}
			} else if (request.Method == HttpMethod.Post) {
				// A direct request message from the RP
				// We need to return a direct response error message so the RP can consume it.
				// Consistent with OpenID 2.0 section 5.1.2.2.
				errorMessage = new DirectErrorResponse(Protocol.Default.Version, incomingMessage);
			} else {
				// This may be an indirect request from an RP that was so badly
				// formed that we cannot even return an error to the RP.
				// The best we can do is display an error to the user.
				// Returning null cues the caller to "throw;"
				return null;
			}

			errorMessage.ErrorMessage = ex.ToStringDescriptive();

			// Allow host to log this error and issue a ticket #.
			// We tear off the field to a local var for thread safety.
			IErrorReporting hostErrorHandler = this.ErrorReporting;
			if (hostErrorHandler != null) {
				errorMessage.Contact = hostErrorHandler.Contact;
				errorMessage.Reference = hostErrorHandler.LogError(ex);
			}

			if (incomingMessage != null) {
				return new AutoResponsiveRequest(incomingMessage, errorMessage, this.SecuritySettings);
			} else {
				return new AutoResponsiveRequest(errorMessage, this.SecuritySettings);
			}
		}

		/// <summary>
		/// Called by derived classes when behaviors are added or removed.
		/// </summary>
		/// <param name="sender">The collection being modified.</param>
		/// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
		private void OnBehaviorsChanged(object sender, NotifyCollectionChangedEventArgs e) {
			foreach (IProviderBehavior profile in e.NewItems) {
				profile.ApplySecuritySettings(this.SecuritySettings);
				Reporting.RecordFeatureUse(profile);
			}
		}

		/// <summary>
		/// Provides a single OP association store instance that can handle switching between
		/// association handle encoding modes.
		/// </summary>
		private class SwitchingAssociationStore : IProviderAssociationStore {
			/// <summary>
			/// The security settings of the Provider.
			/// </summary>
			private readonly ProviderSecuritySettings securitySettings;

			/// <summary>
			/// The association store that records association secrets in the association handles themselves.
			/// </summary>
			private IProviderAssociationStore associationHandleEncoder;

			/// <summary>
			/// The association store that records association secrets in a secret store.
			/// </summary>
			private IProviderAssociationStore associationSecretStorage;

			/// <summary>
			/// Initializes a new instance of the <see cref="SwitchingAssociationStore"/> class.
			/// </summary>
			/// <param name="cryptoKeyStore">The crypto key store.</param>
			/// <param name="securitySettings">The security settings.</param>
			internal SwitchingAssociationStore(ICryptoKeyStore cryptoKeyStore, ProviderSecuritySettings securitySettings) {
				Requires.NotNull(cryptoKeyStore, "cryptoKeyStore");
				Requires.NotNull(securitySettings, "securitySettings");
				this.securitySettings = securitySettings;

				this.associationHandleEncoder = new ProviderAssociationHandleEncoder(cryptoKeyStore);
				this.associationSecretStorage = new ProviderAssociationKeyStorage(cryptoKeyStore);
			}

			/// <summary>
			/// Gets the association store that applies given the Provider's current security settings.
			/// </summary>
			internal IProviderAssociationStore AssociationStore {
				get { return this.securitySettings.EncodeAssociationSecretsInHandles ? this.associationHandleEncoder : this.associationSecretStorage; }
			}

			/// <summary>
			/// Stores an association and returns a handle for it.
			/// </summary>
			/// <param name="secret">The association secret.</param>
			/// <param name="expiresUtc">The UTC time that the association should expire.</param>
			/// <param name="privateAssociation">A value indicating whether this is a private association.</param>
			/// <returns>
			/// The association handle that represents this association.
			/// </returns>
			public string Serialize(byte[] secret, DateTime expiresUtc, bool privateAssociation) {
				return this.AssociationStore.Serialize(secret, expiresUtc, privateAssociation);
			}

			/// <summary>
			/// Retrieves an association given an association handle.
			/// </summary>
			/// <param name="containingMessage">The OpenID message that referenced this association handle.</param>
			/// <param name="isPrivateAssociation">A value indicating whether a private association is expected.</param>
			/// <param name="handle">The association handle.</param>
			/// <returns>
			/// An association instance, or <c>null</c> if the association has expired or the signature is incorrect (which may be because the OP's symmetric key has changed).
			/// </returns>
			/// <exception cref="ProtocolException">Thrown if the association is not of the expected type.</exception>
			public Association Deserialize(IProtocolMessage containingMessage, bool isPrivateAssociation, string handle) {
				return this.AssociationStore.Deserialize(containingMessage, isPrivateAssociation, handle);
			}
		}
	}
}
