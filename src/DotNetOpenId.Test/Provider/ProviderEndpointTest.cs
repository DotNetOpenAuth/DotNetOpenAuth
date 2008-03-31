using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DotNetOpenId.Test.Hosting;
using DotNetOpenId.Provider;

namespace DotNetOpenId.Test.Provider {
	[TestFixture]
	public class ProviderEndpointTest {
		[Test]
		public void Ctor() {
			ProviderEndpoint pe = new ProviderEndpoint();
		}

		[Test]
		public void SimpleEnabledTest() {
			ProviderEndpoint pe = new ProviderEndpoint();
			Assert.IsTrue(pe.Enabled);
			pe.Enabled = false;
			Assert.IsFalse(pe.Enabled);
		}
	}
}
