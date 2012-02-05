//-----------------------------------------------------------------------
// <copyright file="TestBase.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test {
	using System;
	using System.IO;
	using System.Reflection;
	using System.Web;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OAuth.Messages;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Test.Performance;
	using log4net;
	using NUnit.Framework;

	/// <summary>
	/// The base class that all test classes inherit from.
	/// </summary>
	public class TestBase {
		private MessageDescriptionCollection messageDescriptions = new MessageDescriptionCollection();

		/// <summary>
		/// Gets the logger that tests should use.
		/// </summary>
		internal static ILog TestLogger {
			get { return TestUtilities.TestLogger; }
		}

		/// <summary>
		/// Gets the full path to the directory that contains the test ASP.NET site.
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

		internal static Stats MeasurePerformance(Action action, float maximumAllowedUnitTime, int samples = 10, int iterations = 100, string name = null) {
			if (!PerformanceTestUtilities.IsOptimized(typeof(OpenIdRelyingParty).Assembly)) {
				Assert.Inconclusive("Unoptimized code.");
			}

			var timer = new MultiSampleCodeTimer(samples, iterations);
			Stats stats;
			using (new HighPerformance()) {
				stats = timer.Measure(name ?? TestContext.CurrentContext.Test.FullName, action);
			}

			stats.AdjustForScale(PerformanceTestUtilities.Baseline.Median);

			TestUtilities.TestLogger.InfoFormat(
				"Performance counters: median {0}, mean {1}, min {2}, max {3}, stddev {4} ({5}%).",
				stats.Median,
				stats.Mean,
				stats.Minimum,
				stats.Maximum,
				stats.StandardDeviation,
				stats.StandardDeviation / stats.Median * 100);

			Assert.IsTrue(stats.Mean < maximumAllowedUnitTime, "The mean time of {0} exceeded the maximum allowable of {1}.", stats.Mean, maximumAllowedUnitTime);
			TestUtilities.TestLogger.InfoFormat("Within {0}% of the maximum allowed time of {1}.", Math.Round((maximumAllowedUnitTime - stats.Mean) / maximumAllowedUnitTime * 100, 1), maximumAllowedUnitTime);

			return stats;
		}

		/// <summary>
		/// Sets HttpContext.Current to some empty (but non-null!) value.
		/// </summary>
		protected internal static void SetMockHttpContext() {
			HttpContext.Current = new HttpContext(
				new HttpRequest("mock", "http://mock", "mock"),
				new HttpResponse(new StringWriter()));
		}
	}
}
