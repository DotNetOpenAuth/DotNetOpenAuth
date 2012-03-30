//-----------------------------------------------------------------------
// <copyright file="StoreRequestTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Extensions.AttributeExchange {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
	using NUnit.Framework;

	[TestFixture]
	public class StoreRequestTests {
		/// <summary>
		/// Verifies the constructor behavior.
		/// </summary>
		[Test]
		public void Ctor() {
			var req = new StoreRequest();
			Assert.IsFalse(req.Attributes.Any());
		}

		/// <summary>
		/// Verifies the AddAttribute method.
		/// </summary>
		[Test]
		public void AddAttributeByValue() {
			var req = new StoreRequest();
			AttributeValues value = new AttributeValues();
			req.Attributes.Add(value);
			Assert.AreSame(value, req.Attributes.Single());
		}

		/// <summary>
		/// Verifies the AddAttribute method.
		/// </summary>
		[Test]
		public void AddAttributeByPrimitives() {
			var req = new StoreRequest();
			req.Attributes.Add("http://att1", "value1", "value2");
			AttributeValues value = req.Attributes.Single();
			Assert.AreEqual("http://att1", value.TypeUri);
			Assert.IsTrue(MessagingUtilities.AreEquivalent(new[] { "value1", "value2" }, value.Values));
		}

		/// <summary>
		/// Verifies the Equals method.
		/// </summary>
		[Test]
		public void EqualityTests() {
			var req1 = new StoreRequest();
			var req2 = new StoreRequest();
			Assert.AreEqual(req1, req2);

			// Add attributes in different orders deliberately.
			req1.Attributes.Add("http://att1");
			Assert.AreNotEqual(req1, req2);
			req2.Attributes.Add("http://att2");
			Assert.AreNotEqual(req1, req2);
			req1.Attributes.Add("http://att2");
			Assert.AreNotEqual(req1, req2);
			req2.Attributes.Add("http://att1");
			Assert.AreEqual(req1, req2);
		}

		/// <summary>
		/// Verifies that the class is serializable.
		/// </summary>
		[Test]
		public void Serializable() {
			var store = new StoreRequest();
			store.Attributes.Add("http://someAttribute", "val1", "val2");
			var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
			var ms = new MemoryStream();
			formatter.Serialize(ms, store);
			ms.Position = 0;
			var store2 = formatter.Deserialize(ms);
			Assert.AreEqual(store, store2);
		}
	}
}
