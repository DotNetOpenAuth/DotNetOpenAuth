using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Web;
using DotNetOpenId.Configuration;

namespace DotNetOpenId.RelyingParty {
	/// <summary>
	/// Provides the programmatic facilities to act as an OpenId consumer.
	/// </summary>
	/// <remarks>
	/// For easier, ASP.NET designer drop-in support for adding OpenID login support,
	/// see the <see cref="OpenIdLogin"/> or <see cref="OpenIdTextBox"/> controls.
	/// </remarks>
	/// <example>
	/// <code language="ASP.NET">
	///&lt;h2&gt;Login Page &lt;/h2&gt;
	///&lt;asp:Label ID="Label1" runat="server" Text="OpenID Login" /&gt;
	///&lt;asp:TextBox ID="openIdBox" runat="server" /&gt;
	///&lt;asp:Button ID="loginButton" runat="server" Text="Login" OnClick="loginButton_Click" /&gt;
	///&lt;asp:CustomValidator runat="server" ID="openidValidator" ErrorMessage="Invalid OpenID Identifier"
	///    ControlToValidate="openIdBox" EnableViewState="false" OnServerValidate="openidValidator_ServerValidate" /&gt;
	///&lt;br /&gt;
	///&lt;asp:Label ID="loginFailedLabel" runat="server" EnableViewState="False" Text="Login failed"
	///    Visible="False" /&gt;
	///&lt;asp:Label ID="loginCanceledLabel" runat="server" EnableViewState="False" Text="Login canceled"
	///    Visible="False" /&gt;
	/// </code>
	/// <code language="c#">
	///protected void openidValidator_ServerValidate(object source, ServerValidateEventArgs args) {
	///    // This catches common typos that result in an invalid OpenID Identifier.
	///    args.IsValid = Identifier.IsValid(args.Value);
	///}
	///
	///protected void loginButton_Click(object sender, EventArgs e) {
	///    if (!Page.IsValid) return; // don't login if custom validation failed.
	///    OpenIdRelyingParty openid = new OpenIdRelyingParty();
	///    try {
	///        IAuthenticationRequest request = openid.CreateRequest(openIdBox.Text);
	///        // This is where you would add any OpenID extensions you wanted
	///        // to include in the authentication request.
	///        // request.AddExtension(someExtensionRequestInstance);
	///
	///        // Send your visitor to their Provider for authentication.
	///        request.RedirectToProvider();
	///    } catch (OpenIdException ex) {
	///        // The user probably entered an Identifier that 
	///        // was not a valid OpenID endpoint.
	///        openidValidator.Text = ex.Message;
	///        openidValidator.IsValid = false;
	///    }
	///}
	///
	///protected void Page_Load(object sender, EventArgs e) {
	///    openIdBox.Focus();
	///
	///    OpenIdRelyingParty openid = new OpenIdRelyingParty();
	///    if (openid.Response != null) {
	///        switch (openid.Response.Status) {
	///            case AuthenticationStatus.Authenticated:
	///                // This is where you would look for any OpenID extension responses included
	///                // in the authentication assertion.
	///                // var extension = openid.Response.GetExtension&lt;SomeExtensionResponseType&gt;();
	///
	///                // Use FormsAuthentication to tell ASP.NET that the user is now logged in,
	///                // with the OpenID Claimed Identifier as their username.
	///                FormsAuthentication.RedirectFromLoginPage(openid.Response.ClaimedIdentifier, false);
	///                break;
	///            case AuthenticationStatus.Canceled:
	///                loginCanceledLabel.Visible = true;
	///                break;
	///            case AuthenticationStatus.Failed:
	///                loginFailedLabel.Visible = true;
	///                break;
	///            // We don't need to handle SetupRequired because we're not setting
	///            // IAuthenticationRequest.Mode to immediate mode.
	///            //case AuthenticationStatus.SetupRequired:
	///            //    break;
	///        }
	///    }
	///}
	/// </code>
	/// </example>
	[DebuggerDisplay("isAuthenticationResponseReady: {isAuthenticationResponseReady}, stateless: {Store == null}")]
	public class OpenIdRelyingParty {
		internal IRelyingPartyApplicationStore Store;
		Uri request;
		IDictionary<string, string> query;
		MessageEncoder encoder = new MessageEncoder();
		internal IDirectMessageChannel DirectMessageChannel = new DirectMessageHttpChannel();

