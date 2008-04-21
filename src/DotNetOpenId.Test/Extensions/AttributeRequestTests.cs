using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DotNetOpenId.Extensions;

namespace DotNetOpenId.Test.Extensions {
	[TestFixture]
	public class AttributeRequestTests {
		[Test]
		public void CtorDefault() {
			AttributeRequest req = new AttributeRequest();
			Assert.AreEqual(1, req.Count);
			Assert.IsNull(req.TypeUri);
			Assert.IsFalse(req.IsRequired);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CtorEmptyTypeUri() {
			new AttributeRequest(string.Empty);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullTypeUri() {
			new AttributeRequest(null);
		}

		[Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void CtorCountZero() {
			new AttributeRequest(AttributeExchangeConstants.Contact.Email, false, 0);
		}

		[Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void CtorCountNegative() {
			new AttributeRequest(AttributeExchangeConstants.Contact.Email, false, -1);
		}

		[Test]
		public void CtorFull() {
			var req = new AttributeRequest(AttributeExchangeConstants.Contact.Email, true, 5);
			Assert.AreEqual(AttributeExchangeConstants.Contact.Email, req.TypeUri);
			Assert.IsTrue(req.IsRequired);
			Assert.AreEqual(5, req.Count);
		}

		[Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void SetCountZero() {
			var req = new AttributeRequest();
			req.Count = 0;
		}

		[Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void SetCountNegative() {
			var req = new AttributeRequest();
			req.Count = -1;
		}
	}
}
