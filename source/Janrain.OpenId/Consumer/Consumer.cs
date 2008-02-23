namespace Janrain.OpenId.Consumer
{
	using System;
	using System.Collections.Specialized;
	using System.Web.SessionState;
	using Janrain.OpenId;
	using Janrain.OpenId.Store;
 using Janrain.OpenId.Session;

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

		private string session_key_prefix;

		public string SessionKeyPrefix
		{
			get { return session_key_prefix; }
			set { session_key_prefix = value; }
		}

		ServiceEndpointManager manager;

		public Consumer(IAssociationStore store)
		{
			this.manager = new ServiceEndpointManager(null);
			this.consumer = new GenericConsumer(store, new SimpleFetcher());
		}

		[Obsolete("Call the constructor that does not take an ISessionState instance instead.")]
		public Consumer(ISessionState session, IAssociationStore store)
			: this(store)
		{
			// We're ignoring session now as it's no longer necessary,
			// but we keep this overload for backward compatibility.
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

		public ConsumerResponse Complete(NameValueCollection query)
		{
			string token = query[Token.TokenKey];
			if (token == null)
				throw new FailureException(null, "No token found.");

			ConsumerResponse response = this.consumer.Complete(query, token);
			this.manager.Cleanup(response.IdentityUrl, Token.TokenKey);

			return response;
		}
	}
}
