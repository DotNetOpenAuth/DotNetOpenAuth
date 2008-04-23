using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DotNetOpenId.Extensions.AttributeExchange;

namespace DotNetOpenId.Test.Extensions {
	[TestFixture]
	public class AttributeExchangeFetchResponseTests {
		[Test]
		public void AddAttribute() {
			var response = new FetchResponse();
			response.AddAttribute(new AttributeValues("http://someattribute", "Value1"));
		}

		[Test]
		public void AddTwoAttributes() {
			var response = new FetchResponse();
			response.AddAttribute(new AttributeValues("http://someattribute", "Value1"));
			response.AddAttribute(new AttributeValues("http://someOtherAttribute", "Value2"));
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void AddAttributeTwice() {
			var response = new FetchResponse();
			response.AddAttribute(new AttributeValues("http://someattribute", "Value1"));
			response.AddAttribute(new AttributeValues("http://someattribute", "Value1"));
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void AddAttributeNull() {
			var response = new FetchResponse();
			response.AddAttribute(null);
		}
	}
}
