//-----------------------------------------------------------------------
// <copyright file="MessagePartTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Messaging.Reflection {
	using System;
	using System.Linq;
	using System.Reflection;
	using DotNetOAuth.Messaging;
	using DotNetOAuth.Messaging.Reflection;
	using DotNetOAuth.Test.Mocks;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class MessagePartTests : MessagingTestBase {
		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void OptionalNonNullableStruct() {
			this.ParameterizedMessageTypeTest(typeof(MessageWithNonNullableOptionalStruct));
		}

		[TestMethod]
		public void RequiredNonNullableStruct() {
			this.ParameterizedMessageTypeTest(typeof(MessageWithNonNullableRequiredStruct));
		}

		[TestMethod]
		public void OptionalNullableStruct() {
			this.ParameterizedMessageTypeTest(typeof(MessageWithNullableOptionalStruct));
		}

		[TestMethod]
		public void RequiredNullableStruct() {
			this.ParameterizedMessageTypeTest(typeof(MessageWithNullableRequiredStruct));
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullMember() {
			new MessagePart(null, new MessagePartAttribute());
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullAttribute() {
			PropertyInfo field = typeof(MessageWithNullableOptionalStruct).GetProperty("OptionalInt", BindingFlags.NonPublic | BindingFlags.Instance);
			new MessagePart(field, null);
		}

		[TestMethod]
		public void SetValue() {
			var message = new MessageWithNonNullableRequiredStruct();
			MessagePart part = this.ParameterizedMessageTypeTest(message.GetType());
			part.SetValue(message, "5");
			Assert.AreEqual(5, message.OptionalInt);
		}

		[TestMethod]
		public void GetValue() {
			var message = new MessageWithNonNullableRequiredStruct();
			message.OptionalInt = 8;
			MessagePart part = this.ParameterizedMessageTypeTest(message.GetType());
			Assert.AreEqual("8", part.GetValue(message));
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void NonFieldOrPropertyMember() {
			MemberInfo method = typeof(MessageWithNullableOptionalStruct).GetMethod("Equals", BindingFlags.Public | BindingFlags.Instance);
			new MessagePart(method, new MessagePartAttribute());
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
	}
}
