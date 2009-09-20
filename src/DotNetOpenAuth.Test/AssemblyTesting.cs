//-----------------------------------------------------------------------
// <copyright file="AssemblyTesting.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test {
	using System.Diagnostics.Contracts;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class AssemblyTesting {
		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext tc) {
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
