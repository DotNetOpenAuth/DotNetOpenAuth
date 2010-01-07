//-----------------------------------------------------------------------
// <copyright file="LocalizationTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test {
	using System;
	using System.Globalization;
	using System.Threading;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using NUnit.Framework;

	/// <summary>
	/// Tests various localized resources work as expected.
	/// </summary>
	[TestFixture]
	public class LocalizationTests : TestBase {
		/// <summary>
		/// Tests that Serbian localized strings are correctly installed.
		/// </summary>
		[TestCase, ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Ovaj metod zahteva tekući HttpContext. Kao alternativa, koristite preklopljeni metod koji dozvoljava da se prosledi informacija bez HttpContext-a.")]
		public void Serbian() {
			HttpContext.Current = null; // our testbase initializes this, but it must be null to throw
			Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("sr");
			ErrorUtilities.VerifyHttpContext();
		}
	}
}
