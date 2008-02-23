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
			this.manager = new ServiceEndpointManager(null);
			this.consumer = new GenericConsumer(store);
		}

		public AuthRequest Begin(Uri openid_url)
		{
			ServiceEndpoint endpoint = this.manager.GetNextService(openid_url);
			if (endpoint == null)
				throw new FailureException(null, "No openid endpoint found");
			return BeginWithoutDiscovery(endpoint);
		}

		public AuthRequest BeginWithoutDiscovery(ServiceEndpoint endpoint)
		{
			AuthRequest auth_req = this.consumer.Begin(endpoint);
			return auth_req;
		}

		public ConsumerResponse Complete(NameValueCollection nvc) {
			return Complete(Util.NameValueCollectionToDictionary(nvc));
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
