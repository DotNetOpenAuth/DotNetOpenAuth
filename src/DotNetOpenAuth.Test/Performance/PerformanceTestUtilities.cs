//-----------------------------------------------------------------------
// <copyright file="PerformanceTestUtilities.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Performance {
	using System;
	using System.Diagnostics;
	using System.Reflection;
	using System.Threading;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using NUnit.Framework;

	internal static class PerformanceTestUtilities {
		private static Stats baseline;

		static PerformanceTestUtilities() {
			baseline = CollectBaseline();
			TestUtilities.TestLogger.InfoFormat(
				"Scaled where EmptyStaticFunction = 1.0 ({0:f1} nsec = 1.0 units)",
				baseline.Median * 1000);
		}

		internal static Stats Measure(Action action, float maximumAllowedUnitTime, int samples = 10, int iterations = 100, string name = null) {
			if (!IsOptimized(typeof(OpenIdRelyingParty).Assembly)) {
				Assert.Inconclusive("Unoptimized code.");
			}

			var timer = new MultiSampleCodeTimer(samples, iterations);
			Stats stats;
			using (new HighPerformance()) {
				stats = timer.Measure(name ?? TestContext.CurrentContext.Test.FullName, action);
			}

			stats.AdjustForScale(PerformanceTestUtilities.baseline.Median);

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

		private static bool IsOptimized(Assembly assembly) {
			DebuggableAttribute debugAttribute = (DebuggableAttribute)System.Attribute.GetCustomAttribute(assembly, typeof(System.Diagnostics.DebuggableAttribute));
			return debugAttribute == null || !debugAttribute.IsJITOptimizerDisabled;
		}

		private static Stats CollectBaseline() {
			using (new HighPerformance()) {
				return new MultiSampleCodeTimer(10, 1000).Measure(
					"MethodCalls: EmptyStaticFunction()",
					10,
					delegate {
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
}
