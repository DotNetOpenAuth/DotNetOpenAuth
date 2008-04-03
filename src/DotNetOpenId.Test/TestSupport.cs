using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.IO;
using System.Globalization;
using DotNetOpenId.Test.Hosting;
using DotNetOpenId;
using System.Net;
using System.Collections.Specialized;
using DotNetOpenId.RelyingParty;
using System.Diagnostics;

[SetUpFixture]
public class TestSupport {
	public static readonly string TestWebDirectory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\src\DotNetOpenId.TestWeb"));
	public const string HostTestPage = "HostTest.aspx";
	const string identityPage = "IdentityEndpoint.aspx";
	public const string ProviderPage = "ProviderEndpoint.aspx";
	public const string ConsumerPage = "RelyingParty.aspx";
	public enum Scenarios {
		AutoApproval,
		ApproveOnSetup,
		AlwaysDeny,
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
	public static Uri GetFullUrl(string url) {
		return new Uri(Host.BaseUri, url);
	}

	internal static AspNetHost Host { get; private set; }
	internal static EncodingInterceptor Interceptor { get; private set; }

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
