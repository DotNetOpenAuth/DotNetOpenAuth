namespace DotNetOpenId.Consumer {
	using System;
	using DotNetOpenId.Session;
	using Janrain.Yadis;
	using System.Collections.Generic;
	using System.Diagnostics;

	internal class ServiceEndpointManager {
		protected ISessionState Session;

		public ServiceEndpointManager(ISessionState session) {
			this.Session = session;
		}

		public void Cleanup(Uri identityUrl) {
			if (Session != null) {
				Session.Remove(identityUrl.AbsoluteUri);
			}
		}

		public ServiceEndpoint GetNextService(Uri identityUrl) {
			string key = identityUrl.AbsoluteUri;

			List<ServiceEndpoint> endpoints = null;
			if (Session != null) {
				endpoints = Session[key] as List<ServiceEndpoint>;
			}

			if (endpoints == null) {
				endpoints = getServiceEndpoints(identityUrl);
				if (Session != null) {
					Session[key] = endpoints;
				}

				if (endpoints == null) {
					return null;
				}
			}

			ServiceEndpoint endpoint = endpoints[0];

			endpoints.RemoveAt(0);
			if (endpoints.Count == 0 && Session != null)
				Session.Remove(key);

			if (endpoints.Count > 0 && Session == null && TraceUtil.Switch.TraceWarning) {
				Trace.TraceWarning("Multiple endpoints found, but cannot track them without a session.");
			}

			return endpoint;
		}

		static List<ServiceEndpoint> getServiceEndpoints(Uri openid_url) {
			DiscoveryResult result = Yadis.Discover(openid_url);
			if (result == null)
				return null;

			Uri identity_url = result.NormalizedUri;

			List<ServiceEndpoint> endpoints = new List<ServiceEndpoint>();

			if (result.IsXRDS) {
				Xrd xrds_node = new Xrd(result.ResponseText);

				foreach (UriNode uri_node in xrds_node.UriNodes()) {
					try {
						endpoints.Add(new ServiceEndpoint(identity_url, uri_node));
					} catch (ArgumentException) {
					}
				}
			} else {
				try {
					endpoints.Add(new ServiceEndpoint(identity_url, result.ResponseText));
				} catch (ArgumentException) {
					//    pass
				}
			}

			if (endpoints.Count > 0)
				return endpoints;

			return null;
		}
	}
}
