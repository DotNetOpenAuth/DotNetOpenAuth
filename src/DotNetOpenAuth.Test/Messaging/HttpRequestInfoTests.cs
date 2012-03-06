//-----------------------------------------------------------------------
// <copyright file="HttpRequestInfoTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Messaging {
	using System;
	using System.Collections.Specialized;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using NUnit.Framework;

	[TestFixture]
	public class HttpRequestInfoTests : TestBase {
		// All these tests are ineffective because ServerVariables[] cannot be set.
		////[Test]
		////public void CtorRequestWithDifferentPublicHttpHost() {
		////    HttpRequest request = new HttpRequest("file", "http://someserver?a=b", "a=b");
		////    request.ServerVariables["HTTP_HOST"] = "publichost";
		////    HttpRequestInfo info = new HttpRequestInfo(request);
		////    Assert.AreEqual("publichost", info.UrlBeforeRewriting.Host);
		////    Assert.AreEqual(80, info.UrlBeforeRewriting.Port);
		////    Assert.AreEqual(request.Url.Query, info.Query);
		////    Assert.AreEqual(request.QueryString["a"], info.QueryString["a"]);
		////}

		////[Test]
		////public void CtorRequestWithDifferentPublicHttpsHost() {
		////    HttpRequest request = new HttpRequest("file", "https://someserver?a=b", "a=b");
		////    request.ServerVariables["HTTP_HOST"] = "publichost";
		////    HttpRequestInfo info = new HttpRequestInfo(request);
		////    Assert.AreEqual("publichost", info.UrlBeforeRewriting.Host);
		////    Assert.AreEqual(443, info.UrlBeforeRewriting.Port);
		////    Assert.AreEqual(request.Url.Query, info.Query);
		////    Assert.AreEqual(request.QueryString["a"], info.QueryString["a"]);
		////}

		////[Test]
		////public void CtorRequestWithDifferentPublicHostNonstandardPort() {
		////    HttpRequest request = new HttpRequest("file", "http://someserver?a=b", "a=b");
		////    request.ServerVariables["HTTP_HOST"] = "publichost:550";
		////    HttpRequestInfo info = new HttpRequestInfo(request);
		////    Assert.AreEqual("publichost", info.UrlBeforeRewriting.Host);
		////    Assert.AreEqual(550, info.UrlBeforeRewriting.Port);
		////    Assert.AreEqual(request.Url.Query, info.Query);
		////    Assert.AreEqual(request.QueryString["a"], info.QueryString["a"]);
		////}

		////[Test]
		////public void CtorRequestWithDifferentPublicIPv6Host() {
		////    HttpRequest request = new HttpRequest("file", "http://[fe80::587e:c6e5:d3aa:657a]:8089/v3.1/", "");
		////    request.ServerVariables["HTTP_HOST"] = "[fe80::587e:c6e5:d3aa:657b]:8089";
		////    HttpRequestInfo info = new HttpRequestInfo(request);
		////    Assert.AreEqual("[fe80::587e:c6e5:d3aa:657b]", info.UrlBeforeRewriting.Host);
		////    Assert.AreEqual(8089, info.UrlBeforeRewriting.Port);
		////    Assert.AreEqual(request.Url.Query, info.Query);
		////}

		/// <summary>
		/// Verifies that looking up a querystring variable is gracefully handled without a query in the URL.
		/// </summary>
		[Test]
		public void QueryStringLookupWithoutQuery() {
			var info = new HttpRequestInfo("GET", new Uri("http://somehost/somepath"));
			Assert.IsNull(info.QueryString["hi"]);
		}

		/// <summary>
		/// Verifies SSL forwarders are correctly handled when they supply X_FORWARDED_PROTO and HOST
		/// </summary>
		[Test]
		public void GetPublicFacingUrlSSLForwarder1() {
			HttpRequest req = new HttpRequest("a.aspx", "http://someinternalhost/a.aspx?a=b", "a=b");
			var serverVariables = new NameValueCollection();
			serverVariables["HTTP_X_FORWARDED_PROTO"] = "https";
			serverVariables["HTTP_HOST"] = "somehost";
			Uri actual = new HttpRequestWrapper(req).GetPublicFacingUrl(serverVariables);
			Uri expected = new Uri("https://somehost/a.aspx?a=b");
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		/// Verifies SSL forwarders are correctly handled when they supply X_FORWARDED_PROTO and HOST:port
		/// </summary>
		[Test]
		public void GetPublicFacingUrlSSLForwarder2() {
			HttpRequest req = new HttpRequest("a.aspx", "http://someinternalhost/a.aspx?a=b", "a=b");
			var serverVariables = new NameValueCollection();
			serverVariables["HTTP_X_FORWARDED_PROTO"] = "https";
			serverVariables["HTTP_HOST"] = "somehost:999";
			Uri actual = new HttpRequestWrapper(req).GetPublicFacingUrl(serverVariables);
			Uri expected = new Uri("https://somehost:999/a.aspx?a=b");
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		/// Verifies SSL forwarders are correctly handled when they supply just HOST
		/// </summary>
		[Test]
		public void GetPublicFacingUrlSSLForwarder3() {
			HttpRequest req = new HttpRequest("a.aspx", "http://someinternalhost/a.aspx?a=b", "a=b");
			var serverVariables = new NameValueCollection();
			serverVariables["HTTP_HOST"] = "somehost";
			Uri actual = new HttpRequestWrapper(req).GetPublicFacingUrl(serverVariables);
			Uri expected = new Uri("http://somehost/a.aspx?a=b");
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		/// Verifies SSL forwarders are correctly handled when they supply just HOST:port
		/// </summary>
		[Test]
		public void GetPublicFacingUrlSSLForwarder4() {
			HttpRequest req = new HttpRequest("a.aspx", "http://someinternalhost/a.aspx?a=b", "a=b");
			var serverVariables = new NameValueCollection();
			serverVariables["HTTP_HOST"] = "somehost:79";
			Uri actual = new HttpRequestWrapper(req).GetPublicFacingUrl(serverVariables);
			Uri expected = new Uri("http://somehost:79/a.aspx?a=b");
			Assert.AreEqual(expected, actual);
		}
	}
}
