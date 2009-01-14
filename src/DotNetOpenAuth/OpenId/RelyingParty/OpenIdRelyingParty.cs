//-----------------------------------------------------------------------
// <copyright file="OpenIdRelyingParty.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.ComponentModel;
	using System.Linq;
	using System.Web;
	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId.ChannelElements;
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
	public delegate bool EndpointSelector(IXrdsProviderEndpoint endpoint);

	/// <summary>
	/// Provides the programmatic facilities to act as an OpenId consumer.
	/// </summary>
	public sealed class OpenIdRelyingParty {
		/// <summary>
		/// The name of the key to use in the HttpApplication cache to store the
		/// instance of <see cref="StandardRelyingPartyApplicationStore"/> to use.
		/// </summary>
		private const string ApplicationStoreKey = "DotNetOpenAuth.OpenId.RelyingParty.OpenIdRelyingParty.ApplicationStore";

		/// <summary>
		/// Backing field for the <see cref="SecuritySettings"/> property.
		/// </summary>
		private RelyingPartySecuritySettings securitySettings;

		/// <summary>
		/// Backing store for the <see cref="EndpointOrder"/> property.
		/// </summary>
		private Comparison<IXrdsProviderEndpoint> endpointOrder = DefaultEndpointOrder;

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdRelyingParty"/> class.
		/// </summary>
		public OpenIdRelyingParty()
			: this(DotNetOpenAuth.Configuration.RelyingPartySection.Configuration.ApplicationStore.CreateInstance(HttpApplicationStore)) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdRelyingParty"/> class.
		/// </summary>
		/// <param name="applicationStore">The application store.  If null, the relying party will always operate in "dumb mode".</param>
		public OpenIdRelyingParty(IRelyingPartyApplicationStore applicationStore)
			: this(applicationStore, applicationStore, applicationStore) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdRelyingParty"/> class.
		/// </summary>
		/// <param name="associationStore">The association store.  If null, the relying party will always operate in "dumb mode".</param>
		/// <param name="nonceStore">The nonce store to use.  If null, the relying party will always operate in "dumb mode".</param>
		/// <param name="secretStore">The secret store to use.  If null, the relying party will always operate in "dumb mode".</param>
		private OpenIdRelyingParty(IAssociationStore<Uri> associationStore, INonceStore nonceStore, IPrivateSecretStore secretStore) {
			// If we are a smart-mode RP (supporting associations), then we MUST also be 
			// capable of storing nonces to prevent replay attacks.
			// If we're a dumb-mode RP, then 2.0 OPs are responsible for preventing replays.
			ErrorUtilities.VerifyArgument(associationStore == null || nonceStore != null, OpenIdStrings.AssociationStoreRequiresNonceStore);

			this.Channel = new OpenIdChannel(associationStore, nonceStore, secretStore);
			this.AssociationStore = associationStore;
			this.SecuritySettings = RelyingPartySection.Configuration.SecuritySettings.CreateSecuritySettings();

			// Without a nonce store, we must rely on the Provider to protect against
			// replay attacks.  But only 2.0+ Providers can be expected to provide 
			// replay protection.
			if (nonceStore == null) {
				this.SecuritySettings.MinimumRequiredOpenIdVersion = ProtocolVersion.V20;
			}
		}

		/// <summary>
		/// Gets an XRDS sorting routine that uses the XRDS Service/@Priority 
		/// attribute to determine order.
		/// </summary>
		/// <remarks>
		/// Endpoints lacking any priority value are sorted to the end of the list.
		/// </remarks>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static Comparison<IXrdsProviderEndpoint> DefaultEndpointOrder {
			get {
				// Sort first by service type (OpenID 2.0, 1.1, 1.0),
				// then by Service/@priority, then by Service/Uri/@priority
				return (se1, se2) => {
					int result = GetEndpointPrecedenceOrderByServiceType(se1).CompareTo(GetEndpointPrecedenceOrderByServiceType(se2));
					if (result != 0) {
						return result;
					}
					if (se1.ServicePriority.HasValue && se2.ServicePriority.HasValue) {
						result = se1.ServicePriority.Value.CompareTo(se2.ServicePriority.Value);
						if (result != 0) {
							return result;
						}
						if (se1.UriPriority.HasValue && se2.UriPriority.HasValue) {
							return se1.UriPriority.Value.CompareTo(se2.UriPriority.Value);
						} else if (se1.UriPriority.HasValue) {
							return -1;
						} else if (se2.UriPriority.HasValue) {
							return 1;
						} else {
							return 0;
						}
					} else {
						if (se1.ServicePriority.HasValue) {
							return -1;
						} else if (se2.ServicePriority.HasValue) {
							return 1;
						} else {
							// neither service defines a priority, so base ordering by uri priority.
							if (se1.UriPriority.HasValue && se2.UriPriority.HasValue) {
								return se1.UriPriority.Value.CompareTo(se2.UriPriority.Value);
							} else if (se1.UriPriority.HasValue) {
								return -1;
							} else if (se2.UriPriority.HasValue) {
								return 1;
							} else {
								return 0;
							}
						}
					}
				};
			}
		}

		/// <summary>
		/// Gets the standard state storage mechanism that uses ASP.NET's
		/// HttpApplication state dictionary to store associations and nonces.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static IRelyingPartyApplicationStore HttpApplicationStore {
			get {
				HttpContext context = HttpContext.Current;
				ErrorUtilities.VerifyOperation(context != null, OpenIdStrings.StoreRequiredWhenNoHttpContextAvailable, typeof(IRelyingPartyApplicationStore).Name);
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
		/// Gets the channel to use for sending/receiving messages.
		/// </summary>
		public Channel Channel { get; internal set; }

		/// <summary>
		/// Gets the security settings used by this Relying Party.
		/// </summary>
		public RelyingPartySecuritySettings SecuritySettings {
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
		public Comparison<IXrdsProviderEndpoint> EndpointOrder {
			get {
				return this.endpointOrder;
			}

			set {
				ErrorUtilities.VerifyArgumentNotNull(value, "value");
				this.endpointOrder = value;
			}
		}

		/// <summary>
		/// Gets the association store.
		/// </summary>
		internal IAssociationStore<Uri> AssociationStore { get; private set; }

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
		/// An authentication request object that describes the HTTP response to
		/// send to the user agent to initiate the authentication.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown if no OpenID endpoint could be found.</exception>
		public IAuthenticationRequest CreateRequest(Identifier userSuppliedIdentifier, Realm realm, Uri returnToUrl) {
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
			try {
				return this.CreateRequests(userSuppliedIdentifier, realm).First();
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
			try {
				return this.CreateRequests(userSuppliedIdentifier).First();
			} catch (InvalidOperationException ex) {
				throw ErrorUtilities.Wrap(ex, OpenIdStrings.OpenIdEndpointNotFound);
			}
		}

		/// <summary>
		/// Gets an authentication response from a Provider.
		/// </summary>
		/// <returns>The processed authentication response if there is any; <c>null</c> otherwise.</returns>
		/// <remarks>
		/// <para>Requires an <see cref="HttpContext.Current">HttpContext.Current</see> context.</para>
		/// </remarks>
		public IAuthenticationResponse GetResponse() {
			return this.GetResponse(this.Channel.GetRequestFromContext());
		}

		/// <summary>
		/// Gets an authentication response from a Provider.
		/// </summary>
		/// <param name="httpRequestInfo">The HTTP request that may be carrying an authentication response from the Provider.</param>
		/// <returns>The processed authentication response if there is any; <c>null</c> otherwise.</returns>
		public IAuthenticationResponse GetResponse(HttpRequestInfo httpRequestInfo) {
			try {
				var message = this.Channel.ReadFromRequest();
				PositiveAssertionResponse positiveAssertion;
				NegativeAssertionResponse negativeAssertion;
				if ((positiveAssertion = message as PositiveAssertionResponse) != null) {
					return new PositiveAuthenticationResponse(positiveAssertion, this);
				} else if ((negativeAssertion = message as NegativeAssertionResponse) != null) {
					return new NegativeAuthenticationResponse(negativeAssertion);
				} else if (message != null) {
					Logger.WarnFormat("Received unexpected message type {0} when expecting an assertion message.", message.GetType().Name);
				}

				return null;
			} catch (ProtocolException ex) {
				return new FailedAuthenticationResponse(ex);
			}
		}

		/// <summary>
		/// Determines whether some parameter name belongs to OpenID or this library
		/// as a protocol or internal parameter name.
		/// </summary>
		/// <param name="parameterName">Name of the parameter.</param>
		/// <returns>
		/// 	<c>true</c> if the named parameter is a library- or protocol-specific parameter; otherwise, <c>false</c>.
		/// </returns>
		internal static bool IsOpenIdSupportingParameter(string parameterName) {
			Protocol protocol = Protocol.Default;
			return parameterName.StartsWith(protocol.openid.Prefix, StringComparison.OrdinalIgnoreCase)
				|| parameterName.StartsWith("dnoi.", StringComparison.Ordinal);
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
		/// An authentication request object that describes the HTTP response to
		/// send to the user agent to initiate the authentication.
		/// </returns>
		/// <remarks>
		/// <para>Any individual generated request can satisfy the authentication.  
		/// The generated requests are sorted in preferred order.
		/// Each request is generated as it is enumerated to.  Associations are created only as
		/// <see cref="IAuthenticationRequest.RedirectingResponse"/> is called.</para>
		/// <para>No exception is thrown if no OpenID endpoints were discovered.  
		/// An empty enumerable is returned instead.</para>
		/// </remarks>
		internal IEnumerable<IAuthenticationRequest> CreateRequests(Identifier userSuppliedIdentifier, Realm realm, Uri returnToUrl) {
			ErrorUtilities.VerifyArgumentNotNull(realm, "realm");
			ErrorUtilities.VerifyArgumentNotNull(returnToUrl, "returnToUrl");

			return AuthenticationRequest.Create(userSuppliedIdentifier, this, realm, returnToUrl, true).Cast<IAuthenticationRequest>();
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
		/// An authentication request object that describes the HTTP response to
		/// send to the user agent to initiate the authentication.
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
		internal IEnumerable<IAuthenticationRequest> CreateRequests(Identifier userSuppliedIdentifier, Realm realm) {
			ErrorUtilities.VerifyHttpContext();

			// Build the return_to URL
			UriBuilder returnTo = new UriBuilder(MessagingUtilities.GetRequestUrlFromContext());

			// Trim off any parameters with an "openid." prefix, and a few known others
			// to avoid carrying state from a prior login attempt.
			returnTo.Query = string.Empty;
			NameValueCollection queryParams = MessagingUtilities.GetQueryFromContextNVC();
			var returnToParams = new Dictionary<string, string>(queryParams.Count);
			foreach (string key in queryParams) {
				if (!IsOpenIdSupportingParameter(key)) {
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
		/// An authentication request object that describes the HTTP response to
		/// send to the user agent to initiate the authentication.
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
		internal IEnumerable<IAuthenticationRequest> CreateRequests(Identifier userSuppliedIdentifier) {
			ErrorUtilities.VerifyHttpContext();

			// Build the realm URL
			UriBuilder realmUrl = new UriBuilder(MessagingUtilities.GetRequestUrlFromContext());
			realmUrl.Path = HttpContext.Current.Request.ApplicationPath;
			realmUrl.Query = null;
			realmUrl.Fragment = null;

			// For RP discovery, the realm url MUST NOT redirect.  To prevent this for 
			// virtual directory hosted apps, we need to make sure that the realm path ends
			// in a slash (since our calculation above guarantees it doesn't end in a specific
			// page like default.aspx).
			if (!realmUrl.Path.EndsWith("/", StringComparison.Ordinal)) {
				realmUrl.Path += "/";
			}

			return this.CreateRequests(userSuppliedIdentifier, new Realm(realmUrl.Uri));
		}

		/// <summary>
		/// Gets an association between this Relying Party and a given Provider
		/// if it already exists in the association store.
		/// </summary>
		/// <param name="provider">The provider to create an association with.</param>
		/// <returns>The association if one exists and has useful life remaining.  Otherwise <c>null</c>.</returns>
		internal Association GetExistingAssociation(ProviderEndpointDescription provider) {
			ErrorUtilities.VerifyArgumentNotNull(provider, "provider");

			Protocol protocol = Protocol.Lookup(provider.ProtocolVersion);

			// If the RP has no application store for associations, there's no point in creating one.
			if (this.AssociationStore == null) {
				return null;
			}

			// TODO: we need a way to lookup an association that fulfills a given set of security
			// requirements.  We may have a SHA-1 association and a SHA-256 association that need
			// to be called for specifically. (a bizzare scenario, admittedly, making this low priority).
			Association association = this.AssociationStore.GetAssociation(provider.Endpoint);

			// If the returned association does not fulfill security requirements, ignore it.
			if (association != null && !this.SecuritySettings.IsAssociationInPermittedRange(protocol, association.GetAssociationType(protocol))) {
				association = null;
			}

			if (association != null && !association.HasUsefulLifeRemaining) {
				association = null;
			}

			return association;
		}

		/// <summary>
		/// Gets an existing association with the specified Provider, or attempts to create
		/// a new association of one does not already exist.
		/// </summary>
		/// <param name="provider">The provider to get an association for.</param>
		/// <returns>The existing or new association; <c>null</c> if none existed and one could not be created.</returns>
		internal Association GetOrCreateAssociation(ProviderEndpointDescription provider) {
			return this.GetExistingAssociation(provider) ?? this.CreateNewAssociation(provider);
		}

		/// <summary>
		/// Gets the priority rating for a given type of endpoint, allowing a
		/// priority sorting of endpoints.
		/// </summary>
		/// <param name="endpoint">The endpoint to prioritize.</param>
		/// <returns>An arbitary integer, which may be used for sorting against other returned values from this method.</returns>
		private static double GetEndpointPrecedenceOrderByServiceType(IXrdsProviderEndpoint endpoint) {
			// The numbers returned from this method only need to compare against other numbers
			// from this method, which makes them arbitrary but relational to only others here.
			if (endpoint.IsTypeUriPresent(Protocol.V20.OPIdentifierServiceTypeURI)) {
				return 0;
			}
			if (endpoint.IsTypeUriPresent(Protocol.V20.ClaimedIdentifierServiceTypeURI)) {
				return 1;
			}
			if (endpoint.IsTypeUriPresent(Protocol.V11.ClaimedIdentifierServiceTypeURI)) {
				return 2;
			}
			if (endpoint.IsTypeUriPresent(Protocol.V10.ClaimedIdentifierServiceTypeURI)) {
				return 3;
			}
			return 10;
		}

		/// <summary>
		/// Creates a new association with a given Provider.
		/// </summary>
		/// <param name="provider">The provider to create an association with.</param>
		/// <returns>
		/// The newly created association, or null if no association can be created with
		/// the given Provider given the current security settings.
		/// </returns>
		/// <remarks>
		/// A new association is created and returned even if one already exists in the
		/// association store.
		/// Any new association is automatically added to the <see cref="AssociationStore"/>.
		/// </remarks>
		private Association CreateNewAssociation(ProviderEndpointDescription provider) {
			ErrorUtilities.VerifyArgumentNotNull(provider, "provider");

			// If there is no association store, there is no point in creating an association.
			if (this.AssociationStore == null) {
				return null;
			}

			var associateRequest = AssociateRequest.Create(this.SecuritySettings, provider);
			if (associateRequest == null) {
				// this can happen if security requirements and protocol conflict
				// to where there are no association types to choose from.
				return null;
			}

			var associateResponse = this.Channel.Request(associateRequest);
			var associateSuccessfulResponse = associateResponse as AssociateSuccessfulResponse;
			var associateUnsuccessfulResponse = associateResponse as AssociateUnsuccessfulResponse;
			if (associateSuccessfulResponse != null) {
				Association association = associateSuccessfulResponse.CreateAssociation(associateRequest);
				this.AssociationStore.StoreAssociation(provider.Endpoint, association);
				return association;
			} else if (associateUnsuccessfulResponse != null) {
				// TODO: code here
				throw new NotImplementedException();
			} else {
				throw new ProtocolException(MessagingStrings.UnexpectedMessageReceivedOfMany);
			}
		}
	}
}
