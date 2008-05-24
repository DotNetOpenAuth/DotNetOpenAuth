using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DotNetOpenId.Test {
	[TestFixture]
	public class UntrustedWebRequestTests {
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
	}
}
