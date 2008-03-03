using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.IO;
using System.Globalization;
using DotNetOpenId.Test.Hosting;

[SetUpFixture]
public class TestSupport {
	public static readonly string TestWebDirectory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\src\DotNetOpenId.TestWeb"));
	public const string HostTestPage = "HostTest.aspx";
	const string identityPage = "IdentityEndpoint.aspx";
	public const string ProviderPage = "ProviderEndpoint.aspx";
	public const string ConsumerPage = "Consumer.aspx";
	public enum Scenarios {
		AutoApproval,
		ApproveOnSetup,
		AlwaysDeny,
	}
	public static Uri GetIdentityUrl(Scenarios scenario) {
		UriBuilder builder = new UriBuilder(Host.BaseUri);
		builder.Path = "/" + identityPage;
		builder.Query = "user=" + scenario;
		return builder.Uri;
	}
	public static Uri GetDelegateUrl(Scenarios scenario) {
		return new Uri(Host.BaseUri, "/" + scenario);
	}
	public static Uri GetFullUrl(string url) {
		return new Uri(Host.BaseUri, url);
	}

	internal static AspNetHost Host { get; private set; }

	[SetUp]
	public void SetUp() {
		Host = AspNetHost.CreateHost(TestSupport.TestWebDirectory);
	}

	[TearDown]
	public void TearDown() {
		Host.CloseHttp();
		Host = null;
	}
}
