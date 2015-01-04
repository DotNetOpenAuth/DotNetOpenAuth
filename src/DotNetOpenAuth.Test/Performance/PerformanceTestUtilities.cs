//-----------------------------------------------------------------------
// <copyright file="PerformanceTestUtilities.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Performance {
	using System;
	using System.Diagnostics;
	using System.Reflection;
	using System.Threading;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using NUnit.Framework;

	internal static class PerformanceTestUtilities {
		internal static Stats Baseline;

		static PerformanceTestUtilities() {
			Baseline = CollectBaseline();
			TestUtilities.TestLogger.InfoFormat(
				"Scaled where EmptyStaticFunction = 1.0 ({0:f1} nsec = 1.0 units)",
				Baseline.Median * 1000);
		}

		internal static bool IsOptimized(Assembly assembly) {
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
