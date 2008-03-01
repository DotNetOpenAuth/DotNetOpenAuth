using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Net;
using System.Globalization;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DotNetOpenId.Test.Hosting.Tests {
	[TestFixture]
	public class HttpHostTest {
		HttpHost host;
		[SetUp]
		public void HostSetUp() {
			host = new HttpHost(AspNetHostTest.TestWebDirectory);
		}

		[TearDown]
		public void HostTearDown() {
			host.Dispose();
		}

		[Test]
		public void TestHost() {
			string query = "a=b&c=d";
			string body = "aa=bb&cc=dd";

			WebRequest request = WebRequest.Create(string.Format(CultureInfo.InvariantCulture,
				"http://localhost:{0}/{1}?{2}", host.Port, AspNetHostTest.HostTestPage, query));
			request.Method = "POST";
			request.ContentLength = body.Length;
			using (StreamWriter sw = new StreamWriter(request.GetRequestStream()))
				sw.Write(body);

			WebResponse response = request.GetResponse();
			string resultHtml;
			using (StreamReader sr = new StreamReader(response.GetResponseStream()))
				resultHtml = sr.ReadToEnd();
			Assert.IsFalse(string.IsNullOrEmpty(resultHtml));
			Debug.WriteLine(resultHtml);
			Assert.IsTrue(Regex.IsMatch(resultHtml, @"Query.*" + Regex.Escape(query)));
			Assert.IsTrue(Regex.IsMatch(resultHtml, @"Body.*" + Regex.Escape(body)));
		}

	}
}
