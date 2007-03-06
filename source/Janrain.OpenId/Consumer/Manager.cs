namespace Janrain.OpenId.Consumer
{
	using System;
	using System.Web.SessionState;
	using Janrain.Yadis;

	public class ServiceEndpointManager
	{
		protected HttpSessionState session;

		public ServiceEndpointManager(HttpSessionState session)
		{
			throw new NotImplementedException();
		}
		public void Cleanup(Uri openid_url, string prefix)
		{
			throw new NotImplementedException();
		}

		public ServiceEndpoint GetNextService(Uri openid_url, string prefix)
		{
			throw new NotImplementedException();
		}

		protected ServiceEndpoint[] GetServiceEndpoints(Uri openid_url)
		{
			throw new NotImplementedException();
		}

	}
}
