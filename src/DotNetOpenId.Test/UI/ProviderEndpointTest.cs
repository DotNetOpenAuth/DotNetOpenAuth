using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DotNetOpenId.Test.Hosting;
using DotNetOpenId.Provider;
using System.Net;

namespace DotNetOpenId.Test.UI {
	[TestFixture]
	public class ProviderEndpointTest {
		[Test]
		public void Ctor() {
			ProviderEndpoint pe = new ProviderEndpoint();
		}

		[Test]
		public void SimpleEnabled() {
			ProviderEndpoint pe = new ProviderEndpoint();
			Assert.IsTrue(pe.Enabled);
			pe.Enabled = false;
			Assert.IsFalse(pe.Enabled);
		}

		[Test]
		public void OrdinaryHTTPRequest() {
			Uri pe = TestSupport.GetFullUrl(TestSupport.ProviderPage);
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(pe);
			req.AllowAutoRedirect = false;
			HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
			Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
			Assert.AreEqual("text/html; charset=utf-8", resp.ContentType);
		}

		// Most other scenarios for the endpoint control are tested by our 
		// end-to-end testing.
	}
}
