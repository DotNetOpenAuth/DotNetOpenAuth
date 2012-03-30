//-----------------------------------------------------------------------
// <copyright file="AttributeValuesTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Extensions.AttributeExchange {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
	using NUnit.Framework;

	[TestFixture]
	public class AttributeValuesTests : OpenIdTestBase {
		[Test]
		public void Ctor() {
			var att = new AttributeValues();
			Assert.IsNull(att.TypeUri);
			Assert.IsNotNull(att.Values);
			Assert.AreEqual(0, att.Values.Count);

			att = new AttributeValues("http://att");
			Assert.AreEqual("http://att", att.TypeUri);
			Assert.IsNotNull(att.Values);
			Assert.AreEqual(0, att.Values.Count);

			att = new AttributeValues("http://att", "value1", "value2");
			Assert.AreEqual("http://att", att.TypeUri);
			Assert.IsNotNull(att.Values);
			Assert.AreEqual(2, att.Values.Count);
			Assert.AreEqual("value1", att.Values[0]);
			Assert.AreEqual("value2", att.Values[1]);
		}

		/// <summary>
		/// Verifies the Equals method.
		/// </summary>
		[Test]
		public void EqualityTests() {
			var att1 = new AttributeValues();
			var att2 = new AttributeValues();
			Assert.AreEqual(att1, att2);

			att1.TypeUri = "http://att1";
			Assert.AreNotEqual(att1, att2);
			att2.TypeUri = "http://att1";
			Assert.AreEqual(att1, att2);

			att1.Values.Add("value1");
			Assert.AreNotEqual(att1, att2);
			att2.Values.Add("value1");
			Assert.AreEqual(att1, att2);

			// Values that are out of order should not be considered equal.
			att1.Values.Add("value2");
			att2.Values.Insert(0, "value2");
			Assert.AreNotEqual(att1, att2);
		}
	}
}