		internal static Uri DefaultRequestUrl { get { return Util.GetRequestUrlFromContext(); } }
		internal static NameValueCollection DefaultQuery { get { return Util.GetQueryOrFormFromContextNVC(); } }

		/// <summary>
		/// Constructs an OpenId consumer that uses the current HttpContext request
		/// and uses the HttpApplication dictionary as its association store.
		/// </summary>
		/// <remarks>
		/// This method requires a current ASP.NET HttpContext.
		/// </remarks>
		public OpenIdRelyingParty()
			: this(Configuration.Store.CreateInstanceOfStore(HttpApplicationStore),
				Util.GetRequestUrlFromContext(), Util.GetQueryOrFormFromContext()) { }
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
			// Initialize settings with defaults and config section
			Settings = Configuration.SecuritySettings.CreateSecuritySettings();
			Settings.RequireSslChanged += new EventHandler(Settings_RequireSslChanged);

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
		/// <exception cref="OpenIdException">Thrown if no OpenID endpoint could be found.</exception>
		public IAuthenticationRequest CreateRequest(Identifier userSuppliedIdentifier, Realm realm, Uri returnToUrl) {
			var requests = CreateRequests(userSuppliedIdentifier, realm, returnToUrl).GetEnumerator();
			if (requests.MoveNext()) {
				return requests.Current;
			} else {
				throw new OpenIdException(Strings.OpenIdEndpointNotFound);
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
		/// This method requires an ASP.NET HttpContext.
		/// </remarks>
		/// <exception cref="OpenIdException">Thrown if no OpenID endpoint could be found.</exception>
		public IAuthenticationRequest CreateRequest(Identifier userSuppliedIdentifier, Realm realm) {
			var requests = CreateRequests(userSuppliedIdentifier, realm).GetEnumerator();
			if (requests.MoveNext()) {
				return requests.Current;
			} else {
				throw new OpenIdException(Strings.OpenIdEndpointNotFound);
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
		/// This method requires an ASP.NET HttpContext.
		/// </remarks>
		/// <exception cref="OpenIdException">Thrown if no OpenID endpoint could be found.</exception>
		public IAuthenticationRequest CreateRequest(Identifier userSuppliedIdentifier) {
			var requests = CreateRequests(userSuppliedIdentifier).GetEnumerator();
			if (requests.MoveNext()) {
				return requests.Current;
			} else {
				throw new OpenIdException(Strings.OpenIdEndpointNotFound);
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
		/// An authentication request object that describes the HTTP response to
		/// send to the user agent to initiate the authentication.
		/// </returns>
		/// <remarks>
		/// <para>Any individual generated request can satisfy the authentication.  
		/// The generated requests are sorted in preferred order.
		/// Each request is generated as it is enumerated to, and associations are formed
		/// in the process.  Enumerating beyond the first request is therefore costly
		/// and should only be done if the first request fails.</para>
		/// <para>No exception is thrown if no OpenID endpoints were discovered.  
		/// An empty enumerable is returned instead.</para>
		/// </remarks>
		public IEnumerable<IAuthenticationRequest> CreateRequests(Identifier userSuppliedIdentifier, Realm realm, Uri returnToUrl) {
			if (realm == null) throw new ArgumentNullException("realm");
			if (returnToUrl == null) throw new ArgumentNullException("returnToUrl");

			// Normalize the portion of the return_to path that correlates to the realm for capitalization.
			// (so that if a web app base path is /MyApp/, but the URL of this request happens to be
			// /myapp/login.aspx, we bump up the return_to Url to use /MyApp/ so it matches the realm.
			UriBuilder returnTo = new UriBuilder(returnToUrl);
			if (returnTo.Path.StartsWith(realm.AbsolutePath, StringComparison.OrdinalIgnoreCase) &&
				!returnTo.Path.StartsWith(realm.AbsolutePath, StringComparison.Ordinal)) {
				returnTo.Path = realm.AbsolutePath + returnTo.Path.Substring(realm.AbsolutePath.Length);
			}

			return Util.OfType<IAuthenticationRequest>(AuthenticationRequest.Create(userSuppliedIdentifier, this, realm, returnTo.Uri));
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
		/// Each request is generated as it is enumerated to, and associations are formed
		/// in the process.  Enumerating beyond the first request is therefore costly
		/// and should only be done if the first request fails.</para>
		/// <para>No exception is thrown if no OpenID endpoints were discovered.  
		/// An empty enumerable is returned instead.</para>
		/// </remarks>
		public IEnumerable<IAuthenticationRequest> CreateRequests(Identifier userSuppliedIdentifier, Realm realm) {
			if (HttpContext.Current == null) throw new InvalidOperationException(Strings.CurrentHttpContextRequired);

			// Build the return_to URL
			UriBuilder returnTo = new UriBuilder(Util.GetRequestUrlFromContext());
			// Trim off any parameters with an "openid." prefix, and a few known others
			// to avoid carrying state from a prior login attempt.
			returnTo.Query = string.Empty;
			NameValueCollection queryParams = Util.GetQueryFromContextNVC();
			var returnToParams = new Dictionary<string, string>(queryParams.Count);
			foreach (string key in queryParams) {
				if (!ShouldParameterBeStrippedFromReturnToUrl(key)) {
					returnToParams.Add(key, queryParams[key]);
				}
			}
			UriUtil.AppendQueryArgs(returnTo, returnToParams);

			return CreateRequests(userSuppliedIdentifier, realm, returnTo.Uri);
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
		/// Each request is generated as it is enumerated to, and associations are formed
		/// in the process.  Enumerating beyond the first request is therefore costly
		/// and should only be done if the first request fails.</para>
		/// <para>No exception is thrown if no OpenID endpoints were discovered.  
		/// An empty enumerable is returned instead.</para>
		/// </remarks>
		public IEnumerable<IAuthenticationRequest> CreateRequests(Identifier userSuppliedIdentifier) {
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

			return CreateRequests(userSuppliedIdentifier, new Realm(realmUrl.Uri));
		}

		internal static bool ShouldParameterBeStrippedFromReturnToUrl(string parameterName) {
			Protocol protocol = Protocol.Default;
			return parameterName.StartsWith(protocol.openid.Prefix, StringComparison.OrdinalIgnoreCase)
				|| parameterName == Token.TokenKey;
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
						response = AuthenticationResponse.Parse(query, this, request, true);
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
				// Sort first by service type (OpenID 2.0, 1.1, 1.0),
				// then by Service/@priority, then by Service/Uri/@priority
				return (se1, se2) => {
					int result = getEndpointPrecedenceOrderByServiceType(se1).CompareTo(getEndpointPrecedenceOrderByServiceType(se2));
					if (result != 0) return result;
					if (se1.ServicePriority.HasValue && se2.ServicePriority.HasValue) {
						result = se1.ServicePriority.Value.CompareTo(se2.ServicePriority.Value);
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

		static double getEndpointPrecedenceOrderByServiceType(IXrdsProviderEndpoint endpoint) {
			// The numbers returned from this method only need to compare against other numbers
			// from this method, which makes them arbitrary but relational to only others here.
			if (endpoint.IsTypeUriPresent(Protocol.v20.OPIdentifierServiceTypeURI)) {
				return 0;
			}
			if (endpoint.IsTypeUriPresent(Protocol.v20.ClaimedIdentifierServiceTypeURI)) {
				return 1;
			}
			if (endpoint.IsTypeUriPresent(Protocol.v11.ClaimedIdentifierServiceTypeURI)) {
				return 2;
			}
			if (endpoint.IsTypeUriPresent(Protocol.v10.ClaimedIdentifierServiceTypeURI)) {
				return 3;
			}
			return 10;
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

		/// <summary>
		/// Provides access to the adjustable security settings of this instance
		/// of <see cref="OpenIdRelyingParty"/>.
		/// </summary>
		public RelyingPartySecuritySettings Settings { get; private set; }

		void Settings_RequireSslChanged(object sender, EventArgs e) {
			// reset response that may have been calculated to force 
			// reconsideration with new security policy.
			response = null;
		}

		/// <summary>
		/// Gets the relevant Configuration section for this OpenIdRelyingParty.
		/// </summary>
		internal static RelyingPartySection Configuration {
			get { return RelyingPartySection.Configuration; }
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
