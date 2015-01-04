//-----------------------------------------------------------------------
// <copyright file="OpenIdRelyingParty.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Collections.Specialized;
	using System.ComponentModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Net.Mime;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Messages;
	using Validation;

	/// <summary>
	/// A delegate that decides whether a given OpenID Provider endpoint may be
	/// considered for authenticating a user.
	/// </summary>
	/// <param name="endpoint">The endpoint for consideration.</param>
	/// <returns>
	/// <c>True</c> if the endpoint should be considered.  
	/// <c>False</c> to remove it from the pool of acceptable providers.
	/// </returns>
	public delegate bool EndpointSelector(IProviderEndpoint endpoint);

	/// <summary>
	/// Provides the programmatic facilities to act as an OpenID relying party.
	/// </summary>
	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Unavoidable")]
	public class OpenIdRelyingParty : IDisposable, IOpenIdHost {
		/// <summary>
		/// The name of the key to use in the HttpApplication cache to store the
		/// instance of <see cref="MemoryCryptoKeyAndNonceStore"/> to use.
		/// </summary>
		private const string ApplicationStoreKey = "DotNetOpenAuth.OpenId.RelyingParty.OpenIdRelyingParty.HttpApplicationStore";

		/// <summary>
		/// Backing store for the <see cref="Behaviors"/> property.
		/// </summary>
		private readonly ObservableCollection<IRelyingPartyBehavior> behaviors = new ObservableCollection<IRelyingPartyBehavior>();

		/// <summary>
		/// The discovery services to use for identifiers.
		/// </summary>
		private readonly IdentifierDiscoveryServices discoveryServices;

		/// <summary>
		/// Backing field for the <see cref="NonVerifyingRelyingParty"/> property.
		/// </summary>
		private OpenIdRelyingParty nonVerifyingRelyingParty;

		/// <summary>
		/// The lock to obtain when initializing the <see cref="nonVerifyingRelyingParty"/> member.
		/// </summary>
		private object nonVerifyingRelyingPartyInitLock = new object();

		/// <summary>
		/// A dictionary of extension response types and the javascript member 
		/// name to map them to on the user agent.
		/// </summary>
		private Dictionary<Type, string> clientScriptExtensions = new Dictionary<Type, string>();

		/// <summary>
		/// Backing field for the <see cref="SecuritySettings"/> property.
		/// </summary>
		private RelyingPartySecuritySettings securitySettings;

		/// <summary>
		/// Backing store for the <see cref="EndpointOrder"/> property.
		/// </summary>
		private Comparison<IdentifierDiscoveryResult> endpointOrder = DefaultEndpointOrder;

		/// <summary>
		/// Backing field for the <see cref="Channel"/> property.
		/// </summary>
		private Channel channel;

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdRelyingParty"/> class
		/// such that it uses a memory store for things it must remember across logins.
		/// </summary>
		public OpenIdRelyingParty()
			: this(OpenIdElement.Configuration.RelyingParty.ApplicationStore.CreateInstance(GetHttpApplicationStore(), null)) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdRelyingParty" /> class.
		/// </summary>
		/// <param name="applicationStore">The application store.  If <c>null</c>, the relying party will always operate in "stateless/dumb mode".</param>
		/// <param name="hostFactories">The host factories.</param>
		public OpenIdRelyingParty(ICryptoKeyAndNonceStore applicationStore, IHostFactories hostFactories = null)
			: this(applicationStore, applicationStore, hostFactories) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdRelyingParty" /> class.
		/// </summary>
		/// <param name="cryptoKeyStore">The association store.  If <c>null</c>, the relying party will always operate in "stateless/dumb mode".</param>
		/// <param name="nonceStore">The nonce store to use.  If <c>null</c>, the relying party will always operate in "stateless/dumb mode".</param>
		/// <param name="hostFactories">The host factories.</param>
		[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Unavoidable")]
		private OpenIdRelyingParty(ICryptoKeyStore cryptoKeyStore, INonceStore nonceStore, IHostFactories hostFactories) {
			// If we are a smart-mode RP (supporting associations), then we MUST also be 
			// capable of storing nonces to prevent replay attacks.
			// If we're a dumb-mode RP, then 2.0 OPs are responsible for preventing replays.
			Requires.That(cryptoKeyStore == null || nonceStore != null, null, OpenIdStrings.AssociationStoreRequiresNonceStore);

			this.securitySettings = OpenIdElement.Configuration.RelyingParty.SecuritySettings.CreateSecuritySettings();

			this.behaviors.CollectionChanged += this.OnBehaviorsChanged;
			foreach (var behavior in OpenIdElement.Configuration.RelyingParty.Behaviors.CreateInstances(false, null)) {
				this.behaviors.Add(behavior);
			}

			// Without a nonce store, we must rely on the Provider to protect against
			// replay attacks.  But only 2.0+ Providers can be expected to provide 
			// replay protection.
			if (nonceStore == null &&
				this.SecuritySettings.ProtectDownlevelReplayAttacks &&
				this.SecuritySettings.MinimumRequiredOpenIdVersion < ProtocolVersion.V20) {
				Logger.OpenId.Warn("Raising minimum OpenID version requirement for Providers to 2.0 to protect this stateless RP from replay attacks.");
				this.SecuritySettings.MinimumRequiredOpenIdVersion = ProtocolVersion.V20;
			}

			this.channel = new OpenIdRelyingPartyChannel(cryptoKeyStore, nonceStore, this.SecuritySettings, hostFactories);
			var associationStore = cryptoKeyStore != null ? new CryptoKeyStoreAsRelyingPartyAssociationStore(cryptoKeyStore) : null;
			this.AssociationManager = new AssociationManager(this.Channel, associationStore, this.SecuritySettings);
			this.discoveryServices = new IdentifierDiscoveryServices(this);

			Reporting.RecordFeatureAndDependencyUse(this, cryptoKeyStore, nonceStore);
		}

		/// <summary>
		/// Gets an XRDS sorting routine that uses the XRDS Service/@Priority 
		/// attribute to determine order.
		/// </summary>
		/// <remarks>
		/// Endpoints lacking any priority value are sorted to the end of the list.
		/// </remarks>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static Comparison<IdentifierDiscoveryResult> DefaultEndpointOrder {
			get { return IdentifierDiscoveryResult.EndpointOrder; }
		}

		/// <summary>
		/// Gets or sets the channel to use for sending/receiving messages.
		/// </summary>
		public Channel Channel {
			get {
				return this.channel;
			}

			set {
				Requires.NotNull(value, "value");
				this.channel = value;
				this.AssociationManager.Channel = value;
			}
		}

		/// <summary>
		/// Gets the security settings used by this Relying Party.
		/// </summary>
		public RelyingPartySecuritySettings SecuritySettings {
			get {
				return this.securitySettings;
			}

			internal set {
				Requires.NotNull(value, "value");
				this.securitySettings = value;
				this.AssociationManager.SecuritySettings = value;
			}
		}

		/// <summary>
		/// Gets the security settings.
		/// </summary>
		SecuritySettings IOpenIdHost.SecuritySettings {
			get { return this.SecuritySettings; }
		}

		/// <summary>
		/// Gets or sets the optional Provider Endpoint filter to use.
		/// </summary>
		/// <remarks>
		/// Provides a way to optionally filter the providers that may be used in authenticating a user.
		/// If provided, the delegate should return true to accept an endpoint, and false to reject it.
		/// If null, all identity providers will be accepted.  This is the default.
		/// </remarks>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public EndpointSelector EndpointFilter { get; set; }

		/// <summary>
		/// Gets or sets the ordering routine that will determine which XRDS 
		/// Service element to try first 
		/// </summary>
		/// <value>Default is <see cref="DefaultEndpointOrder"/>.</value>
		/// <remarks>
		/// This may never be null.  To reset to default behavior this property 
		/// can be set to the value of <see cref="DefaultEndpointOrder"/>.
		/// </remarks>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public Comparison<IdentifierDiscoveryResult> EndpointOrder {
			get {
				return this.endpointOrder;
			}

			set {
				Requires.NotNull(value, "value");
				this.endpointOrder = value;
			}
		}

		/// <summary>
		/// Gets the extension factories.
		/// </summary>
		public IList<IOpenIdExtensionFactory> ExtensionFactories {
			get { return this.Channel.GetExtensionFactories(); }
		}

		/// <summary>
		/// Gets a list of custom behaviors to apply to OpenID actions.
		/// </summary>
		/// <remarks>
		/// Adding behaviors can impact the security settings of this <see cref="OpenIdRelyingParty"/>
		/// instance in ways that subsequently removing the behaviors will not reverse.
		/// </remarks>
		public ICollection<IRelyingPartyBehavior> Behaviors {
			get { return this.behaviors; }
		}

		/// <summary>
		/// Gets the list of services that can perform discovery on identifiers given to this relying party.
		/// </summary>
		public IList<IIdentifierDiscoveryService> DiscoveryServices {
			get { return this.discoveryServices.DiscoveryServices; }
		}

		/// <summary>
		/// Gets the factory for various dependencies.
		/// </summary>
		IHostFactories IOpenIdHost.HostFactories {
			get { return this.channel.HostFactories; }
		}

		/// <summary>
		/// Gets a value indicating whether this Relying Party can sign its return_to
		/// parameter in outgoing authentication requests.
		/// </summary>
		internal bool CanSignCallbackArguments {
			get { return this.Channel.BindingElements.OfType<ReturnToSignatureBindingElement>().Any(); }
		}

		/// <summary>
		/// Gets the association manager.
		/// </summary>
		internal AssociationManager AssociationManager { get; private set; }

		/// <summary>
		/// Gets the <see cref="OpenIdRelyingParty"/> instance used to process authentication responses
		/// without verifying the assertion or consuming nonces.
		/// </summary>
		protected OpenIdRelyingParty NonVerifyingRelyingParty {
			get {
				if (this.nonVerifyingRelyingParty == null) {
					lock (this.nonVerifyingRelyingPartyInitLock) {
						if (this.nonVerifyingRelyingParty == null) {
							this.nonVerifyingRelyingParty = OpenIdRelyingParty.CreateNonVerifying();
						}
					}
				}

				return this.nonVerifyingRelyingParty;
			}
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
		/// Creates an authentication request to verify that a user controls
		/// some given Identifier.
		/// </summary>
		/// <param name="userSuppliedIdentifier">The Identifier supplied by the user.  This may be a URL, an XRI or i-name.</param>
		/// <param name="realm">The shorest URL that describes this relying party web site's address.
		/// For example, if your login page is found at https://www.example.com/login.aspx,
		/// your realm would typically be https://www.example.com/.</param>
		/// <param name="returnToUrl">The URL of the login page, or the page prepared to receive authentication
		/// responses from the OpenID Provider.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// An authentication request object to customize the request and generate
		/// an object to send to the user agent to initiate the authentication.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown if no OpenID endpoint could be found.</exception>
		public async Task<IAuthenticationRequest> CreateRequestAsync(Identifier userSuppliedIdentifier, Realm realm, Uri returnToUrl, CancellationToken cancellationToken = default(CancellationToken)) {
			Requires.NotNull(userSuppliedIdentifier, "userSuppliedIdentifier");
			Requires.NotNull(realm, "realm");
			Requires.NotNull(returnToUrl, "returnToUrl");
			try {
				var requests = await this.CreateRequestsAsync(userSuppliedIdentifier, realm, returnToUrl);
				return requests.First();
			} catch (InvalidOperationException ex) {
				throw ErrorUtilities.Wrap(ex, OpenIdStrings.OpenIdEndpointNotFound);
			}
		}

		/// <summary>
		/// Creates an authentication request to verify that a user controls
		/// some given Identifier.
		/// </summary>
		/// <param name="userSuppliedIdentifier">The Identifier supplied by the user.  This may be a URL, an XRI or i-name.</param>
		/// <param name="realm">The shorest URL that describes this relying party web site's address.
		/// For example, if your login page is found at https://www.example.com/login.aspx,
		/// your realm would typically be https://www.example.com/.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// An authentication request object that describes the HTTP response to
		/// send to the user agent to initiate the authentication.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown if no OpenID endpoint could be found.</exception>
		/// <exception cref="InvalidOperationException">Thrown if <see cref="HttpContext.Current">HttpContext.Current</see> == <c>null</c>.</exception>
		/// <remarks>
		/// Requires an <see cref="HttpContext.Current">HttpContext.Current</see> context.
		/// </remarks>
		public async Task<IAuthenticationRequest> CreateRequestAsync(Identifier userSuppliedIdentifier, Realm realm, CancellationToken cancellationToken = default(CancellationToken)) {
			Requires.NotNull(userSuppliedIdentifier, "userSuppliedIdentifier");
			Requires.NotNull(realm, "realm");
			try {
				var request = await this.CreateRequestsAsync(userSuppliedIdentifier, realm, cancellationToken: cancellationToken);
				var result = request.First();
				Assumes.True(result != null);
				return result;
			} catch (InvalidOperationException ex) {
				throw ErrorUtilities.Wrap(ex, OpenIdStrings.OpenIdEndpointNotFound);
			}
		}

		/// <summary>
		/// Creates an authentication request to verify that a user controls
		/// some given Identifier.
		/// </summary>
		/// <param name="userSuppliedIdentifier">The Identifier supplied by the user.  This may be a URL, an XRI or i-name.</param>
		/// <param name="requestContext">The request context.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// An authentication request object that describes the HTTP response to
		/// send to the user agent to initiate the authentication.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown if no OpenID endpoint could be found.</exception>
		/// <exception cref="InvalidOperationException">Thrown if <see cref="HttpContext.Current">HttpContext.Current</see> == <c>null</c>.</exception>
		/// <remarks>
		/// Requires an <see cref="HttpContext.Current">HttpContext.Current</see> context.
		/// </remarks>
		public async Task<IAuthenticationRequest> CreateRequestAsync(Identifier userSuppliedIdentifier, HttpRequestBase requestContext = null, CancellationToken cancellationToken = default(CancellationToken)) {
			Requires.NotNull(userSuppliedIdentifier, "userSuppliedIdentifier");
			try {
				var authenticationRequests = await this.CreateRequestsAsync(userSuppliedIdentifier, requestContext, cancellationToken);
				return authenticationRequests.First();
			} catch (InvalidOperationException ex) {
				throw ErrorUtilities.Wrap(ex, OpenIdStrings.OpenIdEndpointNotFound);
			}
		}

		/// <summary>
		/// Generates the authentication requests that can satisfy the requirements of some OpenID Identifier.
		/// </summary>
		/// <param name="userSuppliedIdentifier">The Identifier supplied by the user.  This may be a URL, an XRI or i-name.</param>
		/// <param name="realm">The shorest URL that describes this relying party web site's address.
		/// For example, if your login page is found at https://www.example.com/login.aspx,
		/// your realm would typically be https://www.example.com/.</param>
		/// <param name="returnToUrl">The URL of the login page, or the page prepared to receive authentication
		/// responses from the OpenID Provider.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A sequence of authentication requests, any of which constitutes a valid identity assertion on the Claimed Identifier.
		/// Never null, but may be empty.
		/// </returns>
		/// <remarks>
		///   <para>Any individual generated request can satisfy the authentication.
		/// The generated requests are sorted in preferred order.
		/// Each request is generated as it is enumerated to.  Associations are created only as
		///   <see cref="IAuthenticationRequest.GetRedirectingResponseAsync" /> is called.</para>
		///   <para>No exception is thrown if no OpenID endpoints were discovered.
		/// An empty enumerable is returned instead.</para>
		/// </remarks>
		public virtual async Task<IEnumerable<IAuthenticationRequest>> CreateRequestsAsync(Identifier userSuppliedIdentifier, Realm realm, Uri returnToUrl, CancellationToken cancellationToken = default(CancellationToken)) {
			Requires.NotNull(userSuppliedIdentifier, "userSuppliedIdentifier");
			Requires.NotNull(realm, "realm");
			Requires.NotNull(returnToUrl, "returnToUrl");

			var requests = await AuthenticationRequest.CreateAsync(userSuppliedIdentifier, this, realm, returnToUrl, true, cancellationToken);
			return requests.Cast<IAuthenticationRequest>().CacheGeneratedResults();
		}

		/// <summary>
		/// Generates the authentication requests that can satisfy the requirements of some OpenID Identifier.
		/// </summary>
		/// <param name="userSuppliedIdentifier">The Identifier supplied by the user.  This may be a URL, an XRI or i-name.</param>
		/// <param name="realm">The shorest URL that describes this relying party web site's address.
		/// For example, if your login page is found at https://www.example.com/login.aspx,
		/// your realm would typically be https://www.example.com/.</param>
		/// <param name="requestContext">The request context.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A sequence of authentication requests, any of which constitutes a valid identity assertion on the Claimed Identifier.
		/// Never null, but may be empty.
		/// </returns>
		/// <exception cref="InvalidOperationException">Thrown if <see cref="HttpContext.Current">HttpContext.Current</see> == <c>null</c>.</exception>
		/// <remarks>
		///   <para>Any individual generated request can satisfy the authentication.
		/// The generated requests are sorted in preferred order.
		/// Each request is generated as it is enumerated to.  Associations are created only as
		///   <see cref="IAuthenticationRequest.GetRedirectingResponseAsync" /> is called.</para>
		///   <para>No exception is thrown if no OpenID endpoints were discovered.
		/// An empty enumerable is returned instead.</para>
		///   <para>Requires an <see cref="HttpContext.Current">HttpContext.Current</see> context.</para>
		/// </remarks>
		public async Task<IEnumerable<IAuthenticationRequest>> CreateRequestsAsync(Identifier userSuppliedIdentifier, Realm realm, HttpRequestBase requestContext = null, CancellationToken cancellationToken = default(CancellationToken)) {
			RequiresEx.ValidState(requestContext != null || (HttpContext.Current != null && HttpContext.Current.Request != null), MessagingStrings.HttpContextRequired);
			Requires.NotNull(userSuppliedIdentifier, "userSuppliedIdentifier");
			Requires.NotNull(realm, "realm");

			requestContext = requestContext ?? this.channel.GetRequestFromContext();

			// This next code contract is a BAD idea, because it causes each authentication request to be generated
			// at least an extra time.
			////
			// Build the return_to URL
			UriBuilder returnTo = new UriBuilder(requestContext.GetPublicFacingUrl());

			// Trim off any parameters with an "openid." prefix, and a few known others
			// to avoid carrying state from a prior login attempt.
			returnTo.Query = string.Empty;
			NameValueCollection queryParams = requestContext.GetQueryStringBeforeRewriting();
			var returnToParams = new Dictionary<string, string>(queryParams.Count);
			foreach (string key in queryParams) {
				if (!IsOpenIdSupportingParameter(key) && key != null) {
					returnToParams.Add(key, queryParams[key]);
				}
			}

			returnTo.AppendQueryArgs(returnToParams);

			return await this.CreateRequestsAsync(userSuppliedIdentifier, realm, returnTo.Uri, cancellationToken);
		}

		/// <summary>
		/// Generates the authentication requests that can satisfy the requirements of some OpenID Identifier.
		/// </summary>
		/// <param name="userSuppliedIdentifier">The Identifier supplied by the user.  This may be a URL, an XRI or i-name.</param>
		/// <param name="requestContext">The request context.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A sequence of authentication requests, any of which constitutes a valid identity assertion on the Claimed Identifier.
		/// Never null, but may be empty.
		/// </returns>
		/// <exception cref="InvalidOperationException">Thrown if <see cref="HttpContext.Current">HttpContext.Current</see> == <c>null</c>.</exception>
		/// <remarks>
		///   <para>Any individual generated request can satisfy the authentication.
		/// The generated requests are sorted in preferred order.
		/// Each request is generated as it is enumerated to.  Associations are created only as
		///   <see cref="IAuthenticationRequest.GetRedirectingResponseAsync" /> is called.</para>
		///   <para>No exception is thrown if no OpenID endpoints were discovered.
		/// An empty enumerable is returned instead.</para>
		///   <para>Requires an <see cref="HttpContext.Current">HttpContext.Current</see> context.</para>
		/// </remarks>
		public async Task<IEnumerable<IAuthenticationRequest>> CreateRequestsAsync(Identifier userSuppliedIdentifier, HttpRequestBase requestContext = null, CancellationToken cancellationToken = default(CancellationToken)) {
			Requires.NotNull(userSuppliedIdentifier, "userSuppliedIdentifier");

			return await this.CreateRequestsAsync(userSuppliedIdentifier, Realm.AutoDetect, requestContext, cancellationToken);
		}

		/// <summary>
		/// Gets an authentication response from a Provider.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The processed authentication response if there is any; <c>null</c> otherwise.
		/// </returns>
		/// <remarks>
		/// Requires an <see cref="HttpContext.Current">HttpContext.Current</see> context.
		/// </remarks>
		public Task<IAuthenticationResponse> GetResponseAsync(HttpRequestBase request = null, CancellationToken cancellationToken = default(CancellationToken)) {
			request = request ?? this.channel.GetRequestFromContext();
			return this.GetResponseAsync(request.AsHttpRequestMessage(), cancellationToken);
		}

		/// <summary>
		/// Gets an authentication response from a Provider.
		/// </summary>
		/// <param name="request">The HTTP request that may be carrying an authentication response from the Provider.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The processed authentication response if there is any; <c>null</c> otherwise.
		/// </returns>
		public async Task<IAuthenticationResponse> GetResponseAsync(HttpRequestMessage request, CancellationToken cancellationToken = default(CancellationToken)) {
			Requires.NotNull(request, "httpRequestInfo");
			try {
				var message = await this.Channel.ReadFromRequestAsync(request, cancellationToken);
				PositiveAssertionResponse positiveAssertion;
				NegativeAssertionResponse negativeAssertion;
				IndirectSignedResponse positiveExtensionOnly;
				if ((positiveAssertion = message as PositiveAssertionResponse) != null) {
					// We need to make sure that this assertion is coming from an endpoint
					// that the host deems acceptable.
					var providerEndpoint = new SimpleXrdsProviderEndpoint(positiveAssertion);
					ErrorUtilities.VerifyProtocol(
						this.FilterEndpoint(providerEndpoint),
						OpenIdStrings.PositiveAssertionFromNonQualifiedProvider,
						providerEndpoint.Uri);

					var response = await PositiveAuthenticationResponse.CreateAsync(positiveAssertion, this, cancellationToken);
					foreach (var behavior in this.Behaviors) {
						behavior.OnIncomingPositiveAssertion(response);
					}

					return response;
				} else if ((positiveExtensionOnly = message as IndirectSignedResponse) != null) {
					return new PositiveAnonymousResponse(positiveExtensionOnly);
				} else if ((negativeAssertion = message as NegativeAssertionResponse) != null) {
					return new NegativeAuthenticationResponse(negativeAssertion);
				} else if (message != null) {
					Logger.OpenId.WarnFormat("Received unexpected message type {0} when expecting an assertion message.", message.GetType().Name);
				}

				return null;
			} catch (ProtocolException ex) {
				return new FailedAuthenticationResponse(ex);
			}
		}

		/// <summary>
		/// Processes the response received in a popup window or iframe to an AJAX-directed OpenID authentication.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The HTTP response to send to this HTTP request.
		/// </returns>
		/// <remarks>
		/// Requires an <see cref="HttpContext.Current">HttpContext.Current</see> context.
		/// </remarks>
		public Task<HttpResponseMessage> ProcessResponseFromPopupAsync(HttpRequestBase request = null, CancellationToken cancellationToken = default(CancellationToken)) {
			request = request ?? this.Channel.GetRequestFromContext();
			return this.ProcessResponseFromPopupAsync(request.AsHttpRequestMessage(), cancellationToken);
		}

		/// <summary>
		/// Processes the response received in a popup window or iframe to an AJAX-directed OpenID authentication.
		/// </summary>
		/// <param name="request">The incoming HTTP request that is expected to carry an OpenID authentication response.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The HTTP response to send to this HTTP request.
		/// </returns>
		public Task<HttpResponseMessage> ProcessResponseFromPopupAsync(HttpRequestMessage request, CancellationToken cancellationToken = default(CancellationToken)) {
			Requires.NotNull(request, "request");
			return this.ProcessResponseFromPopupAsync(request, null, cancellationToken);
		}

		/// <summary>
		/// Allows an OpenID extension to read data out of an unverified positive authentication assertion
		/// and send it down to the client browser so that Javascript running on the page can perform
		/// some preprocessing on the extension data.
		/// </summary>
		/// <typeparam name="T">The extension <i>response</i> type that will read data from the assertion.</typeparam>
		/// <param name="propertyName">The property name on the openid_identifier input box object that will be used to store the extension data.  For example: sreg</param>
		/// <remarks>
		/// This method should be called before <see cref="ProcessResponseFromPopupAsync(HttpRequestMessage, CancellationToken)"/>.
		/// </remarks>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design")]
		public void RegisterClientScriptExtension<T>(string propertyName) where T : IClientScriptExtensionResponse {
			Requires.NotNullOrEmpty(propertyName, "propertyName");
			ErrorUtilities.VerifyArgumentNamed(!this.clientScriptExtensions.ContainsValue(propertyName), "propertyName", OpenIdStrings.ClientScriptExtensionPropertyNameCollision, propertyName);
			foreach (var ext in this.clientScriptExtensions.Keys) {
				ErrorUtilities.VerifyArgument(ext != typeof(T), OpenIdStrings.ClientScriptExtensionTypeCollision, typeof(T).FullName);
			}
			this.clientScriptExtensions.Add(typeof(T), propertyName);
		}

		#region IDisposable Members

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		/// <summary>
		/// Determines whether some parameter name belongs to OpenID or this library
		/// as a protocol or internal parameter name.
		/// </summary>
		/// <param name="parameterName">Name of the parameter.</param>
		/// <returns>
		/// 	<c>true</c> if the named parameter is a library- or protocol-specific parameter; otherwise, <c>false</c>.
		/// </returns>
		internal static bool IsOpenIdSupportingParameter(string parameterName) {
			// Yes, it is possible with some query strings to have a null or empty parameter name
			if (string.IsNullOrEmpty(parameterName)) {
				return false;
			}

			Protocol protocol = Protocol.Default;
			return parameterName.StartsWith(protocol.openid.Prefix, StringComparison.OrdinalIgnoreCase)
				|| parameterName.StartsWith(OpenIdUtilities.CustomParameterPrefix, StringComparison.Ordinal);
		}

		/// <summary>
		/// Creates a relying party that does not verify incoming messages against
		/// nonce or association stores.  
		/// </summary>
		/// <returns>The instantiated <see cref="OpenIdRelyingParty"/>.</returns>
		/// <remarks>
		/// Useful for previewing messages while
		/// allowing them to be fully processed and verified later.
		/// </remarks>
		internal static OpenIdRelyingParty CreateNonVerifying() {
			OpenIdRelyingParty rp = new OpenIdRelyingParty();
			try {
				rp.Channel = OpenIdRelyingPartyChannel.CreateNonVerifyingChannel();
				return rp;
			} catch {
				rp.Dispose();
				throw;
			}
		}

		/// <summary>
		/// Processes the response received in a popup window or iframe to an AJAX-directed OpenID authentication.
		/// </summary>
		/// <param name="request">The incoming HTTP request that is expected to carry an OpenID authentication response.</param>
		/// <param name="callback">The callback fired after the response status has been determined but before the Javascript response is formulated.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The HTTP response to send to this HTTP request.
		/// </returns>
		[SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "OpenID", Justification = "real word"), SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "iframe", Justification = "Code contracts")]
		internal async Task<HttpResponseMessage> ProcessResponseFromPopupAsync(HttpRequestMessage request, Action<AuthenticationStatus> callback, CancellationToken cancellationToken) {
			Requires.NotNull(request, "request");

			string extensionsJson = null;
			var authResponse = await this.NonVerifyingRelyingParty.GetResponseAsync(request, cancellationToken);
			ErrorUtilities.VerifyProtocol(authResponse != null, OpenIdStrings.PopupRedirectMissingResponse);

			// Give the caller a chance to notify the hosting page and fill up the clientScriptExtensions collection.
			if (callback != null) {
				callback(authResponse.Status);
			}

			Logger.OpenId.DebugFormat("Popup or iframe callback from OP: {0}", request.RequestUri);
			Logger.Controls.DebugFormat(
				"An authentication response was found in a popup window or iframe using a non-verifying RP with status: {0}",
				authResponse.Status);
			if (authResponse.Status == AuthenticationStatus.Authenticated) {
				var extensionsDictionary = new Dictionary<string, string>();
				foreach (var pair in this.clientScriptExtensions) {
					IClientScriptExtensionResponse extension = (IClientScriptExtensionResponse)authResponse.GetExtension(pair.Key);
					if (extension == null) {
						continue;
					}
					var positiveResponse = (PositiveAuthenticationResponse)authResponse;
					string js = extension.InitializeJavaScriptData(positiveResponse.Response);
					if (!string.IsNullOrEmpty(js)) {
						extensionsDictionary[pair.Value] = js;
					}
				}

				extensionsJson = MessagingUtilities.CreateJsonObject(extensionsDictionary, true);
			}

			string payload = "document.URL";
			if (request.Method == HttpMethod.Post) {
				// Promote all form variables to the query string, but since it won't be passed
				// to any server (this is a javascript window-to-window transfer) the length of
				// it can be arbitrarily long, whereas it was POSTed here probably because it
				// was too long for HTTP transit.
				UriBuilder payloadUri = new UriBuilder(request.RequestUri);
				payloadUri.AppendQueryArgs(await Channel.ParseUrlEncodedFormContentAsync(request, cancellationToken));
				payload = MessagingUtilities.GetSafeJavascriptValue(payloadUri.Uri.AbsoluteUri);
			}

			if (!string.IsNullOrEmpty(extensionsJson)) {
				payload += ", " + extensionsJson;
			}

			return InvokeParentPageScript("dnoa_internal.processAuthorizationResult(" + payload + ")");
		}

		/// <summary>
		/// Performs discovery on the specified identifier.
		/// </summary>
		/// <param name="identifier">The identifier to discover services for.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A non-null sequence of services discovered for the identifier.
		/// </returns>
		internal Task<IEnumerable<IdentifierDiscoveryResult>> DiscoverAsync(Identifier identifier, CancellationToken cancellationToken) {
			return this.discoveryServices.DiscoverAsync(identifier, cancellationToken);
		}

		/// <summary>
		/// Checks whether a given OP Endpoint is permitted by the host relying party.
		/// </summary>
		/// <param name="endpoint">The OP endpoint.</param>
		/// <returns><c>true</c> if the OP Endpoint is allowed; <c>false</c> otherwise.</returns>
		protected internal bool FilterEndpoint(IProviderEndpoint endpoint) {
			if (this.SecuritySettings.RejectAssertionsFromUntrustedProviders) {
				if (!this.SecuritySettings.TrustedProviderEndpoints.Contains(endpoint.Uri)) {
					Logger.OpenId.InfoFormat("Filtering out OP endpoint {0} because it is not on the exclusive trusted provider whitelist.", endpoint.Uri.AbsoluteUri);
					return false;
				}
			}

			if (endpoint.Version < Protocol.Lookup(this.SecuritySettings.MinimumRequiredOpenIdVersion).Version) {
				Logger.OpenId.InfoFormat(
					"Filtering out OP endpoint {0} because it implements OpenID {1} but this relying party requires OpenID {2} or later.",
					endpoint.Uri.AbsoluteUri,
					endpoint.Version,
					Protocol.Lookup(this.SecuritySettings.MinimumRequiredOpenIdVersion).Version);
				return false;
			}

			if (this.EndpointFilter != null) {
				if (!this.EndpointFilter(endpoint)) {
					Logger.OpenId.InfoFormat("Filtering out OP endpoint {0} because the host rejected it.", endpoint.Uri.AbsoluteUri);
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				if (this.nonVerifyingRelyingParty != null) {
					this.nonVerifyingRelyingParty.Dispose();
					this.nonVerifyingRelyingParty = null;
				}

				// Tear off the instance member as a local variable for thread safety.
				IDisposable disposableChannel = this.channel as IDisposable;
				if (disposableChannel != null) {
					disposableChannel.Dispose();
				}
			}
		}

		/// <summary>
		/// Invokes a method on a parent frame or window and closes the calling popup window if applicable.
		/// </summary>
		/// <param name="methodCall">The method to call on the parent window, including
		/// parameters.  (i.e. "callback('arg1', 2)").  No escaping is done by this method.</param>
		/// <returns>The entire HTTP response to send to the popup window or iframe to perform the invocation.</returns>
		private static HttpResponseMessage InvokeParentPageScript(string methodCall) {
			Requires.NotNullOrEmpty(methodCall, "methodCall");

			Logger.OpenId.DebugFormat("Sending Javascript callback: {0}", methodCall);
			StringBuilder builder = new StringBuilder();
			builder.AppendLine("<html><body><script type='text/javascript' language='javascript'><!--");
			builder.AppendLine("//<![CDATA[");
			builder.Append(@"	var inPopup = !window.frameElement;
	var objSrc = inPopup ? window.opener : window.frameElement;
");

			// Something about calling objSrc.{0} can somehow cause FireFox to forget about the inPopup variable,
			// so we have to actually put the test for it ABOVE the call to objSrc.{0} so that it already 
			// whether to call window.self.close() after the call.
			string htmlFormat = @"	if (inPopup) {{
		try {{
			objSrc.{0};
		}} catch (ex) {{
			alert(ex);
		}} finally {{
			window.self.close();
		}}
	}} else {{
		objSrc.{0};
	}}";
			builder.AppendFormat(CultureInfo.InvariantCulture, htmlFormat, methodCall);
			builder.AppendLine("//]]>--></script>");
			builder.AppendLine("</body></html>");

			var response = new HttpResponseMessage();
			response.Content = new StringContent(builder.ToString());
			response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
			return response;
		}

		/// <summary>
		/// Called by derived classes when behaviors are added or removed.
		/// </summary>
		/// <param name="sender">The collection being modified.</param>
		/// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
		private void OnBehaviorsChanged(object sender, NotifyCollectionChangedEventArgs e) {
			foreach (IRelyingPartyBehavior profile in e.NewItems) {
				profile.ApplySecuritySettings(this.SecuritySettings);
				Reporting.RecordFeatureUse(profile);
			}
		}

#if CONTRACTS_FULL
		/// <summary>
		/// Verifies conditions that should be true for any valid state of this object.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Called by code contracts.")]
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called by code contracts.")]
		[ContractInvariantMethod]
		private void ObjectInvariant() {
			Contract.Invariant(this.SecuritySettings != null);
			Contract.Invariant(this.Channel != null);
			Contract.Invariant(this.EndpointOrder != null);
		}
#endif
	}
}
