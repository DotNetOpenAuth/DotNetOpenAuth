//-----------------------------------------------------------------------
// <copyright file="TestBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test {
	using System.IO;
	using System.Reflection;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OAuth.Messages;
	using log4net;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	/// <summary>
	/// The base class that all test classes inherit from.
	/// </summary>
	public class TestBase {
		/// <summary>
		/// The full path to the directory that contains the test ASP.NET site.
		/// </summary>
		internal static readonly string TestWebDirectory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\src\DotNetOpenAuth.TestWeb"));

		private MessageDescriptionCollection messageDescriptions = new MessageDescriptionCollection();

		/// <summary>
		/// Gets or sets the test context which provides
		/// information about and functionality for the current test run.
		/// </summary>
		public TestContext TestContext { get; set; }

		/// <summary>
		/// Gets the logger that tests should use.
		/// </summary>
		internal static ILog TestLogger {
			get { return TestUtilities.TestLogger; }
		}

		internal MessageDescriptionCollection MessageDescriptions {
			get { return this.messageDescriptions; }
		}

		/// <summary>
		/// The TestInitialize method for the test cases.
		/// </summary>
		[TestInitialize]
		public virtual void SetUp() {
			log4net.Config.XmlConfigurator.Configure(Assembly.GetExecutingAssembly().GetManifestResourceStream("DotNetOpenAuth.Test.Logging.config"));
			MessageBase.LowSecurityMode = true;
			this.messageDescriptions = new MessageDescriptionCollection();
		}

		/// <summary>
		/// The TestCleanup method for the test cases.
		/// </summary>
		[TestCleanup]
		public virtual void Cleanup() {
			log4net.LogManager.Shutdown();
		}

		protected internal static void SuspendLogging() {
			LogManager.GetLoggerRepository().Threshold = LogManager.GetLoggerRepository().LevelMap["OFF"];
		}

		protected internal static void ResumeLogging() {
			LogManager.GetLoggerRepository().Threshold = LogManager.GetLoggerRepository().LevelMap["ALL"];
		}
	}
}
