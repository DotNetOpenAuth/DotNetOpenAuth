using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetOAuth.Test {
	[TestClass()]
	public class MessageSerializerTest : TestBase {
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void SerializeNull() {
			var serializer = new ProtocolMessageSerializer<Mocks.TestMessage>();
			serializer.Serialize(null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void DeserializeNull() {
			var serializer = new ProtocolMessageSerializer<Mocks.TestMessage>();
			serializer.Deserialize(null);
		}

		/// <summary>
		/// A test for Deserialize
		/// </summary>
		[TestMethod()]
		public void DeserializeSimple() {
			var serializer = new ProtocolMessageSerializer<Mocks.TestMessage>();
			Dictionary<string, string> fields = new Dictionary<string, string>(StringComparer.Ordinal);
			// We deliberately do this OUT of alphabetical order (caps would go first),
			// since DataContractSerializer demands things to be IN alphabetical order.
			fields["age"] = "15";
			fields["Name"] = "Andrew";
			var actual = serializer.Deserialize(fields);
			Assert.AreEqual(15, actual.Age);
			Assert.AreEqual("Andrew", actual.Name);
			Assert.IsNull(actual.EmptyMember);
		}

		[TestMethod]
		public void DeserializeWithExtraFields() {
			var serializer = new ProtocolMessageSerializer<Mocks.TestMessage>();
			Dictionary<string, string> fields = new Dictionary<string, string>(StringComparer.Ordinal);
			fields["age"] = "15";
			// Add some field that is not recognized by the class.  This simulates a querystring with
			// more parameters than are actually interesting to the protocol message.
			fields["someExtraField"] = "asdf";
			var actual = serializer.Deserialize(fields);
			Assert.AreEqual(15, actual.Age);
			Assert.IsNull(actual.Name);
			Assert.IsNull(actual.EmptyMember);
		}

		/// <summary>
		/// A test for Deserialize
		/// </summary>
		[TestMethod()]
		public void DeserializeEmpty() {
			var serializer = new ProtocolMessageSerializer<Mocks.TestMessage>();
			Dictionary<string, string> fields = new Dictionary<string, string>(StringComparer.Ordinal);
			var actual = serializer.Deserialize(fields);
			Assert.AreEqual(0, actual.Age);
		}

		/// <summary>
		/// A test for Serialize
		/// </summary>
		[TestMethod()]
		public void SerializeTest() {
			var serializer = new ProtocolMessageSerializer<Mocks.TestMessage>();
			var message = new Mocks.TestMessage { Age = 15, Name = "Andrew" };
			IDictionary<string, string> actual = serializer.Serialize(message);
			Assert.AreEqual(2, actual.Count);

			// Test case sensitivity of generated dictionary
			Assert.IsFalse(actual.ContainsKey("Age"));
			Assert.IsTrue(actual.ContainsKey("age"));

			// Test contents of dictionary
			Assert.AreEqual("15", actual["age"]);
			Assert.AreEqual("Andrew", actual["Name"]);
			Assert.IsFalse(actual.ContainsKey("EmptyMember"));
		}
	}
}
