using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DotNetOpenId.Extensions;

namespace DotNetOpenId.Test.Extensions {
	[TestFixture]
	public class SimpleRegistrationTests : ExtensionTestBase {
		[Test]
		public void None() {
			var response = ParameterizedTest<SimpleRegistrationFieldValues>(
				TestSupport.GetIdentityUrl(TestSupport.Scenarios.ExtensionFullCooperation, Version), null);
			Assert.IsNull(response);
		}

		[Test]
		public void Full() {
			var request = new SimpleRegistrationRequestFields();
			request.FullName = SimpleRegistrationRequest.Request;
			request.Email = SimpleRegistrationRequest.Require;
			var response = ParameterizedTest<SimpleRegistrationFieldValues>(
				TestSupport.GetIdentityUrl(TestSupport.Scenarios.ExtensionFullCooperation, Version), request);
			Assert.AreEqual("Andrew Arnott", response.FullName);
			Assert.AreEqual("andrewarnott@gmail.com", response.Email);
		}
		[Test]
		public void Partial() {
			var request = new SimpleRegistrationRequestFields();
			request.FullName = SimpleRegistrationRequest.Request;
			request.Email = SimpleRegistrationRequest.Require;
			var response = ParameterizedTest<SimpleRegistrationFieldValues>(
				TestSupport.GetIdentityUrl(TestSupport.Scenarios.ExtensionPartialCooperation, Version), request);
			Assert.IsNull(response.FullName);
			Assert.AreEqual("andrewarnott@gmail.com", response.Email);
		}
	}
}
