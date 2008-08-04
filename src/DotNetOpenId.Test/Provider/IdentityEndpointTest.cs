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
		void parameterizedIdentityEndpointPage(ProtocolVersion version) {
			Protocol protocol = Protocol.Lookup(version);
			TestSupport.Scenarios scenario = TestSupport.Scenarios.AutoApproval;
			Identifier identityUrl = TestSupport.GetIdentityUrl(scenario, version);
			string html = TestSupport.Host.ProcessRequest(identityUrl);
			Trace.TraceInformation("{0} response:{1}{2}", identityUrl, Environment.NewLine, html);
			Assert.IsTrue(Regex.IsMatch(html, string.Format(CultureInfo.InvariantCulture,
				@"\<link rel=""{1}"" href=""http://[^/]+/{0}""\>\</link\>",
				Regex.Escape(TestSupport.ProviderPage),
				Regex.Escape(protocol.HtmlDiscoveryProviderKey))));
			Assert.IsTrue(Regex.IsMatch(html, string.Format(CultureInfo.InvariantCulture,
				@"\<link rel=""{1}"" href=""http://[^/]+{0}""\>\</link\>",
				Regex.Escape(new Uri(TestSupport.GetDelegateUrl(scenario).ToString()).AbsolutePath),
				Regex.Escape(protocol.HtmlDiscoveryLocalIdKey))));
		}

		[Test]
		public void IdentityEndpoint20Page() {
			parameterizedIdentityEndpointPage(ProtocolVersion.V20);
		}

		[Test]
		public void IdentityEndpoint11Page() {
			parameterizedIdentityEndpointPage(ProtocolVersion.V11);
		}
	}
}
