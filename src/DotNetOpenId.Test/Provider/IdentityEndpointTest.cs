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
			Protocol protocol = Protocol.v20;
			TestSupport.Scenarios scenario = TestSupport.Scenarios.AutoApproval;
			string html = TestSupport.Host.ProcessRequest(TestSupport.GetIdentityUrl(scenario).Uri.AbsoluteUri);
			Trace.TraceInformation("{0} response:{1}{2}", TestSupport.GetIdentityUrl(scenario), Environment.NewLine, html);
			Assert.IsTrue(Regex.IsMatch(html, string.Format(CultureInfo.InvariantCulture,
				@"\<link rel=""{1}"" href=""http://[^/]+/{0}""\>\</link\>",
				Regex.Escape(TestSupport.ProviderPage), 
				Regex.Escape(protocol.HtmlDiscoveryProviderKey))));
			Assert.IsTrue(Regex.IsMatch(html, string.Format(CultureInfo.InvariantCulture,
				@"\<link rel=""{1}"" href=""http://[^/]+{0}""\>\</link\>",
				Regex.Escape(new Uri(TestSupport.GetDelegateUrl(scenario).ToString()).AbsolutePath),
				Regex.Escape(protocol.HtmlDiscoveryLocalIdKey))));
		}
	}
}
