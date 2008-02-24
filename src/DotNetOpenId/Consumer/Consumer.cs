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

		public AuthRequest Begin(Uri openIdUrl)
		{
			ServiceEndpoint endpoint = manager.GetNextService(openIdUrl);
			if (endpoint == null)
				throw new OpenIdException("No openid endpoint found");
			return BeginWithoutDiscovery(endpoint);
		}

		internal AuthRequest BeginWithoutDiscovery(ServiceEndpoint endpoint)
		{
			return consumer.Begin(endpoint);
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
