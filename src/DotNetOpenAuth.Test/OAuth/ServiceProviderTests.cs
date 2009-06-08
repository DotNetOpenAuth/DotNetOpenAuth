//-----------------------------------------------------------------------
// <copyright file="ServiceProviderTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuth {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class ServiceProviderTests : TestBase {
		/// <summary>
		/// Verifies the CreateVerificationCode method.
		/// </summary>
		[TestMethod]
		public void CreateVerificationCode() {
			this.TestCode(VerificationCodeFormat.Numeric, 3, MessagingUtilities.Digits);
			this.TestCode(VerificationCodeFormat.AlphaLower, 5, MessagingUtilities.LowercaseLetters);
			this.TestCode(VerificationCodeFormat.AlphaUpper, 5, MessagingUtilities.UppercaseLetters);
			this.TestCode(VerificationCodeFormat.AlphaNumericNoLookAlikes, 8, MessagingUtilities.AlphaNumericNoLookAlikes);
		}

		private void TestCode(VerificationCodeFormat format, int length, string allowableCharacters) {
			string code = ServiceProvider.CreateVerificationCode(format, length);
			TestContext.WriteLine("{0} of length {2}: {1}", format, code, length);
			Assert.AreEqual(length, code.Length);
			foreach (char ch in code) {
				Assert.IsTrue(allowableCharacters.Contains(ch));
			}
		}
	}
}
