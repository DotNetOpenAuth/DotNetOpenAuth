//-----------------------------------------------------------------------
// <copyright file="HighPerformance.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Performance {
	using System;
	using System.Diagnostics;
	using System.Threading;
	using log4net;
	using NUnit.Framework;

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
			new MultiSampleCodeTimer(10, 1000).Measure(
				"Loop 1K times",
				1,
				delegate {
					int k = 0;
					while (k < 1000) {
						k++;        // still in danger of being optimized.  
					}

					dummy = k;      // avoid optimization.  
				});
		}
	}
}
