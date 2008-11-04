//-----------------------------------------------------------------------
// <copyright file="TestBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test {
	using System.Reflection;
	using DotNetOAuth.OAuth.Messages;
	using log4net;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	/// <summary>
	/// The base class that all test classes inherit from.
	/// </summary>
	public class TestBase {
		/// <summary>
		/// The logger that tests should use.
		/// </summary>
		internal static readonly ILog TestLogger = LogManager.GetLogger("DotNetOAuth.Test");

		/// <summary>
		/// Gets or sets the test context which provides
		/// information about and functionality for the current test run.
		/// </summary>
		public TestContext TestContext { get; set; }

		/// <summary>
		/// The TestInitialize method for the test cases.
		/// </summary>
		[TestInitialize]
		public virtual void SetUp() {
			log4net.Config.XmlConfigurator.Configure(Assembly.GetExecutingAssembly().GetManifestResourceStream("DotNetOAuth.Test.Logging.config"));
			MessageBase.LowSecurityMode = true;
		}

		/// <summary>
		/// The TestCleanup method for the test cases.
		/// </summary>
		[TestCleanup]
		public virtual void Cleanup() {
			log4net.LogManager.Shutdown();
		}
	}
}
