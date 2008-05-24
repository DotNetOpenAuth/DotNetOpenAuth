using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Text.RegularExpressions;
using System.Net;

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
			string[] unsafeHosts = new [] {
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
	}
}
