//-----------------------------------------------------------------------
// <copyright file="TestBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test {
	using System;
	using System.IO;
	using System.Reflection;
	using System.Web;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OAuth.Messages;
	using log4net;
	using NUnit.Framework;

	/// <summary>
	/// The base class that all test classes inherit from.
	/// </summary>
	public class TestBase {
		/// <summary>
		/// The full path to the directory that contains the test ASP.NET site.
		/// </summary>
		internal string TestWebDirectory {
			get {
				// System.IO.Path.GetDirectoryName(new System.Uri(basePath).LocalPath)
				string basePath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
				string relativePath = @"src\DotNetOpenAuth.TestWeb";
				for (int i = 0; !Directory.Exists(Path.Combine(basePath, relativePath)) && i < 4; i++) {
					relativePath = "..\\" + relativePath;
				}
				return Path.GetFullPath(relativePath);
			}
		}

		private MessageDescriptionCollection messageDescriptions = new MessageDescriptionCollection();

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
		[SetUp]
		public virtual void SetUp() {
			log4net.Config.XmlConfigurator.Configure(Assembly.GetExecutingAssembly().GetManifestResourceStream("DotNetOpenAuth.Test.Logging.config"));
			MessageBase.LowSecurityMode = true;
			this.messageDescriptions = new MessageDescriptionCollection();
			SetMockHttpContext();
		}

		/// <summary>
		/// The TestCleanup method for the test cases.
		/// </summary>
		[TearDown]
		public virtual void Cleanup() {
			log4net.LogManager.Shutdown();
		}

		/// <summary>
		/// Sets HttpContext.Current to some empty (but non-null!) value.
		/// </summary>
		protected internal static void SetMockHttpContext() {
			HttpContext.Current = new HttpContext(
				new HttpRequest("mock", "http://mock", "mock"),
				new HttpResponse(new StringWriter()));
		}

#pragma warning disable 0618
		protected internal static void SuspendLogging() {
			LogManager.GetLoggerRepository().Threshold = LogManager.GetLoggerRepository().LevelMap["OFF"];
		}

		protected internal static void ResumeLogging() {
			LogManager.GetLoggerRepository().Threshold = LogManager.GetLoggerRepository().LevelMap["ALL"];
		}
#pragma warning restore 0618
	}
}
