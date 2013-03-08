//-----------------------------------------------------------------------
// <copyright file="MockHttpRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Net;
	using System.Net.Http;
	using System.Text;
	using System.Threading.Tasks;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Test.OpenId;
	using DotNetOpenAuth.Yadis;
	using Validation;

	internal static class MockHttpRequest {
		internal static CoordinatorBase.Handler RegisterMockXrdsResponse(IdentifierDiscoveryResult endpoint) {
			Requires.NotNull(endpoint, "endpoint");

			string identityUri;
			if (endpoint.ClaimedIdentifier == endpoint.Protocol.ClaimedIdentifierForOPIdentifier) {
				identityUri = endpoint.UserSuppliedIdentifier;
			} else {
				identityUri = endpoint.UserSuppliedIdentifier ?? endpoint.ClaimedIdentifier;
			}

			return RegisterMockXrdsResponse(new Uri(identityUri), new IdentifierDiscoveryResult[] { endpoint });
		}

		internal static CoordinatorBase.Handler RegisterMockXrdsResponse(Uri respondingUri, IEnumerable<IdentifierDiscoveryResult> endpoints) {
			Requires.NotNull(endpoints, "endpoints");

			var xrds = new StringBuilder();
			xrds.AppendLine(@"<xrds:XRDS xmlns:xrds='xri://$xrds' xmlns:openid='http://openid.net/xmlns/1.0' xmlns='xri://$xrd*($v*2.0)'>
	<XRD>");
			foreach (var endpoint in endpoints) {
				string template = @"
		<Service priority='10'>
			<Type>{0}</Type>
			<URI>{1}</URI>
			<LocalID>{2}</LocalID>
			<openid:Delegate xmlns:openid='http://openid.net/xmlns/1.0'>{2}</openid:Delegate>
		</Service>";
				string serviceTypeUri;
				if (endpoint.ClaimedIdentifier == endpoint.Protocol.ClaimedIdentifierForOPIdentifier) {
					serviceTypeUri = endpoint.Protocol.OPIdentifierServiceTypeURI;
				} else {
					serviceTypeUri = endpoint.Protocol.ClaimedIdentifierServiceTypeURI;
				}
				string xrd = string.Format(
					CultureInfo.InvariantCulture,
					template,
					HttpUtility.HtmlEncode(serviceTypeUri),
					HttpUtility.HtmlEncode(endpoint.ProviderEndpoint.AbsoluteUri),
					HttpUtility.HtmlEncode(endpoint.ProviderLocalIdentifier));
				xrds.Append(xrd);
			}
			xrds.Append(@"
	</XRD>
</xrds:XRDS>");

			return CoordinatorBase.Handle(respondingUri).By(xrds.ToString(), ContentTypes.Xrds);
		}

		internal static CoordinatorBase.Handler RegisterMockXrdsResponse(UriIdentifier directedIdentityAssignedIdentifier, IdentifierDiscoveryResult providerEndpoint) {
			IdentifierDiscoveryResult identityEndpoint = IdentifierDiscoveryResult.CreateForClaimedIdentifier(
				directedIdentityAssignedIdentifier,
				directedIdentityAssignedIdentifier,
				providerEndpoint.ProviderLocalIdentifier,
				new ProviderEndpointDescription(providerEndpoint.ProviderEndpoint, providerEndpoint.Capabilities),
				10,
				10);
			return RegisterMockXrdsResponse(identityEndpoint);
		}

		internal static CoordinatorBase.Handler RegisterMockXrdsResponse(string embeddedResourcePath, out Identifier id) {
			id = new Uri(new Uri("http://localhost/"), embeddedResourcePath);
			return CoordinatorBase.Handle(new Uri(id))
			                      .By(OpenIdTestBase.LoadEmbeddedFile(embeddedResourcePath), "application/xrds+xml");
		}

		internal static CoordinatorBase.Handler RegisterMockRPDiscovery(bool ssl) {
			string template = @"<xrds:XRDS xmlns:xrds='xri://$xrds' xmlns:openid='http://openid.net/xmlns/1.0' xmlns='xri://$xrd*($v*2.0)'>
	<XRD>
		<Service priority='10'>
			<Type>{0}</Type>
			<URI>{1}</URI>
			<URI>{2}</URI>
		</Service>
	</XRD>
</xrds:XRDS>";
			string xrds = string.Format(
				CultureInfo.InvariantCulture,
				template,
				HttpUtility.HtmlEncode(Protocol.V20.RPReturnToTypeURI),
				HttpUtility.HtmlEncode(OpenIdTestBase.RPRealmUri.AbsoluteUri),
				HttpUtility.HtmlEncode(OpenIdTestBase.RPRealmUriSsl.AbsoluteUri));

			return new CoordinatorBase.Handler(ssl ? OpenIdTestBase.RPRealmUriSsl : OpenIdTestBase.RPRealmUri)
				.By(xrds, ContentTypes.Xrds);
		}

		internal static CoordinatorBase.Handler RegisterMockRedirect(Uri origin, Uri redirectLocation) {
			var response = new HttpResponseMessage(HttpStatusCode.Redirect);
			response.Headers.Location = redirectLocation;
			return new CoordinatorBase.Handler(origin).By(req => response);
		}
	}
}
