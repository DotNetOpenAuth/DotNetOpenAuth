using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using DotNetOpenId;
using DotNetOpenId.RelyingParty;
using DotNetOpenId.Test.Hosting;
using NUnit.Framework;
using DotNetOpenId.Test.Mocks;

[SetUpFixture]
public class TestSupport {
	public static readonly string TestWebDirectory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\src\DotNetOpenId.TestWeb"));
	public const string HostTestPage = "HostTest.aspx";
	const string identityPage = "IdentityEndpoint.aspx";
	public const string ProviderPage = "ProviderEndpoint.aspx";
	public const string MobileConsumerPage = "RelyingPartyMobile.aspx";
	public const string ConsumerPage = "RelyingParty.aspx";
	public enum Scenarios {
		// Authentication test scenarios
		AutoApproval,
		AutoApprovalAddFragment,
		ApproveOnSetup,
		AlwaysDeny,

		// Extension test scenarios
		/// <summary>
		/// Provides all required and requested fields.
		/// </summary>
		ExtensionFullCooperation,
		/// <summary>
		/// Provides only those fields marked as required.
		/// </summary>
		ExtensionPartialCooperation,
	}
	internal static UriIdentifier GetIdentityUrl(Scenarios scenario, ProtocolVersion providerVersion) {
		UriBuilder builder = new UriBuilder(Host.BaseUri);
		builder.Path = "/" + identityPage;
		builder.Query = "user=" + scenario + "&version=" + providerVersion;
		return new UriIdentifier(builder.Uri);
	}
	public static Identifier GetDelegateUrl(Scenarios scenario) {
		return new UriIdentifier(new Uri(Host.BaseUri, "/" + scenario));
	}
	internal static MockIdentifier GetMockIdentifier(Scenarios scenario, ProtocolVersion providerVersion) {
		ServiceEndpoint se = ServiceEndpoint.CreateForClaimedIdentifier(
			GetIdentityUrl(scenario, providerVersion),
			GetDelegateUrl(scenario),
			new Uri(Host.BaseUri, "/ProviderEndpoint.aspx"),
			new string[] { Protocol.Lookup(providerVersion).ClaimedIdentifierServiceTypeURI },
			10,
			10
			);
		return new MockIdentifier(GetIdentityUrl(scenario, providerVersion), new ServiceEndpoint[] { se });
	}
	public static Uri GetFullUrl(string url) {
		return GetFullUrl(url, null);
	}
	public static Uri GetFullUrl(string url, IDictionary<string, string> args) {
		UriBuilder builder = new UriBuilder(new Uri(Host.BaseUri, url));
		UriUtil.AppendQueryArgs(builder, args);
		return builder.Uri;
	}

	internal static AspNetHost Host { get; private set; }
	internal static EncodingInterceptor Interceptor { get; private set; }

	internal static UntrustedWebRequest.MockRequestResponse GenerateMockXrdsResponses(IDictionary<string, string> requestUriAndResponseBody) {
		return (uri, body, acceptTypes) => {
			string contentType = "text/xml; saml=false; https=false; charset=UTF-8";
			string contentEncoding = null;
			MemoryStream stream = new MemoryStream();
			StreamWriter sw = new StreamWriter(stream);
			Assert.IsNull(body);
			string responseBody;
			if (!requestUriAndResponseBody.TryGetValue(uri.AbsoluteUri, out responseBody)) {
				Assert.Fail("Unexpected HTTP request: {0}", uri);
			}
			sw.Write(responseBody);
			sw.Flush();
			stream.Seek(0, SeekOrigin.Begin);
			return new UntrustedWebResponse(uri, uri, new WebHeaderCollection(),
				HttpStatusCode.OK, contentType, contentEncoding, stream);
		};
	}

	[SetUp]
	public void SetUp() {
		Host = AspNetHost.CreateHost(TestSupport.TestWebDirectory);
		Host.MessageInterceptor = Interceptor = new EncodingInterceptor();
	}

	[TearDown]
	public void TearDown() {
		Host.MessageInterceptor = null;
		if (Host != null) {
			Host.CloseHttp();
			Host = null;
		}
	}

	/// <summary>
	/// Uses an RPs stored association to resign an altered message from a Provider,
	/// to simulate a Provider that deliberately sent a bad message in an attempt
	/// to thwart RP security.
	/// </summary>
	internal static void Resign(NameValueCollection nvc, ApplicationMemoryStore store) {
		Debug.Assert(nvc != null);
		Debug.Assert(store != null);
		var dict = Util.NameValueCollectionToDictionary(nvc);
		Protocol protocol = Protocol.Detect(dict);
		Uri providerEndpoint = new Uri(nvc[protocol.openid.op_endpoint]);
		string assoc_handle = nvc[protocol.openid.assoc_handle];
		Association assoc = store.GetAssociation(providerEndpoint, assoc_handle);
		IList<string> signed = nvc[protocol.openid.signed].Split(',');
		var subsetDictionary = new Dictionary<string, string>();
		foreach (string signedKey in signed) {
			string keyName = protocol.openid.Prefix + signedKey;
			subsetDictionary.Add(signedKey, dict[keyName]);
		}
		nvc[protocol.openid.sig] = Convert.ToBase64String(assoc.Sign(subsetDictionary, signed));
	}
}

static class TestExtensions {
	/// <summary>
	/// Gets a URL that can be requested to send an indirect message.
	/// </summary>
	public static Uri ExtractUrl(this IResponse message) {
		DotNetOpenId.Response response = (DotNetOpenId.Response)message;
		IEncodable encodable = response.EncodableMessage;
		UriBuilder builder = new UriBuilder(encodable.RedirectUrl);
		UriUtil.AppendQueryArgs(builder, encodable.EncodedFields);
		return builder.Uri;
	}

	public static NameValueCollection ToNameValueCollection(this IDictionary<string, string> dictionary) {
		NameValueCollection nvc = new NameValueCollection(dictionary.Count);
		foreach (var pair in dictionary) {
			nvc.Add(pair.Key, pair.Value);
		}
		return nvc;
	}
}
