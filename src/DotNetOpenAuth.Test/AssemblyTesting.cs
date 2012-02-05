//-----------------------------------------------------------------------
// <copyright file="AssemblyTesting.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test {
	using System.Diagnostics.Contracts;
	using NUnit.Framework;

	[SetUpFixture]
	public class AssemblyTesting {
		[SetUp]
		public static void AssemblyInitialize() {
			// Make contract failures become test failures.
			Contract.ContractFailed += (sender, e) => {
				// For now, we have tests that verify that preconditions throw exceptions.
				// So we don't want to fail a test just because a precondition check failed.
				if (e.FailureKind != ContractFailureKind.Precondition) {
					e.SetHandled();
					Assert.Fail(e.FailureKind.ToString() + ": " + e.Message);
				}
			};
		}
	}
}
