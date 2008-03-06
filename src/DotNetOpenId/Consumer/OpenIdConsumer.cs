using System;
using System.Collections.Specialized;
using System.Web.SessionState;
using DotNetOpenId;
using System.Web;
using System.Collections.Generic;
using DotNetOpenId.Provider;
using System.Globalization;

namespace DotNetOpenId.Consumer {
	/// <summary>
	/// Provides the programmatic facilities to act as an OpenId consumer.
	/// </summary>
	public class OpenIdConsumer {
		IConsumerApplicationStore store;
		ServiceEndpointManager manager;
		IDictionary<string, string> query;
		
		/// <summary>
		/// The maximum time a user can be allowed to take to complete authentication
		/// at the OpenID Provider web site.
		/// </summary>
		/// <remarks>
		/// This is internal until we can decide whether to leave this static, or make
		/// it an instance member, or put it inside the IConsumerAppliationStore interface.
		/// </remarks>
		internal static TimeSpan MaximumUserAgentAuthenticationTime = TimeSpan.FromMinutes(5);

		/// <summary>
		/// Constructs an OpenId consumer that uses the current HttpContext's querystring
		/// and uses the HttpApplication dictionary as its association store.
		/// </summary>
		/// <remarks>
		/// This method requires a current ASP.NET HttpContext.
		/// </remarks>
		public OpenIdConsumer() : this(Util.GetQueryFromContext(), httpApplicationStore) { }
		/// <summary>
		/// Constructs an OpenId consumer that uses a given querystring and IAssociationStore.
		/// </summary>
		/// <param name="query">The name/value pairs that came in on the QueryString of the web request.</param>
		/// <param name="store">
		/// The application-level store where associations with other OpenId providers can be
		/// preserved for optimized authentication.
		/// If null, 'dumb' mode will always be used.
		/// </param>
		public OpenIdConsumer(NameValueCollection query, IConsumerApplicationStore store)
			: this(Util.NameValueCollectionToDictionary(query), store) {
		}
		/// <summary> Constructs an OpenId consumer that uses a given IAssociationStore.</summary>
		/// <param name="query">The name/value pairs that came in on the QueryString of the web request.</param>
		/// <param name="store">
		/// The application-level store where associations with other OpenId providers can be
		/// preserved for optimized authentication.
		/// If null, 'dumb' mode will always be used, and replay attack cannot be protected against.
		/// </param>
		OpenIdConsumer(IDictionary<string, string> query, IConsumerApplicationStore store) {
			if (query == null) throw new ArgumentNullException("query");
			if (store == null) throw new ArgumentNullException("store");
			this.query = query;
			this.store = store;
			manager = new ServiceEndpointManager(null);
			if (store != null) {
				store.ClearExpiredAssociations(); // every so often we should do this.
			}
		}

		public IAuthenticationRequest CreateRequest(Uri openIdUrl, TrustRoot trustRoot, Uri returnToUrl) {
			ServiceEndpoint endpoint = manager.GetNextService(openIdUrl);
			if (endpoint == null)
				throw new OpenIdException("No openid endpoint found");

			return AuthenticationRequest.Create(endpoint, trustRoot, returnToUrl, store);
		}

		/// <remarks>
		/// This method requires an ASP.NET HttpContext.
		/// </remarks>
		public IAuthenticationRequest CreateRequest(Uri openIdUrl, TrustRoot trustRoot) {
			if (HttpContext.Current == null) throw new InvalidOperationException(Strings.CurrentHttpContextRequired);

			// Build the return_to URL
			UriBuilder returnTo = new UriBuilder(HttpContext.Current.Request.Url);
			// Trim off any parameters with an "openid." prefix, and a few known others
			// to avoid carrying state from a prior login attempt.
			returnTo.Query = string.Empty;
			var returnToParams = new Dictionary<string, string>(HttpContext.Current.Request.QueryString.Count);
			foreach (string key in HttpContext.Current.Request.QueryString) {
				if (!key.StartsWith(QueryStringArgs.openid.Prefix, StringComparison.OrdinalIgnoreCase) 
					&& key != Token.TokenKey) {
					returnToParams.Add(key, HttpContext.Current.Request.QueryString[key]);
				}
			}
			UriUtil.AppendQueryArgs(returnTo, returnToParams);

			return CreateRequest(openIdUrl, trustRoot, returnTo.Uri);
		}

		/// <remarks>
		/// This method requires an ASP.NET HttpContext.
		/// </remarks>
		public IAuthenticationRequest CreateRequest(Uri openIdUrl) {
			if (HttpContext.Current == null) throw new InvalidOperationException(Strings.CurrentHttpContextRequired);

			// Build the trustroot URL
			UriBuilder trustRootUrl = new UriBuilder(HttpContext.Current.Request.Url.AbsoluteUri);
			trustRootUrl.Path = HttpContext.Current.Request.ApplicationPath;

			return CreateRequest(openIdUrl, new TrustRoot(trustRootUrl.ToString()));
		}

		/// <summary>
		/// Gets whether an OpenId provider's response to a prior authentication challenge
		/// is embedded in this web request.
		/// </summary>
		bool isAuthenticationResponseReady {
			get {
				if (!query.ContainsKey(QueryStringArgs.openid.mode))
					return false;

				if (HttpContext.Current != null && !HttpContext.Current.Request.RequestType.Equals("GET", StringComparison.Ordinal))
					return false;

				return true;
			}
		}
		AuthenticationResponse response;
		/// <summary>
		/// Gets the result of a user agent's visit to his OpenId provider in an
		/// authentication attempt.  Null if no response is available.
		/// </summary>
		public IAuthenticationResponse Response {
			get {
				if (response == null && isAuthenticationResponseReady) {
					response = AuthenticationResponse.Parse(query, store);
					manager.Cleanup(response.IdentityUrl);
				}
				return response;
			}
		}

		const string associationStoreKey = "DotNetOpenId.Consumer.Consumer.AssociationStore";
		static IConsumerApplicationStore httpApplicationStore {
			get {
				HttpContext context = HttpContext.Current;
				if (context == null)
					throw new InvalidOperationException(Strings.IAssociationStoreRequiredWhenNoHttpContextAvailable);
				var store = (IConsumerApplicationStore)context.Application[associationStoreKey];
				if (store == null) {
					context.Application.Lock();
					try {
						if ((store = (IConsumerApplicationStore)context.Application[associationStoreKey]) == null) {
							context.Application[associationStoreKey] = store = new ConsumerApplicationMemoryStore();
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
