using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Web;

namespace DotNetOpenId.RelyingParty {
	/// <summary>
	/// Provides the programmatic facilities to act as an OpenId consumer.
	/// </summary>
	[DebuggerDisplay("isAuthenticationResponseReady: {isAuthenticationResponseReady}, stateless: {store == null}")]
	public class OpenIdRelyingParty {
		internal IRelyingPartyApplicationStore Store;
		Uri request;
		IDictionary<string, string> query;
		MessageEncoder encoder = new MessageEncoder();
		internal IDirectMessageChannel DirectMessageChannel = new DirectMessageHttpChannel();

		/// <summary>
		/// Constructs an OpenId consumer that uses the current HttpContext request
		/// and uses the HttpApplication dictionary as its association store.
		/// </summary>
		/// <remarks>
		/// This method requires a current ASP.NET HttpContext.
		/// </remarks>
		public OpenIdRelyingParty()
			: this(HttpApplicationStore,
				Util.GetRequestUrlFromContext(), Util.GetQueryFromContext()) { }
		/// <summary>
		/// Constructs an OpenId consumer that uses a given querystring and IAssociationStore.
		/// </summary>
		/// <param name="store">
		/// The application-level store where associations with other OpenId providers can be
		/// preserved for optimized authentication and information about nonces can be stored.
		/// In a multi-server web farm environment, this store MUST be shared across
		/// all servers.  Optional: if null, the relying party will operate in stateless mode.
		/// </param>
		/// <param name="requestUrl">
		/// Optional.  The current incoming HTTP request that may contain an OpenId assertion.
		/// If not included, any OpenId authentication assertions will not be processed.
		/// </param>
		/// <param name="query">
		/// The name/value pairs that came in on the 
		/// QueryString of a GET request or in the entity of a POST request.
		/// For example: (Request.HttpMethod == "GET" ? Request.QueryString : Request.Form).
		/// This must be supplied if <paramref name="requestUrl"/> is supplied.
		/// </param>
		/// <remarks>
		/// The IRelyingPartyApplicationStore must be shared across an entire web farm 
		/// because of the design of how nonces are stored/retrieved.  Even if
		/// a given visitor is guaranteed to have affinity toward one server,
		/// replay attacks from another host may be directed at another server,
		/// which must therefore share the nonce information in the application
		/// state store in order to stop the intruder.
		/// </remarks>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public OpenIdRelyingParty(IRelyingPartyApplicationStore store, Uri requestUrl, NameValueCollection query) :
			this(store, requestUrl, Util.NameValueCollectionToDictionary(query)) {
		}
		OpenIdRelyingParty(IRelyingPartyApplicationStore store, Uri requestUrl, IDictionary<string, string> query) {
			this.Store = store;
			if (store != null) {
				store.ClearExpiredAssociations(); // every so often we should do this.
			}
			if (requestUrl != null) {
				if (query == null) throw new ArgumentNullException("query");
				this.request = requestUrl;
				this.query = query;
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
		/// An authentication request object that describes the HTTP response to
		/// send to the user agent to initiate the authentication.
		/// </returns>
		public IAuthenticationRequest CreateRequest(Identifier userSuppliedIdentifier, Realm realm, Uri returnToUrl) {
			return AuthenticationRequest.Create(userSuppliedIdentifier, this, realm, returnToUrl);
		}

		/// <remarks>
		/// This method requires an ASP.NET HttpContext.
		/// </remarks>
		public IAuthenticationRequest CreateRequest(Identifier userSuppliedIdentifier, Realm realm) {
			if (HttpContext.Current == null) throw new InvalidOperationException(Strings.CurrentHttpContextRequired);

			// Build the return_to URL
			UriBuilder returnTo = new UriBuilder(Util.GetRequestUrlFromContext());
			// Trim off any parameters with an "openid." prefix, and a few known others
			// to avoid carrying state from a prior login attempt.
			returnTo.Query = string.Empty;
			var returnToParams = new Dictionary<string, string>(HttpContext.Current.Request.QueryString.Count);
			foreach (string key in HttpContext.Current.Request.QueryString) {
				if (!ShouldParameterBeStrippedFromReturnToUrl(key)) {
					returnToParams.Add(key, HttpContext.Current.Request.QueryString[key]);
				}
			}
			UriUtil.AppendQueryArgs(returnTo, returnToParams);

			return CreateRequest(userSuppliedIdentifier, realm, returnTo.Uri);
		}

		internal static bool ShouldParameterBeStrippedFromReturnToUrl(string parameterName) {
			Protocol protocol = Protocol.Default;
			return parameterName.StartsWith(protocol.openid.Prefix, StringComparison.OrdinalIgnoreCase)
				|| parameterName == Token.TokenKey;
		}

		/// <remarks>
		/// This method requires an ASP.NET HttpContext.
		/// </remarks>
		public IAuthenticationRequest CreateRequest(Identifier userSuppliedIdentifier) {
			if (HttpContext.Current == null) throw new InvalidOperationException(Strings.CurrentHttpContextRequired);

			// Build the realm URL
			UriBuilder realmUrl = new UriBuilder(Util.GetRequestUrlFromContext());
			realmUrl.Path = HttpContext.Current.Request.ApplicationPath;
			realmUrl.Query = null;
			realmUrl.Fragment = null;

			// For RP discovery, the realm url MUST NOT redirect.  To prevent this for 
			// virtual directory hosted apps, we need to make sure that the realm path ends
			// in a slash (since our calculation above guarantees it doesn't end in a specific
			// page like default.aspx).
			if (!realmUrl.Path.EndsWith("/", StringComparison.Ordinal))
				realmUrl.Path += "/";

			return CreateRequest(userSuppliedIdentifier, new Realm(realmUrl.Uri));
		}

		/// <summary>
		/// Gets whether an OpenId provider's response to a prior authentication challenge
		/// is embedded in this web request.
		/// </summary>
		bool isAuthenticationResponseReady {
			get {
				if (query == null) return false;
				Protocol protocol = Protocol.Detect(query);
				if (!query.ContainsKey(protocol.openid.mode))
					return false;

				return true;
			}
		}
		IAuthenticationResponse response;
		/// <summary>
		/// Gets the result of a user agent's visit to his OpenId provider in an
		/// authentication attempt.  Null if no response is available.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)] // getter does lots of processing, so avoid debugger calling it.
		public IAuthenticationResponse Response {
			get {
				if (response == null && isAuthenticationResponseReady) {
					try {
						response = AuthenticationResponse.Parse(query, this, request);
					} catch (OpenIdException ex) {
						response = new FailedAuthenticationResponse(ex);
					}
				}
				return response;
			}
		}

		/// <summary>
		/// The message encoder to use.
		/// </summary>
		internal MessageEncoder Encoder { get { return encoder; } }

		private Comparison<IXrdsProviderEndpoint> endpointOrder = DefaultEndpointOrder;
		/// <summary>
		/// Gets/sets the ordering routine that will determine which XRDS 
		/// Service element to try first 
		/// </summary>
		/// <remarks>
		/// This may never be null.  To reset to default behavior this property 
		/// can be set to the value of <see cref="DefaultEndpointOrder"/>.
		/// </remarks>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public Comparison<IXrdsProviderEndpoint> EndpointOrder {
			get { return endpointOrder; }
			set {
				if (value == null) throw new ArgumentNullException("value");
				endpointOrder = value;
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
				// Sort first by Service/@priority, then by Service/Uri/@priority
				return (se1, se2) => {
					if (se1.ServicePriority.HasValue && se2.ServicePriority.HasValue) {
						int result = se1.ServicePriority.Value.CompareTo(se2.ServicePriority.Value);
						if (result != 0) return result;
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
		/// Provides a way to optionally filter the providers that may be used in authenticating a user.
		/// </summary>
		/// <remarks>
		/// If provided, the delegate should return true to accept an endpoint, and false to reject it.
		/// If null, all identity providers will be accepted.  This is the default.
		/// </remarks>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public EndpointSelector EndpointFilter { get; set; }

		const string associationStoreKey = "DotNetOpenId.RelyingParty.RelyingParty.AssociationStore";
		/// <summary>
		/// The standard state storage mechanism that uses ASP.NET's HttpApplication state dictionary
		/// to store associations and nonces.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static IRelyingPartyApplicationStore HttpApplicationStore {
			get {
				HttpContext context = HttpContext.Current;
				if (context == null)
					throw new InvalidOperationException(Strings.IAssociationStoreRequiredWhenNoHttpContextAvailable);
				var store = (IRelyingPartyApplicationStore)context.Application[associationStoreKey];
				if (store == null) {
					context.Application.Lock();
					try {
						if ((store = (IRelyingPartyApplicationStore)context.Application[associationStoreKey]) == null) {
							context.Application[associationStoreKey] = store = new ApplicationMemoryStore();
						}
					} finally {
						context.Application.UnLock();
					}
				}
				return store;
			}
		}
	}

	/// <summary>
	/// A delegate that decides whether a given OpenID Provider endpoint may be
	/// considered for authenticating a user.
	/// </summary>
	/// <returns>
	/// True if the endpoint should be considered.  
	/// False to remove it from the pool of acceptable providers.
	/// </returns>
	public delegate bool EndpointSelector(IXrdsProviderEndpoint endpoint);
}
