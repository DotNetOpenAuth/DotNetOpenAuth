//-----------------------------------------------------------------------
// <copyright file="AttributeRequestTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Extensions {
	using System;
	using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
	using DotNetOpenAuth.Test.OpenId;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class AttributeRequestTests : OpenIdTestBase {
		[TestMethod]
		public void CtorDefault() {
			AttributeRequest req = new AttributeRequest();
			Assert.AreEqual(1, req.Count);
			Assert.IsNull(req.TypeUri);
			Assert.IsFalse(req.IsRequired);
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void CtorEmptyTypeUri() {
			new AttributeRequest(string.Empty);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullTypeUri() {
			new AttributeRequest(null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void CtorCountZero() {
			new AttributeRequest(WellKnownAttributes.Contact.Email, false, 0);
		}

		[TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void CtorCountNegative() {
			new AttributeRequest(WellKnownAttributes.Contact.Email, false, -1);
		}

		[TestMethod]
		public void CtorFull() {
			var req = new AttributeRequest(WellKnownAttributes.Contact.Email, true, 5);
			Assert.AreEqual(WellKnownAttributes.Contact.Email, req.TypeUri);
			Assert.IsTrue(req.IsRequired);
			Assert.AreEqual(5, req.Count);
		}

		[TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void SetCountZero() {
			var req = new AttributeRequest();
			req.Count = 0;
		}

		[TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void SetCountNegative() {
			var req = new AttributeRequest();
			req.Count = -1;
		}

		[TestMethod]
		public void EqualityTests() {
			var req1 = new AttributeRequest();
			var req2 = new AttributeRequest();
			Assert.AreEqual(req1, req2);

			req1.IsRequired = true;
			Assert.AreNotEqual(req1, req2);
			req2.IsRequired = true;
			Assert.AreEqual(req1, req2);

			req1.Count = 3;
			Assert.AreNotEqual(req1, req2);
			req2.Count = 3;
			Assert.AreEqual(req1, req2);

			req1.TypeUri = "http://hi";
			Assert.AreNotEqual(req1, req2);
			req2.TypeUri = "http://hi";
			Assert.AreEqual(req1, req2);
		}
	}
}
