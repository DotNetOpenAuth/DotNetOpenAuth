using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotNetOAuth.Messaging.Reflection;
using System.Reflection;
using DotNetOAuth.Test.Mocks;
using DotNetOAuth.Messaging;

namespace DotNetOAuth.Test.Messaging.Reflection {
	[TestClass]
	public class MessagePartTests :MessagingTestBase {
		class MessageWithNonNullableOptionalStruct : TestMessage {
			/// <summary>
			/// Optional structs like int must be nullable for Optional to make sense.
			/// </summary>
			[MessagePart(IsRequired = false)]
			internal int optionalInt = 0;
		}
		class MessageWithNonNullableRequiredStruct : TestMessage {
			/// <summary>
			/// This should work because a required field will always have a value so it
			/// need not be nullable.
			/// </summary>
			[MessagePart(IsRequired = true)]
			internal int optionalInt = 0;
		}
		class MessageWithNullableOptionalStruct : TestMessage {
			/// <summary>
			/// Optional structs like int must be nullable for Optional to make sense.
			/// </summary>
			[MessagePart(IsRequired = false)]
			internal int? optionalInt = 0;
		}
		class MessageWithNullableRequiredStruct : TestMessage {
			[MessagePart(IsRequired = true)]
			internal int? optionalInt = null;
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void OptionalNonNullableStruct() {
			ParameterizedMessageTypeTest(typeof(MessageWithNonNullableOptionalStruct));
		}

		[TestMethod]
		public void RequiredNonNullableStruct() {
			ParameterizedMessageTypeTest(typeof(MessageWithNonNullableRequiredStruct));
		}

		[TestMethod]
		public void OptionalNullableStruct() {
			ParameterizedMessageTypeTest(typeof(MessageWithNullableOptionalStruct));
		}

		[TestMethod]
		public void RequiredNullableStruct() {
			ParameterizedMessageTypeTest(typeof(MessageWithNullableRequiredStruct));
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullMember() {
			new MessagePart(null, new MessagePartAttribute());
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullAttribute() {
			FieldInfo field = typeof(MessageWithNullableOptionalStruct).GetField("optionalInt", BindingFlags.NonPublic | BindingFlags.Instance);
			new MessagePart(field, null);
		}

		[TestMethod]
		public void SetValue() {
			var message = new MessageWithNonNullableRequiredStruct();
			MessagePart part = ParameterizedMessageTypeTest(message.GetType());
			part.SetValue(message, "5");
			Assert.AreEqual(5, message.optionalInt);
		}

		[TestMethod]
		public void GetValue() {
			var message = new MessageWithNonNullableRequiredStruct();
			message.optionalInt = 8;
			MessagePart part = ParameterizedMessageTypeTest(message.GetType());
			Assert.AreEqual("8", part.GetValue(message));
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void NonFieldOrPropertyMember() {
			MemberInfo method = typeof(MessageWithNullableOptionalStruct).GetMethod("Equals", BindingFlags.Public | BindingFlags.Instance);
			new MessagePart(method, new MessagePartAttribute());
		}

		private MessagePart ParameterizedMessageTypeTest(Type messageType) {
			FieldInfo field = messageType.GetField("optionalInt", BindingFlags.NonPublic | BindingFlags.Instance);
			MessagePartAttribute attribute = field.GetCustomAttributes(typeof(MessagePartAttribute), true).OfType<MessagePartAttribute>().Single();
			return new MessagePart(field, attribute);
		}
	}
}
