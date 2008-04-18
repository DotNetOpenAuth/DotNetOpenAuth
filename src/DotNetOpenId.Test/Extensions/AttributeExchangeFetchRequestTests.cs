using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DotNetOpenId.Extensions;

namespace DotNetOpenId.Test.Extensions {
	[TestFixture]
	public class AttributeExchangeFetchRequestTests {
		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void AddAttributeRequestNull() {
			new AttributeExchangeFetchRequest().AddAttribute(null);
		}

		[Test]
		public void AddAttributeRequest() {
			var req = new AttributeExchangeFetchRequest();
			req.AddAttribute(new AttributeRequest() { TypeUri = "http://someUri" });
		}

		[Test]
		public void AddAttributeRequestStrangeUri() {
			var req = new AttributeExchangeFetchRequest();
			req.AddAttribute(new AttributeRequest() { TypeUri = "=someUri*who*knows*but*this*is*legal" });
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void AddAttributeRequestAgain() {
			var req = new AttributeExchangeFetchRequest();
			req.AddAttribute(new AttributeRequest() { TypeUri = "http://UriTwice" });
			req.AddAttribute(new AttributeRequest() { TypeUri = "http://UriTwice" });
		}

		[Test]
		public void RespondSimpleValue() {
			var req = new AttributeRequest();
			req.TypeUri = "http://someType";
			var resp = req.Respond("value");
			Assert.AreEqual(req.TypeUri, resp.TypeUri);
			Assert.AreEqual(1, resp.Values.Length);
			Assert.AreEqual("value", resp.Values[0]);
		}

		[Test]
		public void RespondTwoValues() {
			var req = new AttributeRequest();
			req.TypeUri = "http://someType";
			req.Count = 2;
			var resp = req.Respond("value1", "value2");
			Assert.AreEqual(req.TypeUri, resp.TypeUri);
			Assert.AreEqual(2, resp.Values.Length);
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
	}
}
