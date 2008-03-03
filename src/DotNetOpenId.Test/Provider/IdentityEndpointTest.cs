using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.IO;
using System.Diagnostics;
using DotNetOpenId.Test.Hosting;
using System.Text.RegularExpressions;
using System.Net;
using System.Globalization;

namespace DotNetOpenId.Test.Provider {
	[TestFixture]
	public class IdentityEndpointTest {
		[Test]
		public void IdentityEndpointPage() {
			TestSupport.Scenarios scenario = TestSupport.Scenarios.AutoApproval;
			string html = TestSupport.Host.ProcessRequest(TestSupport.GetIdentityUrl(scenario).AbsoluteUri);
			Trace.TraceInformation("{0} response:{1}{2}", TestSupport.GetIdentityUrl(scenario), Environment.NewLine, html);
			Assert.IsTrue(Regex.IsMatch(html, string.Format(CultureInfo.InvariantCulture,
				@"\<link rel=""openid.server"" href=""http://[^/]+/{0}""\>\</link\>",
				TestSupport.ProviderPage)));
			Assert.IsTrue(Regex.IsMatch(html, string.Format(CultureInfo.InvariantCulture,
				@"\<link rel=""openid.delegate"" href=""http://[^/]+{0}""\>\</link\>",
				Regex.Escape(TestSupport.GetDelegateUrl(scenario).AbsolutePath))));
		}
	}
}
