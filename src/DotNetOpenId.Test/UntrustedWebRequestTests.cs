using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using DotNetOpenId.Test.Mocks;
using NUnit.Framework;

namespace DotNetOpenId.Test {
	[TestFixture]
	public class UntrustedWebRequestTests {
		TimeSpan timeoutDefault;
		[SetUp]
		public void SetUp() {
			UntrustedWebRequest.WhitelistHosts.Clear();
			UntrustedWebRequest.WhitelistHostsRegex.Clear();
			UntrustedWebRequest.BlacklistHosts.Clear();
			UntrustedWebRequest.BlacklistHostsRegex.Clear();
			timeoutDefault = UntrustedWebRequest.Timeout;
		}
		[TearDown]
		public void TearDown() {
			UntrustedWebRequest.Timeout = timeoutDefault; // in case a test changed it
		}

		[Test]
		public void DisallowUnsafeHosts() {
			string[] unsafeHosts = new[] {
				// IPv4 loopback representations
				"http://127.0.0.1",
				"http://127.100.0.1",
				"http://127.0.0.100",
				"http://2130706433", // 127.0.0.1 in decimal format
				"http://0x7f000001", // 127.0.0.1 in hex format
				// IPv6 loopback representation
				"http://[::1]",
				// disallowed schemes
				"ftp://ftp.microsoft.com",
				"xri://boo",
			};
			foreach (string unsafeHost in unsafeHosts) {
				try {
					UntrustedWebRequest.Request(new Uri(unsafeHost));
					Assert.Fail("ArgumentException expected but none thrown.");
				} catch (ArgumentException) {
					// expected exception caught.
				}
			}
		}

		[Test]
		public void Whitelist() {
			UntrustedWebRequest.WhitelistHosts.Add("localhost");
			// if this works, then we'll be waiting around for localhost to not respond
			// for a while unless we take the timeout to zero.
			UntrustedWebRequest.Timeout = TimeSpan.Zero; // will be reset in TearDown method
			try {
				UntrustedWebRequest.Request(new Uri("http://localhost:1234"));
				// We're verifying that an ArgumentException is not thrown
				// since we requested localhost to be allowed.
			} catch (WebException) {
				// It's ok, we're not expecting the request to succeed.
			}
		}

		[Test]
		public void WhitelistRegex() {
			UntrustedWebRequest.WhitelistHostsRegex.Add(new Regex(@"^127\.\d+\.\d+\.\d+$"));
			// if this works, then we'll be waiting around for localhost to not respond
			// for a while unless we take the timeout to zero.
			UntrustedWebRequest.Timeout = TimeSpan.Zero; // will be reset in TearDown method
			try {
				UntrustedWebRequest.Request(new Uri("http://127.0.0.1:1234"));
				// We're verifying that an ArgumentException is not thrown
				// since we requested localhost to be allowed.
			} catch (WebException) {
				// It's ok, we're not expecting the request to succeed.
			}
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void Blacklist() {
			UntrustedWebRequest.BlacklistHosts.Add("www.microsoft.com");
			UntrustedWebRequest.Request(new Uri("http://WWW.MICROSOFT.COM"));
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void BlacklistRegex() {
			UntrustedWebRequest.BlacklistHostsRegex.Add(new Regex(@"\Wmicrosoft.com$"));
			UntrustedWebRequest.Request(new Uri("http://WWW.MICROSOFT.COM"));
		}

		/// <summary>
		/// Tests an implicit redirect where the HTTP server changes the responding URI without even
		/// redirecting the client.
		/// </summary>
		[Test]
		public void Redirects() {
			UntrustedWebRequest.WhitelistHosts.Add("localhost");
			UntrustedWebResponse resp = new UntrustedWebResponse(
				new Uri("http://localhost/req"), new Uri("http://localhost/resp"),
					new WebHeaderCollection(), HttpStatusCode.OK, "text/html", null, new MemoryStream());
			MockHttpRequest.RegisterMockResponse(resp);
			Assert.AreSame(resp, UntrustedWebRequest.Request(new Uri("http://localhost/req")));
		}

		/// <summary>
		/// Tests that HTTP Location headers that only use a relative path get interpreted correctly.
		/// </summary>
		[Test]
		public void RelativeRedirect() {
			UntrustedWebRequest.WhitelistHosts.Add("localhost");
			UntrustedWebResponse resp1 = new UntrustedWebResponse(
				new Uri("http://localhost/dir/file1"), new Uri("http://localhost/dir/file1"),
				new WebHeaderCollection {
					{ HttpResponseHeader.Location, "file2" },
				}, HttpStatusCode.Redirect, "text/html", null, new MemoryStream());
			MockHttpRequest.RegisterMockResponse(resp1);
			UntrustedWebResponse resp2 = new UntrustedWebResponse(
				new Uri("http://localhost/dir/file2"), new Uri("http://localhost/dir/file2"),
				new WebHeaderCollection(), HttpStatusCode.OK, "text/html", null, new MemoryStream());
			MockHttpRequest.RegisterMockResponse(resp2);
			Assert.AreSame(resp2, UntrustedWebRequest.Request(new Uri("http://localhost/dir/file1")));
		}
	}
}
