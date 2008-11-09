//-----------------------------------------------------------------------
// <copyright file="ErrorUtilitiesTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Messaging {
	using System;
	using DotNetOpenAuth.Messaging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class ErrorUtilitiesTests {
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void VerifyArgumentNotNullThrows() {
			ErrorUtilities.VerifyArgumentNotNull(null, "someArg");
		}

		[TestMethod]
		public void VerifyArgumentNotNullDoesNotThrow() {
			ErrorUtilities.VerifyArgumentNotNull("hi", "someArg");
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void VerifyNonZeroLengthOnNull() {
			ErrorUtilities.VerifyNonZeroLength(null, "someArg");
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void VerifyNonZeroLengthOnEmpty() {
			ErrorUtilities.VerifyNonZeroLength(string.Empty, "someArg");
		}

		[TestMethod]
		public void VerifyNonZeroLengthOnNonEmpty() {
			ErrorUtilities.VerifyNonZeroLength("some Value", "someArg");
		}
	}
}
