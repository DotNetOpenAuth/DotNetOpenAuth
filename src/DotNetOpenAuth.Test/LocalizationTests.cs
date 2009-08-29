//-----------------------------------------------------------------------
// <copyright file="LocalizationTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test {
	using System;
	using System.Globalization;
	using System.Threading;
	using DotNetOpenAuth.Messaging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	/// <summary>
	/// Tests various localized resources work as expected.
	/// </summary>
	[TestClass]
	public class LocalizationTests {
		/// <summary>
		/// Tests that Serbian localized strings are correctly installed.
		/// </summary>
		[TestMethod, ExpectedException(typeof(InvalidOperationException), "Ovaj metod zahteva tekući HttpContext. Kao alternativa, koristite preklopljeni metod koji dozvoljava da se prosledi informacija bez HttpContext-a.")]
		public void Serbian() {
			Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("sr");
			ErrorUtilities.VerifyHttpContext();
		}
	}
}
