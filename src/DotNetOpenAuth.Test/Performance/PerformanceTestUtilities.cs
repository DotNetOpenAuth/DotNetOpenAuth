namespace DotNetOpenAuth.Test.Performance {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using log4net;
	using NUnit.Framework;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using System.Threading;
	using System.Diagnostics;

	internal static class PerformanceTestUtilities {
		internal static Stats Baseline;

		private static readonly StatsCollection data = new StatsCollection();

		static PerformanceTestUtilities() {
			Baseline = CollectBaseline();
			TestUtilities.TestLogger.InfoFormat(
				"Scaled where EmptyStaticFunction = 1.0 ({0:f1} nsec = 1.0 units)",
				Baseline.Median * 1000);
		}

		internal static Stats Measure(Action action, float maximumAllowedUnitTime, int samples = 10, int iterations = 100, string name = null) {
			if (!StatsLogger.IsOptimized(typeof(OpenIdRelyingParty).Assembly)) {
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

		internal static bool CoolOff() {
			using (var pc = new System.Diagnostics.PerformanceCounter()) {
				pc.CategoryName = "Processor";
				pc.CounterName = "% Processor Time";
				pc.InstanceName = "_Total";

				TimeSpan samplingInterval = TimeSpan.FromMilliseconds(1000);
				TimeSpan minimumQuietTime = TimeSpan.FromSeconds(2);
				TimeSpan maximumTimeBeforeBail = TimeSpan.FromSeconds(30);
				float maximumCpuUtilization = 10;
				DateTime startTryingStamp = DateTime.Now;
				int hitsRequired = (int)(minimumQuietTime.TotalMilliseconds / samplingInterval.TotalMilliseconds);
				while (DateTime.Now - startTryingStamp < maximumTimeBeforeBail) {
					int hits;
					for (hits = 0; hits < hitsRequired; hits++) {
						float currentCpuUtilization = pc.NextValue();
						if (currentCpuUtilization > maximumCpuUtilization) {
							//Console.WriteLine("Miss: CPU at {0}% utilization", currentCpuUtilization);
							break;
						}

						//Console.WriteLine("Hit: CPU at {0}% utilization", currentCpuUtilization);
						Thread.Sleep(samplingInterval);
					}

					if (hits == hitsRequired) {
						return true;
					}

					Thread.Sleep(samplingInterval);
				}

				return false;
			}
		}

		private static Stats CollectBaseline() {
			using (new HighPerformance()) {
				return new MultiSampleCodeTimer(10, 1000).Measure("MethodCalls: EmptyStaticFunction()", 10, delegate {
					Class.EmptyStaticFunction();
					Class.EmptyStaticFunction();
					Class.EmptyStaticFunction();
					Class.EmptyStaticFunction();
					Class.EmptyStaticFunction();
					Class.EmptyStaticFunction();
					Class.EmptyStaticFunction();
					Class.EmptyStaticFunction();
					Class.EmptyStaticFunction();
					Class.EmptyStaticFunction();
				});
			}
		}

		private class Class {
			[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
			public static void EmptyStaticFunction() {
			}
		}
	}

	/// <summary>
	/// Suppresses logging and forces the CPU into a high performance mode.
	/// </summary>
	internal class HighPerformance : IDisposable {
		private readonly log4net.Core.Level originalLoggerThreshold;
		private readonly PowerManagment.PowerSetting powerSetting;
		private readonly ProcessPriorityClass originalProcessPriority;

#pragma warning disable 0618
		/// <summary>
		/// Initializes a new instance of the <see cref="HighPerformance"/> class.
		/// </summary>
		internal HighPerformance() {
			if (!PerformanceTestUtilities.CoolOff()) {
				Assert.Inconclusive("Timed out waiting for a quiet CPU in which to perform perf tests.");
			}

			this.originalLoggerThreshold = LogManager.GetLoggerRepository().Threshold;
			LogManager.GetLoggerRepository().Threshold = LogManager.GetLoggerRepository().LevelMap["OFF"];
			this.powerSetting = new PowerManagment.PowerSetting(PowerManagment.PowerProfiles.HighPerformance);
			this.originalProcessPriority = Process.GetCurrentProcess().PriorityClass;
			Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
			Thread.CurrentThread.Priority = ThreadPriority.Highest;
			HighCpu();
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
			Thread.CurrentThread.Priority = ThreadPriority.Normal;
			Process.GetCurrentProcess().PriorityClass = this.originalProcessPriority;
			this.powerSetting.Dispose(); // restores original power setting.
			LogManager.GetLoggerRepository().Threshold = this.originalLoggerThreshold;
		}
#pragma warning restore 0618

		/// <summary>
		/// Runs the CPU in a tight loop to get it out of any low power state.
		/// </summary>
		private static void HighCpu() {
			int dummy;
			new MultiSampleCodeTimer(10, 1000).Measure("Loop 1K times", 1, delegate {
				int k = 0;
				while (k < 1000)
					k++;        // still in danger of being optimized.  
				dummy = k;      // avoid optimization.  
			});
		}
	}
}
