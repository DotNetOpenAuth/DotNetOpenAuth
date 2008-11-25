//-----------------------------------------------------------------------
// <copyright file="UITestSupport.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.OpenId.UI {
	using DotNetOpenAuth.Test.Hosting;

	////[SetUpFixture]
	public class UITestSupport {
		internal static AspNetHost Host { get; private set; }

		////[SetUp]
		////public void SetUp() {
		////    Host = AspNetHost.CreateHost(TestSupport.TestWebDirectory);
		////}

		////[TearDown]
		////public void TearDown() {
		////    if (Host != null) {
		////        Host.CloseHttp();
		////        Host = null;
		////    }
		////}
	}
}
