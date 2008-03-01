using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.IO;
using System.Diagnostics;
using DotNetOpenId.Test.Hosting;
using System.Text.RegularExpressions;

namespace DotNetOpenId.Test.Provider {
	[TestFixture]
	public class OpenIdProviderTest {
		static readonly string webDirectory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\src\DotNetOpenId.TestWeb"));
		Host host;

		[SetUp]
		public void SetUpHost() {
			host = Host.CreateHost(webDirectory);
		}

		[Test]
		public void TestHost() {
			StringWriter sw = new StringWriter();
			string query =  "a=b&c=d";
			string body = "aa=bb&cc=dd";
			host.ProcessRequest("hosttest.aspx",query, body, sw);
			string resultHtml = sw.ToString();
			Assert.IsFalse(string.IsNullOrEmpty(resultHtml));
			Debug.WriteLine(resultHtml);
			Assert.IsTrue(Regex.IsMatch(resultHtml, @"Query.*" + Regex.Escape(query)));
			Assert.IsTrue(Regex.IsMatch(resultHtml, @"Body.*" + Regex.Escape(body)));
		}
	}
}
