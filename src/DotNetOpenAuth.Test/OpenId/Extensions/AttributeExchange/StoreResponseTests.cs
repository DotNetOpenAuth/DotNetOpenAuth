//-----------------------------------------------------------------------
// <copyright file="StoreResponseTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Extensions.AttributeExchange {
	using System.IO;
	using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
	using NUnit.Framework;

	[TestFixture]
	public class StoreResponseTests {
		/// <summary>
		/// Verifies the constructor's behavior.
		/// </summary>
		[Test]
		public void Ctor() {
			var response = new StoreResponse();
			Assert.IsTrue(response.Succeeded, "The default status should be Succeeded.");
			Assert.IsNull(response.FailureReason);

			response = new StoreResponse("failed");
			Assert.IsFalse(response.Succeeded);
			Assert.AreEqual("failed", response.FailureReason);
		}

		/// <summary>
		/// Verifies the Equals method.
		/// </summary>
		[Test]
		public void EqualityTests() {
			var response1 = new StoreResponse();
			var response2 = new StoreResponse();
			Assert.AreEqual(response1, response2);

			response1.Succeeded = true;
			response2.Succeeded = false;
			Assert.AreNotEqual(response1, response2);

			response1.Succeeded = false;
			Assert.AreEqual(response1, response2);

			response1.FailureReason = "bad code";
			Assert.AreNotEqual(response1, response2);

			response2.FailureReason = "bad code";
			Assert.AreEqual(response1, response2);
		}

		/// <summary>
		/// Verifies that the class is serializable.
		/// </summary>
		[Test]
		public void Serializable() {
			var store = new StoreResponse();
			store.Succeeded = false;
			store.FailureReason = "some reason";
			var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
			var ms = new MemoryStream();
			formatter.Serialize(ms, store);
			ms.Position = 0;
			var store2 = formatter.Deserialize(ms);
			Assert.AreEqual(store, store2);
		}
	}
}
