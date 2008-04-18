using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DotNetOpenId.Extensions;

namespace DotNetOpenId.Test.Extensions {
	[TestFixture]
	public class AttributeExchangeFetchResponseTests {
		[Test]
		public void AddAttribute() {
			var response = new AttributeExchangeFetchResponse();
			response.AddAttribute(new AttributeResponse {
				TypeUri = "http://someattribute",
				Values = new[] { "Value1" },
			});
		}

		[Test]
		public void AddTwoAttributes() {
			var response = new AttributeExchangeFetchResponse();
			response.AddAttribute(new AttributeResponse {
				TypeUri = "http://someattribute",
				Values = new[] { "Value1" },
			});
			response.AddAttribute(new AttributeResponse {
				TypeUri = "http://someOtherAttribute",
				Values = new[] { "Value2" },
			});
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void AddAttributeTwice() {
			var response = new AttributeExchangeFetchResponse();
			response.AddAttribute(new AttributeResponse {
				TypeUri = "http://someattribute",
				Values = new[] { "Value1" },
			});
			response.AddAttribute(new AttributeResponse {
				TypeUri = "http://someattribute",
				Values = new[] { "Value1" },
			});
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void AddAttributeNull() {
			var response = new AttributeExchangeFetchResponse();
			response.AddAttribute(null);
		}
	}
}
