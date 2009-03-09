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
				e.Handled = true;
				Assert.Fail(e.FailureKind.ToString() + ": " + e.DebugMessage);
			};
		}
	}
}
