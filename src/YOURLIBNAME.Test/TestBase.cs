namespace YOURLIBNAME.Test {
	using System.Reflection;
	using log4net;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	public class TestBase {
		internal static readonly ILog Logger = LogManager.GetLogger("YOURLIBNAME.Test");

		/// <summary>
		/// Gets or sets the test context which provides
		/// information about and functionality for the current test run.
		/// </summary>
		public TestContext TestContext { get; set; }

		[TestInitialize]
		public virtual void SetUp() {
			log4net.Config.XmlConfigurator.Configure(Assembly.GetExecutingAssembly().GetManifestResourceStream("YOURLIBNAME.Test.Logging.config"));
		}

		[TestCleanup]
		public virtual void Cleanup() {
			log4net.LogManager.Shutdown();
		}
	}
}
