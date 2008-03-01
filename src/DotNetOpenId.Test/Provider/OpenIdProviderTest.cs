using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.IO;
using System.Diagnostics;
using DotNetOpenId.Test.Hosting;
using System.Text.RegularExpressions;
using DotNetOpenId.Test.Hosting.Tests;

namespace DotNetOpenId.Test.Provider {
	[TestFixture]
	public class OpenIdProviderTest {
		AspNetHost host;

		[SetUp]
		public void SetUpHost() {
			host = AspNetHost.CreateHost(AspNetHostTest.TestWebDirectory);
		}

	}
}
