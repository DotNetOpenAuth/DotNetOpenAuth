//-----------------------------------------------------------------------
// <copyright file="SimpleRegistrationTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Extensions {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class SimpleRegistrationTests : ExtensionTestBase {
		[TestMethod]
		public void Simple() {
			ClaimsRequest request = new ClaimsRequest() { Nickname = DemandLevel.Request };
			ClaimsResponse response = new ClaimsResponse(Constants.sreg_ns);
			response.Nickname = "Andrew";
			Roundtrip(Protocol.Default, new[] { request }, new[] { response });
		}

		////[TestMethod]
		////public void None() {
		////    var response = ParameterizedTest<ClaimsResponse>(
		////        TestSupport.Scenarios.ExtensionFullCooperation, Version, null);
		////    Assert.IsNull(response);
		////}

		////[TestMethod]
		////public void Full() {
		////    var request = new ClaimsRequest();
		////    request.FullName = DemandLevel.Request;
		////    request.Email = DemandLevel.Require;
		////    var response = ParameterizedTest<ClaimsResponse>(
		////        TestSupport.Scenarios.ExtensionFullCooperation, Version, request);
		////    Assert.AreEqual("Andrew Arnott", response.FullName);
		////    Assert.AreEqual("andrewarnott@gmail.com", response.Email);
		////}
		////[TestMethod]
		////public void Partial() {
		////    var request = new ClaimsRequest();
		////    request.FullName = DemandLevel.Request;
		////    request.Email = DemandLevel.Require;
		////    var response = ParameterizedTest<ClaimsResponse>(
		////        TestSupport.Scenarios.ExtensionPartialCooperation, Version, request);
		////    Assert.IsNull(response.FullName);
		////    Assert.AreEqual("andrewarnott@gmail.com", response.Email);
		////}

		////[TestMethod]
		////public void Birthdates() {
		////    var response = new ClaimsResponse();
		////    // Verify that they both start out as null
		////    Assert.IsNull(response.BirthDateRaw);
		////    Assert.IsFalse(response.BirthDate.HasValue);

		////    // Verify that null can be set.
		////    response.BirthDate = null;
		////    response.BirthDateRaw = null;
		////    Assert.IsNull(response.BirthDateRaw);
		////    Assert.IsFalse(response.BirthDate.HasValue);

		////    // Verify that the strong-typed BirthDate property can be set and that it affects the raw property.
		////    response.BirthDate = DateTime.Parse("April 4, 1984");
		////    Assert.AreEqual(4, response.BirthDate.Value.Month);
		////    Assert.AreEqual("1984-04-04", response.BirthDateRaw);

		////    // Verify that the raw property can be set with a complete birthdate and that it affects the strong-typed property.
		////    response.BirthDateRaw = "1998-05-08";
		////    Assert.AreEqual("1998-05-08", response.BirthDateRaw);
		////    Assert.AreEqual(DateTime.Parse("May 8, 1998", CultureInfo.InvariantCulture), response.BirthDate);

		////    // Verify that an partial raw birthdate works, and sets the strong-typed property to null since it cannot be represented.
		////    response.BirthDateRaw = "2000-00-00";
		////    Assert.AreEqual("2000-00-00", response.BirthDateRaw);
		////    Assert.IsFalse(response.BirthDate.HasValue);
		////}

		////[TestMethod, ExpectedException(typeof(ArgumentException))]
		////public void InvalidRawBirthdate() {
		////    var response = new ClaimsResponse();
		////    response.BirthDateRaw = "2008";
		////}
	}
}
