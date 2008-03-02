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
	public const string IdentityPage = "IdentityEndpoint.aspx";
	public const string ProviderPage = "ProviderEndpoint.aspx";
	public const string ConsumerPage = "Consumer.aspx";
	public static Uri IdentityUrl {
		get { return new Uri(Host.BaseUri, "/IdentityEndpoint.aspx?user=bob"); }
	}
	public static Uri DelegateUrl {
		get { return new Uri(Host.BaseUri, "/bob"); }
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
