namespace Janrain.OpenId.Consumer
{
	using System;
	using System.Web.SessionState;
	using Janrain.Yadis;
	using System.Collections.Generic;

	public class ServiceEndpointManager
	{
		protected HttpSessionState session;

		public ServiceEndpointManager(HttpSessionState session)
		{
			this.session = session;
		}

		public void Cleanup(Uri openid_url, string prefix)
		{
			string key = prefix + openid_url.AbsoluteUri;

			this.session.Remove(key);
		}

		public ServiceEndpoint GetNextService(Uri openid_url, string prefix)
		{
			string key = prefix + openid_url.AbsoluteUri;

			List<ServiceEndpoint> endpoints = this.session[key] as List<ServiceEndpoint>;

			if (endpoints == null)
			{
				endpoints = this.GetServiceEndpoints(openid_url);

				if (endpoints == null)
				{
					return null;
				}
			}

			ServiceEndpoint endpoint = endpoints[0];

			endpoints.RemoveAt(0);
			if (endpoints.Count == 0)
				this.session.Remove(key);

			return endpoint;
		}

		protected List<ServiceEndpoint> GetServiceEndpoints(Uri openid_url)
		{
			DiscoveryResult result = Yadis.Discover(openid_url);
			if (result == null)
				return null;

			Uri identity_url = result.NormalizedUri;

			List<ServiceEndpoint> endpoints = new List<ServiceEndpoint>();

			if (result.IsXRDS)
			{
				Xrd xrds_node = new Xrd(result.ResponseText);

				foreach (UriNode uri_node in xrds_node.UriNodes())
				{
					try
					{
						endpoints.Add(new ServiceEndpoint(identity_url, uri_node));
					}
					catch (ArgumentException)
					{
					}
				}
			}
			else
			{
				try
				{
					endpoints.Add(new ServiceEndpoint(identity_url, result.ResponseText));
				}
				catch (ArgumentException)
				{
					//    pass
				}
			}

			if (endpoints.Count > 0)
				return endpoints;

			return null;
		}
	}
}
