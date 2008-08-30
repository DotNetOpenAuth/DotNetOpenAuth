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
		public void SerializeNullFields() {
			var serializer = new ProtocolMessageSerializer<Mocks.TestMessage>();
			serializer.Serialize(null, new Mocks.TestMessage());
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void SerializeNullMessage() {
			var serializer = new ProtocolMessageSerializer<Mocks.TestMessage>();
			serializer.Serialize(new Dictionary<string, string>(), null);
		}

		[TestMethod, ExpectedException(typeof(ProtocolException))]
		public void SerializeInvalidMessage() {
			var serializer = new ProtocolMessageSerializer<Mocks.TestMessage>();
			Dictionary<string, string> fields = new Dictionary<string, string>(StringComparer.Ordinal);
			Mocks.TestMessage message = new DotNetOAuth.Test.Mocks.TestMessage();
			message.EmptyMember = "invalidvalue";
			serializer.Serialize(message);
		}

		[TestMethod()]
		public void SerializeTest() {
			var serializer = new ProtocolMessageSerializer<Mocks.TestMessage>();
			var message = new Mocks.TestMessage { Age = 15, Name = "Andrew", Location = new Uri("http://localhost") };
			IDictionary<string, string> actual = serializer.Serialize(message);
			Assert.AreEqual(3, actual.Count);

			// Test case sensitivity of generated dictionary
			Assert.IsFalse(actual.ContainsKey("Age"));
			Assert.IsTrue(actual.ContainsKey("age"));

			// Test contents of dictionary
			Assert.AreEqual("15", actual["age"]);
			Assert.AreEqual("Andrew", actual["Name"]);
			Assert.AreEqual("http://localhost/", actual["Location"]);
			Assert.IsFalse(actual.ContainsKey("EmptyMember"));
		}

		[TestMethod]
		public void SerializeToExistingDictionary() {
			var serializer = new ProtocolMessageSerializer<Mocks.TestMessage>();
			var message = new Mocks.TestMessage { Age = 15, Name = "Andrew" };
			var fields = new Dictionary<string, string>();
			fields["someExtraField"] = "someValue";
			serializer.Serialize(fields, message);
			Assert.AreEqual(3, fields.Count);
			Assert.AreEqual("15", fields["age"]);
			Assert.AreEqual("Andrew", fields["Name"]);
			Assert.AreEqual("someValue", fields["someExtraField"]);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void DeserializeNull() {
			var serializer = new ProtocolMessageSerializer<Mocks.TestMessage>();
			serializer.Deserialize(null);
		}

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
			fields["Name"] = "Andrew";
			// Add some field that is not recognized by the class.  This simulates a querystring with
			// more parameters than are actually interesting to the protocol message.
			fields["someExtraField"] = "asdf";
			var actual = serializer.Deserialize(fields);
			Assert.AreEqual(15, actual.Age);
			Assert.AreEqual("Andrew", actual.Name);
			Assert.IsNull(actual.EmptyMember);
		}

		[TestMethod()]
		public void DeserializeEmpty() {
			var serializer = new ProtocolMessageSerializer<Mocks.TestMessage>();
			Dictionary<string, string> fields = new Dictionary<string, string>(StringComparer.Ordinal);
			var actual = serializer.Deserialize(fields);
			Assert.AreEqual(0, actual.Age);
			Assert.IsNull(actual.Name);
			Assert.IsNull(actual.EmptyMember);
		}

		[TestMethod, ExpectedException(typeof(ProtocolException))]
		public void DeserializeInvalidMessage() {
			var serializer = new ProtocolMessageSerializer<Mocks.TestMessage>();
			Dictionary<string, string> fields = new Dictionary<string, string>(StringComparer.Ordinal);
			// Put in a value where the field should be empty.
			fields["EmptyMember"] = "15";
			serializer.Deserialize(fields);
		}
	}
}
