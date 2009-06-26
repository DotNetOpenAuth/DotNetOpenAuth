//-----------------------------------------------------------------------
// <copyright file="HttpRequestInfoTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Messaging {
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
	}
}
