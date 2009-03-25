//-----------------------------------------------------------------------
// <copyright file="FetchResponseTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenId.Test.OpenId.Extensions {
	using System;
	using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
	using DotNetOpenAuth.Test.OpenId;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class FetchResponseTests : OpenIdTestBase {
		[TestMethod]
		public void AddAttribute() {
			var response = new FetchResponse();
			response.Attributes.Add(new AttributeValues("http://someattribute", "Value1"));
		}

		[TestMethod]
		public void AddTwoAttributes() {
			var response = new FetchResponse();
			response.Attributes.Add(new AttributeValues("http://someattribute", "Value1"));
			response.Attributes.Add(new AttributeValues("http://someOtherAttribute", "Value2"));
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void AddAttributeTwice() {
			var response = new FetchResponse();
			response.Attributes.Add(new AttributeValues("http://someattribute", "Value1"));
			response.Attributes.Add(new AttributeValues("http://someattribute", "Value1"));
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void AddAttributeNull() {
			var response = new FetchResponse();
			response.Attributes.Add(null);
		}

		[TestMethod]
		public void EqualityTests() {
			var response1 = new FetchResponse();
			var response2 = new FetchResponse();
			Assert.AreEqual(response1, response2);

			response1.UpdateUrl = new Uri("http://updateurl");
			Assert.AreNotEqual(response1, response2);
			response2.UpdateUrl = new Uri("http://updateurl");
			Assert.AreEqual(response1, response2);

			// Add attributes in different orders deliberately.
			response1.Attributes.Add(new AttributeValues("http://att1"));
			Assert.AreNotEqual(response1, response2);
			response2.Attributes.Add(new AttributeValues("http://att2"));
			Assert.AreNotEqual(response1, response2);
			response1.Attributes.Add(new AttributeValues("http://att2"));
			Assert.AreNotEqual(response1, response2);
			response2.Attributes.Add(new AttributeValues("http://att1"));
			Assert.AreEqual(response1, response2);
		}
	}
}
