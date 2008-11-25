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
