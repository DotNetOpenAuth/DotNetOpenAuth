using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetOpenId.Provider;
using DotNetOpenId.RelyingParty;
using ProviderMemoryStore = DotNetOpenId.AssociationMemoryStore<DotNetOpenId.AssociationRelyingPartyType>;
using System.Net;
using System.Collections.Specialized;
using System.Web;
using System.Diagnostics;

namespace DotNetOpenId.Test.Hosting {
	class InterceptionHost : IDisposable {
		public InterceptionHost(ProviderMemoryStore store, Uri providerEndpoint, HandleRequest requestHandler) {
			this.store = store;
			this.providerEndpoint = providerEndpoint;
			this.requestHandler = requestHandler;
			oldTransform = Fetcher.GetResponseFromRequest;
			Fetcher.GetResponseFromRequest = getResponse;
		}
		public void Dispose() {
			Fetcher.GetResponseFromRequest = oldTransform;
		}

		public delegate IResponse HandleRequest(IRequest request);
		HandleRequest requestHandler;
		ProviderMemoryStore store;
		Uri providerEndpoint;

		Fetcher.HttpRequestToResponseTransform oldTransform;

		FetchResponse getResponse(HttpWebRequest request, byte[] data) {
			NameValueCollection query = HttpUtility.ParseQueryString(
				request.Method == "POST" ? Encoding.UTF8.GetString(data) : request.RequestUri.Query);
			OpenIdProvider provider = new OpenIdProvider(store, providerEndpoint,
				request.RequestUri, query);
			var response = requestHandler(provider.Request);
			if (response == null) {
				return oldTransform(request, data);
			} else {
				return new FetchResponse(request.RequestUri, response.Code, response.Headers, response.Body);
			}
		}
	}
}
