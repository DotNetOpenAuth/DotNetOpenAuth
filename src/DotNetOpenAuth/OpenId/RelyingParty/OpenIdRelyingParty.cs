//-----------------------------------------------------------------------
// <copyright file="OpenIdRelyingParty.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Collections.Specialized;
	using System.ComponentModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.Linq;
	using System.Net;
	using System.Net.Mime;
	using System.Text;
	using System.Web;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Messages;

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
	[ContractVerification(true)]
	public class OpenIdRelyingParty : IDisposable {
		/// <summary>
		/// The name of the key to use in the HttpApplication cache to store the
		/// instance of <see cref="StandardRelyingPartyApplicationStore"/> to use.
		/// </summary>
		private const string ApplicationStoreKey = "DotNetOpenAuth.OpenId.RelyingParty.OpenIdRelyingParty.HttpApplicationStore";

		/// <summary>
		/// Backing store for the <see cref="Behaviors"/> property.
		/// </summary>
		private readonly ObservableCollection<IRelyingPartyBehavior> behaviors = new ObservableCollection<IRelyingPartyBehavior>();

		/// <summary>
		/// Backing field for the <see cref="DiscoveryServices"/> property.
		/// </summary>
		private readonly IList<IIdentifierDiscoveryService> discoveryServices = new List<IIdentifierDiscoveryService>(2);

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
		/// Initializes a new instance of the <see cref="OpenIdRelyingParty"/> class.
		/// </summary>
		public OpenIdRelyingParty()
			: this(DotNetOpenAuthSection.Configuration.OpenId.RelyingParty.ApplicationStore.CreateInstance(HttpApplicationStore)) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdRelyingParty"/> class.
		/// </summary>
		/// <param name="applicationStore">The application store.  If <c>null</c>, the relying party will always operate in "dumb mode".</param>
		public OpenIdRelyingParty(IRelyingPartyApplicationStore applicationStore)
			: this(applicationStore, applicationStore) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdRelyingParty"/> class.
		/// </summary>
		/// <param name="associationStore">The association store.  If null, the relying party will always operate in "dumb mode".</param>
		/// <param name="nonceStore">The nonce store to use.  If null, the relying party will always operate in "dumb mode".</param>
		private OpenIdRelyingParty(IAssociationStore<Uri> associationStore, INonceStore nonceStore) {
			// If we are a smart-mode RP (supporting associations), then we MUST also be 
			// capable of storing nonces to prevent replay attacks.
			// If we're a dumb-mode RP, then 2.0 OPs are responsible for preventing replays.
			Contract.Requires<ArgumentException>(associationStore == null || nonceStore != null, OpenIdStrings.AssociationStoreRequiresNonceStore);

			this.securitySettings = DotNetOpenAuthSection.Configuration.OpenId.RelyingParty.SecuritySettings.CreateSecuritySettings();

			foreach (var discoveryService in DotNetOpenAuthSection.Configuration.OpenId.RelyingParty.DiscoveryServices.CreateInstances(true)) {
				this.discoveryServices.Add(discoveryService);
			}

			this.behaviors.CollectionChanged += this.OnBehaviorsChanged;
			foreach (var behavior in DotNetOpenAuthSection.Configuration.OpenId.RelyingParty.Behaviors.CreateInstances(false)) {
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

			this.channel = new OpenIdChannel(associationStore, nonceStore, this.SecuritySettings);
			this.AssociationManager = new AssociationManager(this.Channel, associationStore, this.SecuritySettings);

			Reporting.RecordFeatureAndDependencyUse(this, associationStore, nonceStore);
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
		/// Gets the standard state storage mechanism that uses ASP.NET's
		/// HttpApplication state dictionary to store associations and nonces.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static IRelyingPartyApplicationStore HttpApplicationStore {
			get {
				Contract.Ensures(Contract.Result<IRelyingPartyApplicationStore>() != null);

				HttpContext context = HttpContext.Current;
				ErrorUtilities.VerifyOperation(context != null, Strings.StoreRequiredWhenNoHttpContextAvailable, typeof(IRelyingPartyApplicationStore).Name);
				var store = (IRelyingPartyApplicationStore)context.Application[ApplicationStoreKey];
				if (store == null) {
					context.Application.Lock();
					try {
						if ((store = (IRelyingPartyApplicationStore)context.Application[ApplicationStoreKey]) == null) {
							context.Application[ApplicationStoreKey] = store = new StandardRelyingPartyApplicationStore();
						}
					} finally {
						context.Application.UnLock();
					}
				}

				return store;
			}
		}

		/// <summary>
		/// Gets or sets the channel to use for sending/receiving messages.
		/// </summary>
		public Channel Channel {
			get {
				return this.channel;
			}

			set {
				Contract.Requires<ArgumentNullException>(value != null);
				this.channel = value;
				this.AssociationManager.Channel = value;
			}
		}

		/// <summary>
		/// Gets the security settings used by this Relying Party.
		/// </summary>
		public RelyingPartySecuritySettings SecuritySettings {
			get {
				Contract.Ensures(Contract.Result<RelyingPartySecuritySettings>() != null);
				return this.securitySettings;
			}

			internal set {
				Contract.Requires<ArgumentNullException>(value != null);
				this.securitySettings = value;
				this.AssociationManager.SecuritySettings = value;
			}
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
				Contract.Requires<ArgumentNullException>(value != null);
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
			get { return this.discoveryServices; }
		}

		/// <summary>
		/// Gets a value indicating whether this Relying Party can sign its return_to
		/// parameter in outgoing authentication requests.
		/// </summary>
		internal bool CanSignCallbackArguments {
			get { return this.Channel.BindingElements.OfType<ReturnToSignatureBindingElement>().Any(); }
		}

		/// <summary>
		/// Gets the web request handler to use for discovery and the part of
		/// authentication where direct messages are sent to an untrusted remote party.
		/// </summary>
		internal IDirectWebRequestHandler WebRequestHandler {
			get { return this.Channel.WebRequestHandler; }
		}

		/// <summary>
		/// Gets the association manager.
		/// </summary>
		public AssociationManager AssociationManager { get; private set; }

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
		/// Creates an authentication request to verify that a user controls
		/// some given Identifier.
		/// </summary>
		/// <param name="userSuppliedIdentifier">
		/// The Identifier supplied by the user.  This may be a URL, an XRI or i-name.
		/// </param>
		/// <param name="realm">
		/// The shorest URL that describes this relying party web site's address.
		/// For example, if your login page is found at https://www.example.com/login.aspx,
		/// your realm would typically be https://www.example.com/.
		/// </param>
		/// <param name="returnToUrl">
		/// The URL of the login page, or the page prepared to receive authentication 
		/// responses from the OpenID Provider.
		/// </param>
		/// <returns>
		/// An authentication request object to customize the request and generate
		/// an object to send to the user agent to initiate the authentication.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown if no OpenID endpoint could be found.</exception>
		public IAuthenticationRequest CreateRequest(Identifier userSuppliedIdentifier, Realm realm, Uri returnToUrl) {
			Contract.Requires<ArgumentNullException>(userSuppliedIdentifier != null);
			Contract.Requires<ArgumentNullException>(realm != null);
			Contract.Requires<ArgumentNullException>(returnToUrl != null);
			Contract.Ensures(Contract.Result<IAuthenticationRequest>() != null);
			try {
				return this.CreateRequests(userSuppliedIdentifier, realm, returnToUrl).First();
			} catch (InvalidOperationException ex) {
				throw ErrorUtilities.Wrap(ex, OpenIdStrings.OpenIdEndpointNotFound);
			}
		}

		/// <summary>
		/// Creates an authentication request to verify that a user controls
		/// some given Identifier.
		/// </summary>
		/// <param name="userSuppliedIdentifier">
		/// The Identifier supplied by the user.  This may be a URL, an XRI or i-name.
		/// </param>
		/// <param name="realm">
		/// The shorest URL that describes this relying party web site's address.
		/// For example, if your login page is found at https://www.example.com/login.aspx,
		/// your realm would typically be https://www.example.com/.
		/// </param>
		/// <returns>
		/// An authentication request object that describes the HTTP response to
		/// send to the user agent to initiate the authentication.
		/// </returns>
		/// <remarks>
		/// <para>Requires an <see cref="HttpContext.Current">HttpContext.Current</see> context.</para>
		/// </remarks>
		/// <exception cref="ProtocolException">Thrown if no OpenID endpoint could be found.</exception>
		/// <exception cref="InvalidOperationException">Thrown if <see cref="HttpContext.Current">HttpContext.Current</see> == <c>null</c>.</exception>
		public IAuthenticationRequest CreateRequest(Identifier userSuppliedIdentifier, Realm realm) {
			Contract.Requires<ArgumentNullException>(userSuppliedIdentifier != null);
			Contract.Requires<ArgumentNullException>(realm != null);
			Contract.Ensures(Contract.Result<IAuthenticationRequest>() != null);
			try {
				var result = this.CreateRequests(userSuppliedIdentifier, realm).First();
				Contract.Assume(result != null);
				return result;
			} catch (InvalidOperationException ex) {
				throw ErrorUtilities.Wrap(ex, OpenIdStrings.OpenIdEndpointNotFound);
			}
		}

		/// <summary>
		/// Creates an authentication request to verify that a user controls
		/// some given Identifier.
		/// </summary>
		/// <param name="userSuppliedIdentifier">
		/// The Identifier supplied by the user.  This may be a URL, an XRI or i-name.
		/// </param>
		/// <returns>
		/// An authentication request object that describes the HTTP response to
		/// send to the user agent to initiate the authentication.
		/// </returns>
		/// <remarks>
		/// <para>Requires an <see cref="HttpContext.Current">HttpContext.Current</see> context.</para>
		/// </remarks>
		/// <exception cref="ProtocolException">Thrown if no OpenID endpoint could be found.</exception>
		/// <exception cref="InvalidOperationException">Thrown if <see cref="HttpContext.Current">HttpContext.Current</see> == <c>null</c>.</exception>
		public IAuthenticationRequest CreateRequest(Identifier userSuppliedIdentifier) {
			Contract.Requires<ArgumentNullException>(userSuppliedIdentifier != null);
			Contract.Ensures(Contract.Result<IAuthenticationRequest>() != null);
			try {
				return this.CreateRequests(userSuppliedIdentifier).First();
			} catch (InvalidOperationException ex) {
				throw ErrorUtilities.Wrap(ex, OpenIdStrings.OpenIdEndpointNotFound);
			}
		}

		/// <summary>
		/// Generates the authentication requests that can satisfy the requirements of some OpenID Identifier.
		/// </summary>
		/// <param name="userSuppliedIdentifier">
		/// The Identifier supplied by the user.  This may be a URL, an XRI or i-name.
		/// </param>
		/// <param name="realm">
		/// The shorest URL that describes this relying party web site's address.
		/// For example, if your login page is found at https://www.example.com/login.aspx,
		/// your realm would typically be https://www.example.com/.
		/// </param>
		/// <param name="returnToUrl">
		/// The URL of the login page, or the page prepared to receive authentication 
		/// responses from the OpenID Provider.
		/// </param>
		/// <returns>
		/// A sequence of authentication requests, any of which constitutes a valid identity assertion on the Claimed Identifier.
		/// Never null, but may be empty.
		/// </returns>
		/// <remarks>
		/// <para>Any individual generated request can satisfy the authentication.  
		/// The generated requests are sorted in preferred order.
		/// Each request is generated as it is enumerated to.  Associations are created only as
		/// <see cref="IAuthenticationRequest.RedirectingResponse"/> is called.</para>
		/// <para>No exception is thrown if no OpenID endpoints were discovered.  
		/// An empty enumerable is returned instead.</para>
		/// </remarks>
		public virtual IEnumerable<IAuthenticationRequest> CreateRequests(Identifier userSuppliedIdentifier, Realm realm, Uri returnToUrl) {
			Contract.Requires<ArgumentNullException>(userSuppliedIdentifier != null);
			Contract.Requires<ArgumentNullException>(realm != null);
			Contract.Requires<ArgumentNullException>(returnToUrl != null);
			Contract.Ensures(Contract.Result<IEnumerable<IAuthenticationRequest>>() != null);

			return AuthenticationRequest.Create(userSuppliedIdentifier, this, realm, returnToUrl, true).Cast<IAuthenticationRequest>().CacheGeneratedResults();
		}

		/// <summary>
		/// Generates the authentication requests that can satisfy the requirements of some OpenID Identifier.
		/// </summary>
		/// <param name="userSuppliedIdentifier">
		/// The Identifier supplied by the user.  This may be a URL, an XRI or i-name.
		/// </param>
		/// <param name="realm">
		/// The shorest URL that describes this relying party web site's address.
		/// For example, if your login page is found at https://www.example.com/login.aspx,
		/// your realm would typically be https://www.example.com/.
		/// </param>
		/// <returns>
		/// A sequence of authentication requests, any of which constitutes a valid identity assertion on the Claimed Identifier.
		/// Never null, but may be empty.
		/// </returns>
		/// <remarks>
		/// <para>Any individual generated request can satisfy the authentication.  
		/// The generated requests are sorted in preferred order.
		/// Each request is generated as it is enumerated to.  Associations are created only as
		/// <see cref="IAuthenticationRequest.RedirectingResponse"/> is called.</para>
		/// <para>No exception is thrown if no OpenID endpoints were discovered.  
		/// An empty enumerable is returned instead.</para>
		/// <para>Requires an <see cref="HttpContext.Current">HttpContext.Current</see> context.</para>
		/// </remarks>
		/// <exception cref="InvalidOperationException">Thrown if <see cref="HttpContext.Current">HttpContext.Current</see> == <c>null</c>.</exception>
		public IEnumerable<IAuthenticationRequest> CreateRequests(Identifier userSuppliedIdentifier, Realm realm) {
			Contract.Requires<InvalidOperationException>(HttpContext.Current != null && HttpContext.Current.Request != null, MessagingStrings.HttpContextRequired);
			Contract.Requires<ArgumentNullException>(userSuppliedIdentifier != null);
			Contract.Requires<ArgumentNullException>(realm != null);
			Contract.Ensures(Contract.Result<IEnumerable<IAuthenticationRequest>>() != null);

			// This next code contract is a BAD idea, because it causes each authentication request to be generated
			// at least an extra time.
			////Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<IAuthenticationRequest>>(), el => el != null));

			// Build the return_to URL
			UriBuilder returnTo = new UriBuilder(this.Channel.GetRequestFromContext().UrlBeforeRewriting);

			// Trim off any parameters with an "openid." prefix, and a few known others
			// to avoid carrying state from a prior login attempt.
			returnTo.Query = string.Empty;
			NameValueCollection queryParams = this.Channel.GetRequestFromContext().QueryStringBeforeRewriting;
			var returnToParams = new Dictionary<string, string>(queryParams.Count);
			foreach (string key in queryParams) {
				if (!IsOpenIdSupportingParameter(key) && key != null) {
					returnToParams.Add(key, queryParams[key]);
				}
			}
			returnTo.AppendQueryArgs(returnToParams);

			return this.CreateRequests(userSuppliedIdentifier, realm, returnTo.Uri);
		}

		/// <summary>
		/// Generates the authentication requests that can satisfy the requirements of some OpenID Identifier.
		/// </summary>
		/// <param name="userSuppliedIdentifier">
		/// The Identifier supplied by the user.  This may be a URL, an XRI or i-name.
		/// </param>
		/// <returns>
		/// A sequence of authentication requests, any of which constitutes a valid identity assertion on the Claimed Identifier.
		/// Never null, but may be empty.
		/// </returns>
		/// <remarks>
		/// <para>Any individual generated request can satisfy the authentication.  
		/// The generated requests are sorted in preferred order.
		/// Each request is generated as it is enumerated to.  Associations are created only as
		/// <see cref="IAuthenticationRequest.RedirectingResponse"/> is called.</para>
		/// <para>No exception is thrown if no OpenID endpoints were discovered.  
		/// An empty enumerable is returned instead.</para>
		/// <para>Requires an <see cref="HttpContext.Current">HttpContext.Current</see> context.</para>
		/// </remarks>
		/// <exception cref="InvalidOperationException">Thrown if <see cref="HttpContext.Current">HttpContext.Current</see> == <c>null</c>.</exception>
		public IEnumerable<IAuthenticationRequest> CreateRequests(Identifier userSuppliedIdentifier) {
			Contract.Requires<ArgumentNullException>(userSuppliedIdentifier != null);
			Contract.Requires<InvalidOperationException>(HttpContext.Current != null && HttpContext.Current.Request != null, MessagingStrings.HttpContextRequired);
			Contract.Ensures(Contract.Result<IEnumerable<IAuthenticationRequest>>() != null);

			return this.CreateRequests(userSuppliedIdentifier, Realm.AutoDetect);
		}

		/// <summary>
		/// Gets an authentication response from a Provider.
		/// </summary>
		/// <returns>The processed authentication response if there is any; <c>null</c> otherwise.</returns>
		/// <remarks>
		/// <para>Requires an <see cref="HttpContext.Current">HttpContext.Current</see> context.</para>
		/// </remarks>
		public IAuthenticationResponse GetResponse() {
			Contract.Requires<InvalidOperationException>(HttpContext.Current != null && HttpContext.Current.Request != null, MessagingStrings.HttpContextRequired);
			return this.GetResponse(this.Channel.GetRequestFromContext());
		}

		/// <summary>
		/// Gets an authentication response from a Provider.
		/// </summary>
		/// <param name="httpRequestInfo">The HTTP request that may be carrying an authentication response from the Provider.</param>
		/// <returns>The processed authentication response if there is any; <c>null</c> otherwise.</returns>
		public IAuthenticationResponse GetResponse(HttpRequestInfo httpRequestInfo) {
			Contract.Requires<ArgumentNullException>(httpRequestInfo != null);
			try {
				var message = this.Channel.ReadFromRequest(httpRequestInfo);
				PositiveAssertionResponse positiveAssertion;
				NegativeAssertionResponse negativeAssertion;
				IndirectSignedResponse positiveExtensionOnly;
				if ((positiveAssertion = message as PositiveAssertionResponse) != null) {
					if (this.EndpointFilter != null) {
						// We need to make sure that this assertion is coming from an endpoint
						// that the host deems acceptable.
						var providerEndpoint = new SimpleXrdsProviderEndpoint(positiveAssertion);
						ErrorUtilities.VerifyProtocol(
							this.EndpointFilter(providerEndpoint),
							OpenIdStrings.PositiveAssertionFromNonWhitelistedProvider,
							providerEndpoint.Uri);
					}

					var response = new PositiveAuthenticationResponse(positiveAssertion, this);
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
		/// <returns>The HTTP response to send to this HTTP request.</returns>
		/// <remarks>
		/// <para>Requires an <see cref="HttpContext.Current">HttpContext.Current</see> context.</para>
		/// </remarks>
		public OutgoingWebResponse ProcessResponseFromPopup() {
			Contract.Requires<InvalidOperationException>(HttpContext.Current != null && HttpContext.Current.Request != null, MessagingStrings.HttpContextRequired);
			Contract.Ensures(Contract.Result<OutgoingWebResponse>() != null);

			return this.ProcessResponseFromPopup(this.Channel.GetRequestFromContext());
		}

		/// <summary>
		/// Processes the response received in a popup window or iframe to an AJAX-directed OpenID authentication.
		/// </summary>
		/// <param name="request">The incoming HTTP request that is expected to carry an OpenID authentication response.</param>
		/// <returns>The HTTP response to send to this HTTP request.</returns>
		public OutgoingWebResponse ProcessResponseFromPopup(HttpRequestInfo request) {
			Contract.Requires<ArgumentNullException>(request != null);
			Contract.Ensures(Contract.Result<OutgoingWebResponse>() != null);

			return this.ProcessResponseFromPopup(request, null);
		}

		/// <summary>
		/// Allows an OpenID extension to read data out of an unverified positive authentication assertion
		/// and send it down to the client browser so that Javascript running on the page can perform
		/// some preprocessing on the extension data.
		/// </summary>
		/// <typeparam name="T">The extension <i>response</i> type that will read data from the assertion.</typeparam>
		/// <param name="propertyName">The property name on the openid_identifier input box object that will be used to store the extension data.  For example: sreg</param>
		/// <remarks>
		/// This method should be called before <see cref="ProcessResponseFromPopup()"/>.
		/// </remarks>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design")]
		public void RegisterClientScriptExtension<T>(string propertyName) where T : IClientScriptExtensionResponse {
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(propertyName));
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
			rp.Channel = OpenIdChannel.CreateNonVerifyingChannel();
			return rp;
		}

		/// <summary>
		/// Processes the response received in a popup window or iframe to an AJAX-directed OpenID authentication.
		/// </summary>
		/// <param name="request">The incoming HTTP request that is expected to carry an OpenID authentication response.</param>
		/// <param name="callback">The callback fired after the response status has been determined but before the Javascript response is formulated.</param>
		/// <returns>
		/// The HTTP response to send to this HTTP request.
		/// </returns>
		internal OutgoingWebResponse ProcessResponseFromPopup(HttpRequestInfo request, Action<AuthenticationStatus> callback) {
			Contract.Requires<ArgumentNullException>(request != null);
			Contract.Ensures(Contract.Result<OutgoingWebResponse>() != null);

			string extensionsJson = null;
			var authResponse = this.NonVerifyingRelyingParty.GetResponse();
			ErrorUtilities.VerifyProtocol(authResponse != null, "OpenID popup window or iframe did not recognize an OpenID response in the request.");

			// Give the caller a chance to notify the hosting page and fill up the clientScriptExtensions collection.
			if (callback != null) {
				callback(authResponse.Status);
			}

			Logger.OpenId.DebugFormat("Popup or iframe callback from OP: {0}", request.Url);
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
			if (request.HttpMethod == "POST") {
				// Promote all form variables to the query string, but since it won't be passed
				// to any server (this is a javascript window-to-window transfer) the length of
				// it can be arbitrarily long, whereas it was POSTed here probably because it
				// was too long for HTTP transit.
				UriBuilder payloadUri = new UriBuilder(request.Url);
				payloadUri.AppendQueryArgs(request.Form.ToDictionary());
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
		/// <returns>A non-null sequence of services discovered for the identifier.</returns>
		public IEnumerable<IdentifierDiscoveryResult> Discover(Identifier identifier) {
			Contract.Requires<ArgumentNullException>(identifier != null);
			Contract.Ensures(Contract.Result<IEnumerable<IdentifierDiscoveryResult>>() != null);

			IEnumerable<IdentifierDiscoveryResult> results = Enumerable.Empty<IdentifierDiscoveryResult>();
			foreach (var discoverer in this.DiscoveryServices) {
				bool abortDiscoveryChain;
				var discoveryResults = discoverer.Discover(identifier, this.WebRequestHandler, out abortDiscoveryChain).CacheGeneratedResults();
				results = results.Concat(discoveryResults);
				if (abortDiscoveryChain) {
					Logger.OpenId.InfoFormat("Further discovery on '{0}' was stopped by the {1} discovery service.", identifier, discoverer.GetType().Name);
					break;
				}
			}

			// If any OP Identifier service elements were found, we must not proceed
			// to use any Claimed Identifier services, per OpenID 2.0 sections 7.3.2.2 and 11.2.
			// For a discussion on this topic, see
			// http://groups.google.com/group/dotnetopenid/browse_thread/thread/4b5a8c6b2210f387/5e25910e4d2252c8
			// Sometimes the IIdentifierDiscoveryService will automatically filter this for us, but
			// just to be sure, we'll do it here as well.
			if (!this.SecuritySettings.AllowDualPurposeIdentifiers) {
				results = results.CacheGeneratedResults(); // avoid performing discovery repeatedly
				var opIdentifiers = results.Where(result => result.ClaimedIdentifier == result.Protocol.ClaimedIdentifierForOPIdentifier);
				var claimedIdentifiers = results.Where(result => result.ClaimedIdentifier != result.Protocol.ClaimedIdentifierForOPIdentifier);
				results = opIdentifiers.Any() ? opIdentifiers : claimedIdentifiers;
			}

			return results;
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
		private static OutgoingWebResponse InvokeParentPageScript(string methodCall) {
			Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(methodCall));

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

			var response = new OutgoingWebResponse();
			response.Body = builder.ToString();
			response.Headers.Add(HttpResponseHeader.ContentType, new ContentType("text/html").ToString());
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
