//-----------------------------------------------------------------------
// <copyright file="ServiceProviderTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuth {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using NUnit.Framework;

	[TestFixture]
	public class ServiceProviderTests : TestBase {
		/// <summary>
		/// Verifies the CreateVerificationCode method.
		/// </summary>
		[Test]
		public void CreateVerificationCode() {
			this.TestCode(VerificationCodeFormat.Numeric, 3, MessagingUtilities.Digits);
			this.TestCode(VerificationCodeFormat.AlphaLower, 5, MessagingUtilities.LowercaseLetters);
			this.TestCode(VerificationCodeFormat.AlphaUpper, 5, MessagingUtilities.UppercaseLetters);
			this.TestCode(VerificationCodeFormat.AlphaNumericNoLookAlikes, 8, MessagingUtilities.AlphaNumericNoLookAlikes);
		}

		private void TestCode(VerificationCodeFormat format, int length, string allowableCharacters) {
			string code = ServiceProvider.CreateVerificationCode(format, length);
			TestUtilities.TestLogger.InfoFormat("{0} of length {2}: {1}", format, code, length);
			Assert.AreEqual(length, code.Length);
			foreach (char ch in code) {
				Assert.IsTrue(allowableCharacters.Contains(ch));
			}
		}
	}
}
