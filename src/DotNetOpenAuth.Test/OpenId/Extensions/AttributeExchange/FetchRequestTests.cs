//-----------------------------------------------------------------------
// <copyright file="FetchRequestTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Extensions {
	using System;
	using System.IO;
	using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
	using DotNetOpenAuth.Test.OpenId;
	using NUnit.Framework;

	[TestFixture]
	public class FetchRequestTests : OpenIdTestBase {
		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void AddAttributeRequestNull() {
			new FetchRequest().Attributes.Add(null);
		}

		[Test]
		public void AddAttributeRequest() {
			var req = new FetchRequest();
			req.Attributes.Add(new AttributeRequest() { TypeUri = "http://someUri" });
		}

		[Test]
		public void AddAttributeRequestStrangeUri() {
			var req = new FetchRequest();
			req.Attributes.Add(new AttributeRequest() { TypeUri = "=someUri*who*knows*but*this*is*legal" });
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void AddAttributeRequestAgain() {
			var req = new FetchRequest();
			req.Attributes.Add(new AttributeRequest() { TypeUri = "http://UriTwice" });
			req.Attributes.Add(new AttributeRequest() { TypeUri = "http://UriTwice" });
		}

		[Test]
		public void RespondSimpleValue() {
			var req = new AttributeRequest();
			req.TypeUri = "http://someType";
			var resp = req.Respond("value");
			Assert.AreEqual(req.TypeUri, resp.TypeUri);
			Assert.AreEqual(1, resp.Values.Count);
			Assert.AreEqual("value", resp.Values[0]);
		}

		[Test]
		public void RespondTwoValues() {
			var req = new AttributeRequest();
			req.TypeUri = "http://someType";
			req.Count = 2;
			var resp = req.Respond("value1", "value2");
			Assert.AreEqual(req.TypeUri, resp.TypeUri);
			Assert.AreEqual(2, resp.Values.Count);
			Assert.AreEqual("value1", resp.Values[0]);
			Assert.AreEqual("value2", resp.Values[1]);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void RespondTooManyValues() {
			var req = new AttributeRequest();
			req.TypeUri = "http://someType";
			req.Count = 1;
			req.Respond("value1", "value2");
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void RespondNull() {
			var req = new AttributeRequest();
			req.TypeUri = "http://someType";
			req.Count = 1;
			req.Respond(null);
		}

		[Test]
		public void EqualityTests() {
			var req1 = new FetchRequest();
			var req2 = new FetchRequest();
			Assert.AreEqual(req1, req2);

			req1.UpdateUrl = new Uri("http://hi");
			Assert.AreNotEqual(req1, req2);
			req2.UpdateUrl = new Uri("http://hi");
			Assert.AreEqual(req1, req2);

			// Add attributes in different orders deliberately.
			req1.Attributes.Add(new AttributeRequest("http://att1"));
			Assert.AreNotEqual(req1, req2);
			req2.Attributes.Add(new AttributeRequest("http://att2"));
			Assert.AreNotEqual(req1, req2);
			req1.Attributes.Add(new AttributeRequest("http://att2"));
			Assert.AreNotEqual(req1, req2);
			req2.Attributes.Add(new AttributeRequest("http://att1"));
			Assert.AreEqual(req1, req2);
		}

		/// <summary>
		/// Verifies that the class is serializable.
		/// </summary>
		[Test]
		public void Serializable() {
			var fetch = new FetchRequest();
			fetch.Attributes.AddRequired("http://someAttribute");
			var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
			var ms = new MemoryStream();
			formatter.Serialize(ms, fetch);
			ms.Position = 0;
			var fetch2 = formatter.Deserialize(ms);
			Assert.AreEqual(fetch, fetch2);
		}
	}
}
