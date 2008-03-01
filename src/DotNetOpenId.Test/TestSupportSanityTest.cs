using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Net;
using System.Globalization;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DotNetOpenId.Test {
	[TestFixture]
	public class TestSupportSanityTest {
		[Test]
		public void TestHost() {
			string query = "a=b&c=d";
			string body = "aa=bb&cc=dd";
			string resultHtml = TestSupport.HttpHost.ProcessRequest(TestSupport.HostTestPage + "?" + query, body);

			Assert.IsFalse(string.IsNullOrEmpty(resultHtml));
			Debug.WriteLine(resultHtml);
			Assert.IsTrue(Regex.IsMatch(resultHtml, @"Query.*" + Regex.Escape(query)));
			Assert.IsTrue(Regex.IsMatch(resultHtml, @"Body.*" + Regex.Escape(body)));
		}

	}
}
