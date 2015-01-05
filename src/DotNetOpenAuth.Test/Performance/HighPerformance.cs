//-----------------------------------------------------------------------
// <copyright file="HighPerformance.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Performance {
	using System;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Runtime.InteropServices;
	using System.Threading;

	using DotNetOpenAuth.Logging;

	using NUnit.Framework;

	/// <summary>
	/// Suppresses logging and forces the CPU into a high performance mode.
	/// </summary>
	internal class HighPerformance : IDisposable {
		private readonly PowerManagment.PowerSetting powerSetting;
		private readonly ProcessPriorityClass originalProcessPriority;

#pragma warning disable 0618
		/// <summary>
		/// Initializes a new instance of the <see cref="HighPerformance"/> class.
		/// </summary>
		internal HighPerformance() {
			////if (!WaitForQuietCpu()) {
			////    Assert.Inconclusive("Timed out waiting for a quiet CPU in which to perform perf tests.");
			////}

			this.powerSetting = new PowerManagment.PowerSetting(PowerManagment.PowerProfiles.HighPerformance);
			this.originalProcessPriority = Process.GetCurrentProcess().PriorityClass;
			Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
			Thread.CurrentThread.Priority = ThreadPriority.Highest;
			SpinCpu();
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
			Thread.CurrentThread.Priority = ThreadPriority.Normal;
			Process.GetCurrentProcess().PriorityClass = this.originalProcessPriority;
			this.powerSetting.Dispose(); // restores original power setting.
		}
#pragma warning restore 0618

		/// <summary>
		/// Runs the CPU in a tight loop to get it out of any low power state.
		/// </summary>
		private static void SpinCpu() {
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

		private static bool WaitForQuietCpu(float maxCpuSpike = 25, int minSecondsOfQuiet = 2, int maxSecondsBeforeGiveUp = 30) {
			using (var pc = new System.Diagnostics.PerformanceCounter()) {
				pc.CategoryName = "Processor";
				pc.CounterName = "% Processor Time";
				pc.InstanceName = "_Total";

				TimeSpan samplingInterval = TimeSpan.FromMilliseconds(1000);
				TimeSpan minimumQuietTime = TimeSpan.FromSeconds(minSecondsOfQuiet);
				TimeSpan maximumTimeBeforeBail = TimeSpan.FromSeconds(maxSecondsBeforeGiveUp);
				DateTime startTryingStamp = DateTime.Now;
				int hitsRequired = (int)(minimumQuietTime.TotalMilliseconds / samplingInterval.TotalMilliseconds);
				while (DateTime.Now - startTryingStamp < maximumTimeBeforeBail) {
					int hits;
					for (hits = 0; hits < hitsRequired; hits++) {
						float currentCpuUtilization = pc.NextValue();
						if (currentCpuUtilization > maxCpuSpike) {
							////Console.WriteLine("Miss: CPU at {0}% utilization", currentCpuUtilization);
							break;
						}

						////Console.WriteLine("Hit: CPU at {0}% utilization", currentCpuUtilization);
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

		/// <summary>
		/// PowerManagement allows you to access the funtionality of the Control Panel -> Power Options
		/// dialog in windows.  (Currently we only use VISTA APIs). 
		/// </summary>
		private static class PowerManagment {
			internal static unsafe Guid CurrentPolicy {
				get {
					Guid* retPolicy = null;
					Guid ret = Guid.Empty;
					try {
						int callRet = PowerGetActiveScheme(IntPtr.Zero, ref retPolicy);
						if (callRet == 0) {
							ret = *retPolicy;
							Marshal.FreeHGlobal((IntPtr)retPolicy);
						}
					} catch (Exception) {
					}
					return ret;
				}

				set {
					Guid newPolicy = value;
					int result = PowerSetActiveScheme(IntPtr.Zero, ref newPolicy);
					if (result != 0) {
						TestUtilities.TestLogger.ErrorFormat("Unable to set power management policy.  Error code: {0}", result);
						////throw new Win32Exception(result);
					}
				}
			}

			[DllImport("powrprof.dll")]
			private static unsafe extern int PowerGetActiveScheme(IntPtr reservedZero, ref Guid* policyGuidRet);

			[DllImport("powrprof.dll")]
			private static extern int PowerSetActiveScheme(IntPtr reservedZero, ref Guid policyGuid);

			internal static class PowerProfiles {
				internal static Guid HighPerformance = new Guid(0x8c5e7fda, 0xe8bf, 0x4a96, 0x9a, 0x85, 0xa6, 0xe2, 0x3a, 0x8c, 0x63, 0x5c);

				internal static Guid Balanced = new Guid(0x381b4222, 0xf694, 0x41f0, 0x96, 0x85, 0xff, 0x5b, 0xb2, 0x60, 0xdf, 0x2e);

				internal static Guid PowerSaver = new Guid(0xa1841308, 0x3541, 0x4fab, 0xbc, 0x81, 0xf7, 0x15, 0x56, 0xf2, 0x0b, 0x4a);
			}

			internal class PowerSetting : IDisposable {
				/// <summary>
				/// The power policy in effect when this instance was constructed.
				/// </summary>
				private Guid previousPolicy;

				/// <summary>
				/// Initializes a new instance of the <see cref="PowerSetting"/> class.
				/// </summary>
				/// <param name="powerProfile">The power profile.</param>
				internal PowerSetting(Guid powerProfile) {
					this.previousPolicy = PowerManagment.CurrentPolicy;
					if (this.previousPolicy != powerProfile) {
						PowerManagment.CurrentPolicy = powerProfile;
					}
				}

				/// <summary>
				/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
				/// </summary>
				public void Dispose() {
					if (this.previousPolicy != PowerManagment.CurrentPolicy) {
						PowerManagment.CurrentPolicy = this.previousPolicy;
					}
				}
			}
		}
	}
}
