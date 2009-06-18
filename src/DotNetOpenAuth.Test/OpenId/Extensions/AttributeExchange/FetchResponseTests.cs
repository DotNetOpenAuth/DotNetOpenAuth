//-----------------------------------------------------------------------
// <copyright file="FetchResponseTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Extensions {
	using System;
	using System.IO;
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
		public void GetAttributeValue() {
			var response = new FetchResponse();

			// Verify that null is returned if the attribute is absent.
			Assert.IsNull(response.GetAttributeValue("http://someattribute"));

			// Now add an attribute with no values.
			response.Attributes.Add(new AttributeValues("http://someattribute2"));
			Assert.IsNull(response.GetAttributeValue("http://someattribute2"));

			// Now add an attribute with many values.
			response.Attributes.Add(new AttributeValues("http://someattribute3", "a", "b", "c"));
			Assert.AreEqual("a", response.GetAttributeValue("http://someattribute3"));
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

		/// <summary>
		/// Verifies that the class is serializable.
		/// </summary>
		[TestMethod]
		public void Serializable() {
			var fetch = new FetchResponse();
			fetch.Attributes.Add("http://someAttribute", "val1", "val2");
			var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
			var ms = new MemoryStream();
			formatter.Serialize(ms, fetch);
			ms.Position = 0;
			var fetch2 = formatter.Deserialize(ms);
			Assert.AreEqual(fetch, fetch2);
		}
	}
}
