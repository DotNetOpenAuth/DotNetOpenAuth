//-----------------------------------------------------------------------
// <copyright file="ErrorUtilitiesTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Messaging {
	using System;
	using DotNetOpenAuth.Messaging;
	using NUnit.Framework;

	[TestFixture]
	public class ErrorUtilitiesTests {
		[TestCase, ExpectedException(typeof(ArgumentNullException))]
		public void VerifyArgumentNotNullThrows() {
			ErrorUtilities.VerifyArgumentNotNull(null, "someArg");
		}

		[TestCase]
		public void VerifyArgumentNotNullDoesNotThrow() {
			ErrorUtilities.VerifyArgumentNotNull("hi", "someArg");
		}

		[TestCase, ExpectedException(typeof(ArgumentNullException))]
		public void VerifyNonZeroLengthOnNull() {
			ErrorUtilities.VerifyNonZeroLength(null, "someArg");
		}

		[TestCase, ExpectedException(typeof(ArgumentException))]
		public void VerifyNonZeroLengthOnEmpty() {
			ErrorUtilities.VerifyNonZeroLength(string.Empty, "someArg");
		}

		[TestCase]
		public void VerifyNonZeroLengthOnNonEmpty() {
			ErrorUtilities.VerifyNonZeroLength("some Value", "someArg");
		}
	}
}
