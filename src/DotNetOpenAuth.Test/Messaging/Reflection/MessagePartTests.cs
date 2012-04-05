//-----------------------------------------------------------------------
// <copyright file="MessagePartTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Messaging.Reflection {
	using System;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.Test.Mocks;
	using NUnit.Framework;

	[TestFixture]
	public class MessagePartTests : MessagingTestBase {
		[Test, ExpectedException(typeof(ArgumentException))]
		public void OptionalNonNullableStruct() {
			this.ParameterizedMessageTypeTest(typeof(MessageWithNonNullableOptionalStruct));
		}

		[Test]
		public void RequiredNonNullableStruct() {
			this.ParameterizedMessageTypeTest(typeof(MessageWithNonNullableRequiredStruct));
		}

		[Test]
		public void OptionalNullableStruct() {
			var message = new MessageWithNullableOptionalStruct();
			var part = this.ParameterizedMessageTypeTest(message.GetType());

			Assert.IsNull(part.GetValue(message));
			part.SetValue(message, "3");
			Assert.AreEqual("3", part.GetValue(message));
		}

		[Test]
		public void RequiredNullableStruct() {
			this.ParameterizedMessageTypeTest(typeof(MessageWithNullableRequiredStruct));
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullMember() {
			new MessagePart(null, new MessagePartAttribute());
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullAttribute() {
			PropertyInfo field = typeof(MessageWithNullableOptionalStruct).GetProperty("OptionalInt", BindingFlags.NonPublic | BindingFlags.Instance);
			new MessagePart(field, null);
		}

		[Test]
		public void SetValue() {
			var message = new MessageWithNonNullableRequiredStruct();
			MessagePart part = this.ParameterizedMessageTypeTest(message.GetType());
			part.SetValue(message, "5");
			Assert.AreEqual(5, message.OptionalInt);
		}

		[Test]
		public void GetValue() {
			var message = new MessageWithNonNullableRequiredStruct();
			message.OptionalInt = 8;
			MessagePart part = this.ParameterizedMessageTypeTest(message.GetType());
			Assert.AreEqual("8", part.GetValue(message));
		}

		[Test]
		public void Base64Member() {
			var message = new MessageWithBase64EncodedString();
			message.LastName = "andrew";
			MessagePart part = GetMessagePart(message.GetType(), "nameBytes");
			Assert.AreEqual("YW5kcmV3", part.GetValue(message));
			part.SetValue(message, "YXJub3R0");
			Assert.AreEqual("arnott", message.LastName);
		}

		[Test]
		public void ConstantFieldMemberValidValues() {
			var message = new MessageWithConstantField();
			MessagePart part = GetMessagePart(message.GetType(), "ConstantField");
			Assert.AreEqual("abc", part.GetValue(message));
			part.SetValue(message, "abc");
			Assert.AreEqual("abc", part.GetValue(message));
		}

		[Test, ExpectedException(typeof(ProtocolException))]
		public void ConstantFieldMemberInvalidValues() {
			var message = new MessageWithConstantField();
			MessagePart part = GetMessagePart(message.GetType(), "ConstantField");
			part.SetValue(message, "def");
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void NonFieldOrPropertyMember() {
			MemberInfo method = typeof(MessageWithNullableOptionalStruct).GetMethod("Equals", BindingFlags.Public | BindingFlags.Instance);
			new MessagePart(method, new MessagePartAttribute());
		}

		[Test]
		public void RequiredMinAndMaxVersions() {
			Type messageType = typeof(MessageWithMinAndMaxVersionParts);
			FieldInfo newIn2Field = messageType.GetField("NewIn2", BindingFlags.Public | BindingFlags.Instance);
			MessagePartAttribute newIn2Attribute = newIn2Field.GetCustomAttributes(typeof(MessagePartAttribute), true).OfType<MessagePartAttribute>().Single();

			FieldInfo removedIn3Field = messageType.GetField("RemovedIn3", BindingFlags.Public | BindingFlags.Instance);
			MessagePartAttribute removedIn3Attribute = removedIn3Field.GetCustomAttributes(typeof(MessagePartAttribute), true).OfType<MessagePartAttribute>().Single();

			Assert.AreEqual(new Version(2, 0), newIn2Attribute.MinVersionValue);
			Assert.AreEqual(new Version(2, 5), removedIn3Attribute.MaxVersionValue);
		}

		private static MessagePart GetMessagePart(Type messageType, string memberName) {
			FieldInfo field = messageType.GetField(memberName, BindingFlags.NonPublic | BindingFlags.Instance);
			MessagePartAttribute attribute = field.GetCustomAttributes(typeof(MessagePartAttribute), true).OfType<MessagePartAttribute>().Single();
			return new MessagePart(field, attribute);
		}

		private MessagePart ParameterizedMessageTypeTest(Type messageType) {
			PropertyInfo field = messageType.GetProperty("OptionalInt", BindingFlags.NonPublic | BindingFlags.Instance);
			MessagePartAttribute attribute = field.GetCustomAttributes(typeof(MessagePartAttribute), true).OfType<MessagePartAttribute>().Single();
			return new MessagePart(field, attribute);
		}

		private class MessageWithNonNullableOptionalStruct : TestMessage {
			// Optional structs like int must be nullable for Optional to make sense.
			[MessagePart(IsRequired = false)]
			internal int OptionalInt { get; set; }
		}

		private class MessageWithNonNullableRequiredStruct : TestMessage {
			// This should work because a required field will always have a value so it
			// need not be nullable.
			[MessagePart(IsRequired = true)]
			internal int OptionalInt { get; set; }
		}

		private class MessageWithNullableOptionalStruct : TestMessage {
			// Optional structs like int must be nullable for Optional to make sense.
			[MessagePart(IsRequired = false)]
			internal int? OptionalInt { get; set; }
		}

		private class MessageWithNullableRequiredStruct : TestMessage {
			[MessagePart(IsRequired = true)]
			private int? OptionalInt { get; set; }
		}

		private class MessageWithBase64EncodedString : TestMessage {
			[MessagePart]
			private byte[] nameBytes;

			public string LastName {
				get { return this.nameBytes != null ? Encoding.UTF8.GetString(this.nameBytes) : null; }
				set { this.nameBytes = value != null ? Encoding.UTF8.GetBytes(value) : null; }
			}
		}

		private class MessageWithConstantField : TestMessage {
			[MessagePart(IsRequired = true)]
#pragma warning disable 0414 // read by reflection
			private readonly string ConstantField = "abc";
#pragma warning restore 0414
		}

		private class MessageWithMinAndMaxVersionParts : TestMessage {
#pragma warning disable 0649 // written to by reflection
			[MessagePart(MinVersion = "2.0")]
			public string NewIn2;

			[MessagePart(MaxVersion = "2.5")]
			public string RemovedIn3;
#pragma warning restore 0649
		}
	}
}
