using System;
using System.Collections.Specialized;
using System.Web.SessionState;
using DotNetOpenId;
using System.Web;
using System.Collections.Generic;
using DotNetOpenId.Provider;
using IConsumerAssociationStore = DotNetOpenId.IAssociationStore<System.Uri>;
using ConsumerMemoryStore = DotNetOpenId.AssociationMemoryStore<System.Uri>;
using System.Globalization;

namespace DotNetOpenId.Consumer {
	/// <summary>
	/// Provides the programmatic facilities to act as an OpenId consumer.
	/// </summary>
	public class OpenIdConsumer {
		GenericConsumer consumer;
		ServiceEndpointManager manager;
		IDictionary<string, string> query;

		/// <summary>
		/// Constructs an OpenId consumer that uses the current HttpContext's querystring
		/// and uses the HttpApplication dictionary as its association store.
		/// </summary>
		/// <remarks>
		/// This method requires a current ASP.NET HttpContext.
		/// </remarks>
		public OpenIdConsumer() : this(Util.GetQueryFromContext(), httpApplicationAssociationStore) { }
		/// <summary>
		/// Constructs an OpenId consumer that uses a given querystring and IAssociationStore.
		/// </summary>
		/// <param name="query">The name/value pairs that came in on the QueryString of the web request.</param>
		/// <param name="store">
		/// The application-level store where associations with other OpenId providers can be
		/// preserved for optimized authentication.
		/// If null, 'dumb' mode will always be used.
		/// </param>
		public OpenIdConsumer(NameValueCollection query, IConsumerAssociationStore store)
			: this(Util.NameValueCollectionToDictionary(query), store) {
		}
		/// <summary> Constructs an OpenId consumer that uses a given IAssociationStore.</summary>
		/// <param name="query">The name/value pairs that came in on the QueryString of the web request.</param>
		/// <param name="store">
		/// The application-level store where associations with other OpenId providers can be
		/// preserved for optimized authentication.
		/// If null, 'dumb' mode will always be used.
		/// </param>
		OpenIdConsumer(IDictionary<string, string> query, IConsumerAssociationStore store) {
			if (query == null) throw new ArgumentNullException("query");
			this.query = query;
			manager = new ServiceEndpointManager(null);
			consumer = new GenericConsumer(store);
		}

		public AuthenticationRequest CreateRequest(Uri openIdUrl, TrustRoot trustRootUrl, Uri returnToUrl) {
			ServiceEndpoint endpoint = manager.GetNextService(openIdUrl);
			if (endpoint == null)
				throw new OpenIdException("No openid endpoint found");
			return prepareRequest(endpoint, trustRootUrl, returnToUrl);
		}

		/// <remarks>
		/// This method requires an ASP.NET HttpContext.
		/// </remarks>
		public AuthenticationRequest CreateRequest(Uri openIdUrl, TrustRoot trustRootUrl) {
			if (HttpContext.Current == null) throw new InvalidOperationException(Strings.CurrentHttpContextRequired);

			// Build the return_to URL
			UriBuilder returnTo = new UriBuilder(HttpContext.Current.Request.Url);
			// Trim off any parameters with an "openid." prefix, and a few known others
			// to avoid carrying state from a prior login attempt.
			returnTo.Query = string.Empty;
			var returnToParams = new Dictionary<string, string>(HttpContext.Current.Request.QueryString.Count);
			foreach (string key in HttpContext.Current.Request.QueryString) {
				if (!key.StartsWith(QueryStringArgs.openid.Prefix, StringComparison.OrdinalIgnoreCase) 
					&& key != QueryStringArgs.nonce && key != Token.TokenKey) {
					returnToParams.Add(key, HttpContext.Current.Request.QueryString[key]);
				}
			}
			UriUtil.AppendQueryArgs(returnTo, returnToParams);

			return CreateRequest(openIdUrl, trustRootUrl, returnTo.Uri);
		}

		/// <remarks>
		/// This method requires an ASP.NET HttpContext.
		/// </remarks>
		public AuthenticationRequest CreateRequest(Uri openIdUrl) {
			if (HttpContext.Current == null) throw new InvalidOperationException(Strings.CurrentHttpContextRequired);

			// Build the trustroot URL
			UriBuilder trustRootUrl = new UriBuilder(HttpContext.Current.Request.Url.AbsoluteUri);
			trustRootUrl.Path = HttpContext.Current.Request.ApplicationPath;

			return CreateRequest(openIdUrl, new TrustRoot(trustRootUrl.ToString()));
		}

		AuthenticationRequest prepareRequest(ServiceEndpoint endpoint,
			TrustRoot trustRootUrl, Uri returnToUrl) {
			// Throw an exception now if the trustroot and the return_to URLs don't match
			// as required by the provider.  We could wait for the provider to test this and
			// fail, but this will be faster and give us a better error message.
			if (!trustRootUrl.IsUrlWithinTrustRoot(returnToUrl))
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
					Strings.ReturnToNotUnderTrustRoot, returnToUrl, trustRootUrl));

			return consumer.Begin(endpoint, trustRootUrl, returnToUrl);
		}

		/// <summary>
		/// Gets whether an OpenId provider's response to a prior authentication challenge
		/// is embedded in this web request.
		/// </summary>
		bool isAuthenticationResponseReady {
			get {
				if (HttpContext.Current == null) throw new InvalidOperationException(Strings.CurrentHttpContextRequired);

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
		public AuthenticationResponse Response {
			get {
				if (response == null && isAuthenticationResponseReady) {
					response = consumer.Complete(query);
					manager.Cleanup(response.IdentityUrl);
				}
				return response;
			}
		}

		const string associationStoreKey = "DotNetOpenId.Consumer.Consumer.AssociationStore";
		static IConsumerAssociationStore httpApplicationAssociationStore {
			get {
				HttpContext context = HttpContext.Current;
				if (context == null)
					throw new InvalidOperationException(Strings.IAssociationStoreRequiredWhenNoHttpContextAvailable);
				var store = (IConsumerAssociationStore)context.Application[associationStoreKey];
				if (store == null) {
					context.Application.Lock();
					try {
						if ((store = (IConsumerAssociationStore)context.Application[associationStoreKey]) == null) {
							context.Application[associationStoreKey] = store = new ConsumerMemoryStore();
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
