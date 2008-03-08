using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DotNetOpenId.Test.Hosting;
using System.Net;

namespace DotNetOpenId.Test.RelyingParty {
	[TestFixture]
	public class OpenIdTextBoxTest {
		[Test]
		public void TextBoxAppears() {
			string html = TestSupport.Host.ProcessRequest(TestSupport.ConsumerPage);
			Assert.IsTrue(html.Contains("<input "));
		}
	}
}
