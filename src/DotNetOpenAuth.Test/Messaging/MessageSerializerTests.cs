//-----------------------------------------------------------------------
// <copyright file="MessageSerializerTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Xml;
	using DotNetOpenAuth.Messaging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	/// <summary>
	/// Tests for the <see cref="MessageSerializer"/> class.
	/// </summary>
	[TestClass()]
	public class MessageSerializerTests : MessagingTestBase {
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void SerializeNull() {
			var serializer = MessageSerializer.Get(typeof(Mocks.TestMessage));
			serializer.Serialize(null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void GetInvalidMessageType() {
			MessageSerializer.Get(typeof(string));
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void GetNullType() {
			MessageSerializer.Get(null);
		}

		[TestMethod()]
		public void SerializeTest() {
			var serializer = MessageSerializer.Get(typeof(Mocks.TestMessage));
			var message = GetStandardTestMessage(FieldFill.CompleteBeforeBindings);
			var expected = GetStandardTestFields(FieldFill.CompleteBeforeBindings);
			IDictionary<string, string> actual = serializer.Serialize(this.MessageDescriptions.GetAccessor(message));
			Assert.AreEqual(4, actual.Count);

			// Test case sensitivity of generated dictionary
			Assert.IsFalse(actual.ContainsKey("Age"));
			Assert.IsTrue(actual.ContainsKey("age"));

			// Test contents of dictionary
			Assert.AreEqual(expected["age"], actual["age"]);
			Assert.AreEqual(expected["Name"], actual["Name"]);
			Assert.AreEqual(expected["Location"], actual["Location"]);
			Assert.AreEqual(expected["Timestamp"], actual["Timestamp"]);
			Assert.IsFalse(actual.ContainsKey("EmptyMember"));
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void DeserializeNull() {
			var serializer = MessageSerializer.Get(typeof(Mocks.TestMessage));
			serializer.Deserialize(null, null);
		}

		[TestMethod]
		public void DeserializeSimple() {
			var serializer = MessageSerializer.Get(typeof(Mocks.TestMessage));
			Dictionary<string, string> fields = new Dictionary<string, string>(StringComparer.Ordinal);
			fields["Name"] = "Andrew";
			fields["age"] = "15";
			fields["Timestamp"] = "1990-01-01T00:00:00";
			var actual = new Mocks.TestDirectedMessage();
			serializer.Deserialize(fields, this.MessageDescriptions.GetAccessor(actual));
			Assert.AreEqual(15, actual.Age);
			Assert.AreEqual("Andrew", actual.Name);
			Assert.AreEqual(DateTime.Parse("1/1/1990"), actual.Timestamp);
			Assert.IsNull(actual.EmptyMember);
		}

		/// <summary>
		/// This tests deserialization of a message that is comprised of [MessagePart]'s
		/// that are defined in multiple places in the inheritance tree.
		/// </summary>
		/// <remarks>
		/// The element sorting rules are first inheritance order, then alphabetical order.
		/// This test validates correct behavior on both.
		/// </remarks>
		[TestMethod]
		public void DeserializeVerifyElementOrdering() {
			var serializer = MessageSerializer.Get(typeof(Mocks.TestDerivedMessage));
			Dictionary<string, string> fields = new Dictionary<string, string>(StringComparer.Ordinal);
			// We deliberately do this OUT of order,
			// since DataContractSerializer demands elements to be in 
			// 1) inheritance then 2) alphabetical order.
			// Proper xml element order would be: Name, age, Second..., TheFirst...
			fields["TheFirstDerivedElement"] = "first";
			fields["age"] = "15";
			fields["Name"] = "Andrew";
			fields["SecondDerivedElement"] = "second";
			fields["explicit"] = "explicitValue";
			fields["private"] = "privateValue";
			var actual = new Mocks.TestDerivedMessage();
			serializer.Deserialize(fields, this.MessageDescriptions.GetAccessor(actual));
			Assert.AreEqual(15, actual.Age);
			Assert.AreEqual("Andrew", actual.Name);
			Assert.AreEqual("first", actual.TheFirstDerivedElement);
			Assert.AreEqual("second", actual.SecondDerivedElement);
			Assert.AreEqual("explicitValue", ((Mocks.IBaseMessageExplicitMembers)actual).ExplicitProperty);
			Assert.AreEqual("privateValue", actual.PrivatePropertyAccessor);
		}

		[TestMethod]
		public void DeserializeWithExtraFields() {
			var serializer = MessageSerializer.Get(typeof(Mocks.TestMessage));
			Dictionary<string, string> fields = new Dictionary<string, string>(StringComparer.Ordinal);
			fields["age"] = "15";
			fields["Name"] = "Andrew";
			fields["Timestamp"] = XmlConvert.ToString(DateTime.UtcNow, XmlDateTimeSerializationMode.Utc);
			// Add some field that is not recognized by the class.  This simulates a querystring with
			// more parameters than are actually interesting to the protocol message.
			fields["someExtraField"] = "asdf";
			var actual = new Mocks.TestDirectedMessage();
			serializer.Deserialize(fields, this.MessageDescriptions.GetAccessor(actual));
			Assert.AreEqual(15, actual.Age);
			Assert.AreEqual("Andrew", actual.Name);
			Assert.IsNull(actual.EmptyMember);
		}

		[TestMethod, ExpectedException(typeof(ProtocolException))]
		public void DeserializeInvalidMessage() {
			IProtocolMessage message = new Mocks.TestDirectedMessage();
			var serializer = MessageSerializer.Get(message.GetType());
			var fields = GetStandardTestFields(FieldFill.AllRequired);
			fields["age"] = "-1"; // Set an disallowed value.
			serializer.Deserialize(fields, this.MessageDescriptions.GetAccessor(message));
		}
	}
}
