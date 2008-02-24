	using System;
	using System.Collections.Specialized;
	using System.Web.SessionState;
	using DotNetOpenId;
	using DotNetOpenId.Store;
	using DotNetOpenId.Session;
	using System.Web;
	using System.Collections.Generic;
	using DotNetOpenId.Provider;
	using IConsumerAssociationStore = DotNetOpenId.Store.IAssociationStore<System.Uri>;
	using ConsumerMemoryStore = DotNetOpenId.Store.AssociationMemoryStore<System.Uri>;
using System.Globalization;

namespace DotNetOpenId.Consumer {
	public class Consumer
	{
		GenericConsumer consumer;

		ServiceEndpointManager manager;

		/// <summary>
		/// Constructs an OpenId consumer that uses the HttpApplication dictionary as
		/// its association store.
		/// </summary>
		public Consumer() : this(HttpApplicationAssociationStore) { }

		/// <summary>
		/// Constructs an OpenId consumer that uses a given IAssociationStore.
		/// </summary>
		public Consumer(IConsumerAssociationStore store)
		{
			manager = new ServiceEndpointManager(null);
			consumer = new GenericConsumer(store);
		}

		public AuthenticationRequest Begin(Uri openIdUrl, 
			TrustRoot trustRootUrl, Uri returnToUrl)
		{
			ServiceEndpoint endpoint = manager.GetNextService(openIdUrl);
			if (endpoint == null)
				throw new OpenIdException("No openid endpoint found");
			return BeginWithoutDiscovery(endpoint, trustRootUrl, returnToUrl);
		}

		/// <remarks>
		/// This method requires an ASP.NET HttpContext.
		/// </remarks>
		public AuthenticationRequest Begin(Uri openIdUrl, TrustRoot trustRootUrl) {
			if (HttpContext.Current == null) throw new InvalidOperationException(Strings.CurrentHttpContextRequired);

			// Build the return_to URL
			UriBuilder returnTo = new UriBuilder(HttpContext.Current.Request.Url);
			// Trim off any parameters with an "openid." prefix to avoid carrying
			// state from a prior login attempt.
			returnTo.Query = string.Empty;
			var returnToParams = new Dictionary<string, string>(HttpContext.Current.Request.QueryString.Count);
			foreach (string key in HttpContext.Current.Request.QueryString) {
				if (!key.StartsWith(QueryStringArgs.openid.Prefix, StringComparison.OrdinalIgnoreCase) && key != QueryStringArgs.nonce) {
					returnToParams.Add(key, HttpContext.Current.Request.QueryString[key]);
				}
			}
			UriUtil.AppendQueryArgs(returnTo, returnToParams);

			return Begin(openIdUrl, trustRootUrl, returnTo.Uri);
		}

		/// <remarks>
		/// This method requires an ASP.NET HttpContext.
		/// </remarks>
		public AuthenticationRequest Begin(Uri openIdUrl) {
			if (HttpContext.Current == null) throw new InvalidOperationException(Strings.CurrentHttpContextRequired);

			// Build the trustroot URL
			UriBuilder trustRootUrl = new UriBuilder(HttpContext.Current.Request.Url.AbsoluteUri);
			trustRootUrl.Path = HttpContext.Current.Request.ApplicationPath;

			return Begin(openIdUrl, new TrustRoot(trustRootUrl.ToString()));
		}

		internal AuthenticationRequest BeginWithoutDiscovery(ServiceEndpoint endpoint,
			TrustRoot trustRootUrl, Uri returnToUrl)
		{
			// Throw an exception now if the trustroot and the return_to URLs don't match
			// as required by the provider.  We could wait for the provider to test this and
			// fail, but this will be faster and give us a better error message.
			if (!trustRootUrl.IsUrlWithinTrustRoot(returnToUrl))
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
					Strings.ReturnToNotUnderTrustRoot, returnToUrl, trustRootUrl));

			return consumer.Begin(endpoint, trustRootUrl, returnToUrl);
		}

		public ConsumerResponse Complete(NameValueCollection query) {
			return Complete(Util.NameValueCollectionToDictionary(query));
		}

		public ConsumerResponse Complete(IDictionary<string, string> query)
		{
			ConsumerResponse response = consumer.Complete(query);
			manager.Cleanup(response.IdentityUrl);

			return response;
		}

		const string associationStoreKey = "DotNetOpenId.Consumer.Consumer.AssociationStore";
		static IConsumerAssociationStore HttpApplicationAssociationStore {
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
