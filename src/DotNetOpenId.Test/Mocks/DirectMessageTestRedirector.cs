using System;
using System.Collections.Generic;
using System.Diagnostics;
using DotNetOpenId.Provider;
using DotNetOpenId.RelyingParty;
using IProviderAssociationStore = DotNetOpenId.IAssociationStore<DotNetOpenId.AssociationRelyingPartyType>;

namespace DotNetOpenId.Test.Mocks {
	class DirectMessageTestRedirector : IDirectMessageChannel {

		IProviderAssociationStore providerStore;

		public DirectMessageTestRedirector(IProviderAssociationStore providerStore) {
			if (providerStore == null) throw new ArgumentNullException("providerStore");
			this.providerStore = providerStore;
		}

		#region IDirectMessageChannel Members

		public IDictionary<string, string> SendDirectMessageAndGetResponse(ServiceEndpoint providerEndpoint, IDictionary<string, string> fields) {
			OpenIdProvider provider = new OpenIdProvider(providerStore, providerEndpoint.ProviderEndpoint,
				providerEndpoint.ProviderEndpoint, fields.ToNameValueCollection());
			Debug.Assert(provider.Request.IsResponseReady, "Direct messages should always have an immediately available response.");
			Response webResponse = (Response)provider.Request.Response;
			EncodableResponse opAuthResponse = (EncodableResponse)webResponse.EncodableMessage;
			return opAuthResponse.EncodedFields;
		}

		#endregion
	}
}
