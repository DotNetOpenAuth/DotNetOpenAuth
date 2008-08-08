using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DotNetOpenId.Test.Hosting;

namespace DotNetOpenId.Test.UI {
	[SetUpFixture]
	public class UITestSupport {
		internal static AspNetHost Host { get; private set; }

		[SetUp]
		public void SetUp() {
			Host = AspNetHost.CreateHost(TestSupport.TestWebDirectory);
		}

		[TearDown]
		public void TearDown() {
			if (Host != null) {
				Host.CloseHttp();
				Host = null;
			}
		}
	}
}
