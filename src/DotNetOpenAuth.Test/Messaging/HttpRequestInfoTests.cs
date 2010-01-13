//-----------------------------------------------------------------------
// <copyright file="HttpRequestInfoTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Messaging {
	using System;
	using System.Collections.Specialized;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class HttpRequestInfoTests : TestBase {
		[TestMethod]
		public void CtorDefault() {
			HttpRequestInfo info = new HttpRequestInfo();
			Assert.AreEqual("GET", info.HttpMethod);
		}

		[TestMethod]
		public void CtorRequest() {
			HttpRequest request = new HttpRequest("file", "http://someserver?a=b", "a=b");
			////request.Headers["headername"] = "headervalue"; // PlatformNotSupportedException prevents us mocking this up
			HttpRequestInfo info = new HttpRequestInfo(request);
			Assert.AreEqual(request.Headers["headername"], info.Headers["headername"]);
			Assert.AreEqual(request.Url.Query, info.Query);
			Assert.AreEqual(request.QueryString["a"], info.QueryString["a"]);
			Assert.AreEqual(request.Url, info.Url);
			Assert.AreEqual(request.Url, info.UrlBeforeRewriting);
			Assert.AreEqual(request.HttpMethod, info.HttpMethod);
		}

		// All these tests are ineffective because ServerVariables[] cannot be set.
		////[TestMethod]
		////public void CtorRequestWithDifferentPublicHttpHost() {
		////    HttpRequest request = new HttpRequest("file", "http://someserver?a=b", "a=b");
		////    request.ServerVariables["HTTP_HOST"] = "publichost";
		////    HttpRequestInfo info = new HttpRequestInfo(request);
		////    Assert.AreEqual("publichost", info.UrlBeforeRewriting.Host);
		////    Assert.AreEqual(80, info.UrlBeforeRewriting.Port);
		////    Assert.AreEqual(request.Url.Query, info.Query);
		////    Assert.AreEqual(request.QueryString["a"], info.QueryString["a"]);
		////}

		////[TestMethod]
		////public void CtorRequestWithDifferentPublicHttpsHost() {
		////    HttpRequest request = new HttpRequest("file", "https://someserver?a=b", "a=b");
		////    request.ServerVariables["HTTP_HOST"] = "publichost";
		////    HttpRequestInfo info = new HttpRequestInfo(request);
		////    Assert.AreEqual("publichost", info.UrlBeforeRewriting.Host);
		////    Assert.AreEqual(443, info.UrlBeforeRewriting.Port);
		////    Assert.AreEqual(request.Url.Query, info.Query);
		////    Assert.AreEqual(request.QueryString["a"], info.QueryString["a"]);
		////}

		////[TestMethod]
		////public void CtorRequestWithDifferentPublicHostNonstandardPort() {
		////    HttpRequest request = new HttpRequest("file", "http://someserver?a=b", "a=b");
		////    request.ServerVariables["HTTP_HOST"] = "publichost:550";
		////    HttpRequestInfo info = new HttpRequestInfo(request);
		////    Assert.AreEqual("publichost", info.UrlBeforeRewriting.Host);
		////    Assert.AreEqual(550, info.UrlBeforeRewriting.Port);
		////    Assert.AreEqual(request.Url.Query, info.Query);
		////    Assert.AreEqual(request.QueryString["a"], info.QueryString["a"]);
		////}

		////[TestMethod]
		////public void CtorRequestWithDifferentPublicIPv6Host() {
		////    HttpRequest request = new HttpRequest("file", "http://[fe80::587e:c6e5:d3aa:657a]:8089/v3.1/", "");
		////    request.ServerVariables["HTTP_HOST"] = "[fe80::587e:c6e5:d3aa:657b]:8089";
		////    HttpRequestInfo info = new HttpRequestInfo(request);
		////    Assert.AreEqual("[fe80::587e:c6e5:d3aa:657b]", info.UrlBeforeRewriting.Host);
		////    Assert.AreEqual(8089, info.UrlBeforeRewriting.Port);
		////    Assert.AreEqual(request.Url.Query, info.Query);
		////}

		/// <summary>
		/// Checks that a property dependent on another null property
		/// doesn't generate a NullReferenceException.
		/// </summary>
		[TestMethod]
		public void QueryBeforeSettingUrl() {
			HttpRequestInfo info = new HttpRequestInfo();
			Assert.IsNull(info.Query);
		}

		/// <summary>
		/// Verifies that looking up a querystring variable is gracefully handled without a query in the URL.
		/// </summary>
		[TestMethod]
		public void QueryStringLookupWithoutQuery() {
			HttpRequestInfo info = new HttpRequestInfo();
			Assert.IsNull(info.QueryString["hi"]);
		}

		/// <summary>
		/// Verifies SSL forwarders are correctly handled when they supply X_FORWARDED_PROTO and HOST
		/// </summary>
		[TestMethod]
		public void GetPublicFacingUrlSSLForwarder1() {
			HttpRequest req = new HttpRequest("a.aspx", "http://someinternalhost/a.aspx?a=b", "a=b");
			var serverVariables = new NameValueCollection();
			serverVariables["HTTP_X_FORWARDED_PROTO"] = "https";
			serverVariables["HTTP_HOST"] = "somehost";
			Uri actual = HttpRequestInfo.GetPublicFacingUrl(req, serverVariables);
			Uri expected = new Uri("https://somehost/a.aspx?a=b");
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		/// Verifies SSL forwarders are correctly handled when they supply X_FORWARDED_PROTO and HOST:port
		/// </summary>
		[TestMethod]
		public void GetPublicFacingUrlSSLForwarder2() {
			HttpRequest req = new HttpRequest("a.aspx", "http://someinternalhost/a.aspx?a=b", "a=b");
			var serverVariables = new NameValueCollection();
			serverVariables["HTTP_X_FORWARDED_PROTO"] = "https";
			serverVariables["HTTP_HOST"] = "somehost:999";
			Uri actual = HttpRequestInfo.GetPublicFacingUrl(req, serverVariables);
			Uri expected = new Uri("https://somehost:999/a.aspx?a=b");
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		/// Verifies SSL forwarders are correctly handled when they supply just HOST
		/// </summary>
		[TestMethod]
		public void GetPublicFacingUrlSSLForwarder3() {
			HttpRequest req = new HttpRequest("a.aspx", "http://someinternalhost/a.aspx?a=b", "a=b");
			var serverVariables = new NameValueCollection();
			serverVariables["HTTP_HOST"] = "somehost";
			Uri actual = HttpRequestInfo.GetPublicFacingUrl(req, serverVariables);
			Uri expected = new Uri("http://somehost/a.aspx?a=b");
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		/// Verifies SSL forwarders are correctly handled when they supply just HOST:port
		/// </summary>
		[TestMethod]
		public void GetPublicFacingUrlSSLForwarder4() {
			HttpRequest req = new HttpRequest("a.aspx", "http://someinternalhost/a.aspx?a=b", "a=b");
			var serverVariables = new NameValueCollection();
			serverVariables["HTTP_HOST"] = "somehost:79";
			Uri actual = HttpRequestInfo.GetPublicFacingUrl(req, serverVariables);
			Uri expected = new Uri("http://somehost:79/a.aspx?a=b");
			Assert.AreEqual(expected, actual);
		}
	}
}
