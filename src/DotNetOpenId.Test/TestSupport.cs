using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.IO;
using System.Globalization;
using DotNetOpenId.Test.Hosting;
using DotNetOpenId;
using System.Net;

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
}
