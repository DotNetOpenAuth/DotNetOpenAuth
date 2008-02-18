namespace DotNetOpenId.Consumer
{
	using System;
	using System.Collections.Specialized;
	using System.Web.SessionState;
	using DotNetOpenId;
	using DotNetOpenId.Store;
	using DotNetOpenId.Session;
	using System.Web;
	using System.Collections.Generic;
	using IConsumerAssociationStore = DotNetOpenId.Store.IAssociationStore<System.Uri>;
	using ConsumerMemoryStore = DotNetOpenId.Store.AssociationMemoryStore<System.Uri>;

	public class FailureException : ApplicationException
	{
		public Uri identity_url;

		public FailureException(Uri identity_url, string message)
			: base(message)
		{
			this.identity_url = identity_url;
		}
	}

	public class CancelException : ApplicationException
	{
		public Uri identity_url;

		public CancelException(Uri identity_url)
		{
			this.identity_url = identity_url;
		}
	}

	public class SetupNeededException : ApplicationException
	{
		private Uri consumer_id;
		public Uri ConsumerId
		{
			get { return consumer_id; }
		}

		private Uri user_setup_url;
		public Uri UserSetupUrl
		{
			get { return user_setup_url; }
		}

		public SetupNeededException(Uri consumer_id, Uri user_setup_url)
		{
			this.consumer_id = consumer_id;
			this.user_setup_url = user_setup_url;
		}
	}

	public class Consumer
	{
		GenericConsumer consumer;

		public string SessionKeyPrefix { get; set; }

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
			this.consumer = new GenericConsumer(store, new SimpleFetcher());
		}

		public AuthRequest Begin(Uri openid_url)
		{
			ServiceEndpoint endpoint = this.manager.GetNextService(openid_url, this.SessionKeyPrefix);
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
			string token = query[Token.TokenKey];
			if (token == null)
				throw new FailureException(null, "No token found.");

			ConsumerResponse response = this.consumer.Complete(query, token);
			this.manager.Cleanup(response.IdentityUrl, Token.TokenKey);

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
