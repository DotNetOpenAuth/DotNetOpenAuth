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
				if (e.FailureKind == ContractFailureKind.Precondition) {
					// Currently we ignore these so that the regular ErrorUtilities can kick in.
					e.SetHandled();
				} else {
					e.SetHandled();
					Assert.Fail(e.FailureKind.ToString() + ": " + e.Message);
				}
			};
		}
	}
}
