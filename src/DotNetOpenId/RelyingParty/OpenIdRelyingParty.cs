using System;
using System.Collections.Specialized;
using System.Web.SessionState;
using DotNetOpenId;
using System.Web;
using System.Collections.Generic;
using DotNetOpenId.Provider;
using System.Globalization;
using System.Diagnostics;

namespace DotNetOpenId.RelyingParty {
	/// <summary>
	/// Provides the programmatic facilities to act as an OpenId consumer.
	/// </summary>
	[DebuggerDisplay("isAuthenticationResponseReady: {isAuthenticationResponseReady}, stateless: {store == null}")]
	public class OpenIdRelyingParty {
		IRelyingPartyApplicationStore store;
		Uri request;
		IDictionary<string, string> query;

		/// <summary>
		/// Constructs an OpenId consumer that uses the current HttpContext request
		/// and uses the HttpApplication dictionary as its association store.
		/// </summary>
		/// <remarks>
		/// This method requires a current ASP.NET HttpContext.
		/// </remarks>
		public OpenIdRelyingParty() : this(HttpApplicationStore, Util.GetRequestUrlFromContext()) { }
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
		/// <remarks>
		/// The IRelyingPartyApplicationStore must be shared across an entire web farm 
		/// because of the design of how nonces are stored/retrieved.  Even if
		/// a given visitor is guaranteed to have affinity toward one server,
		/// replay attacks from another host may be directed at another server,
		/// which must therefore share the nonce information in the application
		/// state store in order to stop the intruder.
		/// </remarks>
		public OpenIdRelyingParty(IRelyingPartyApplicationStore store, Uri requestUrl) {
			this.store = store;
			if (store != null) {
				store.ClearExpiredAssociations(); // every so often we should do this.
			}
			if (requestUrl != null) {
				this.request = requestUrl;
				this.query = Util.NameValueCollectionToDictionary(HttpUtility.ParseQueryString(requestUrl.Query));
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
			return AuthenticationRequest.Create(userSuppliedIdentifier, realm, returnToUrl, store);
		}

		/// <remarks>
		/// This method requires an ASP.NET HttpContext.
		/// </remarks>
		public IAuthenticationRequest CreateRequest(Identifier userSuppliedIdentifier, Realm realm) {
			if (HttpContext.Current == null) throw new InvalidOperationException(Strings.CurrentHttpContextRequired);

			// Build the return_to URL
			UriBuilder returnTo = new UriBuilder(HttpContext.Current.Request.Url);
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
			UriBuilder realmUrl = new UriBuilder(HttpContext.Current.Request.Url);
			realmUrl.Path = HttpContext.Current.Request.ApplicationPath;
			realmUrl.Query = null;
			realmUrl.Fragment = null;

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

				if (HttpContext.Current != null && !HttpContext.Current.Request.RequestType.Equals("GET", StringComparison.Ordinal))
					return false;

				return true;
			}
		}
		IAuthenticationResponse response;
		/// <summary>
		/// Gets the result of a user agent's visit to his OpenId provider in an
		/// authentication attempt.  Null if no response is available.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)] // getter does work
		public IAuthenticationResponse Response {
			get {
				if (response == null && isAuthenticationResponseReady) {
					try {
						if (TraceUtil.Switch.TraceInfo)
							Trace.TraceInformation("OpenID authentication response detected.");
						response = AuthenticationResponse.Parse(query, store, request);
					} catch (OpenIdException ex) {
						response = new FailedAuthenticationResponse(ex);
					}
				}
				return response;
			}
		}

		const string associationStoreKey = "DotNetOpenId.RelyingParty.RelyingParty.AssociationStore";
		/// <summary>
		/// The standard state storage mechanism that uses ASP.NET's HttpApplication state dictionary
		/// to store associations and nonces.
		/// </summary>
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
}
