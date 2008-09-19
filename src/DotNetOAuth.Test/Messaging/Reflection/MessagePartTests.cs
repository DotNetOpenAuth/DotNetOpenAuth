using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotNetOAuth.Messaging.Reflection;
using System.Reflection;

namespace DotNetOAuth.Test.Messaging.Reflection {
	[TestClass]
	public class MessagePartTests :MessagingTestBase  {
		class MessageWithNonNullableOptionalStruct {
			/// <summary>
			/// Optional structs like int must be nullable for Optional to make sense.
			/// </summary>
			[MessagePart(IsRequired = false)]
			internal int optionalInt = 0;
		}
		class MessageWithNonNullableRequiredStruct {
			/// <summary>
			/// This should work because a required field will always have a value so it
			/// need not be nullable.
			/// </summary>
			[MessagePart(IsRequired = true)]
			internal int optionalInt = 0;
		}
		class MessageWithNullableOptionalStruct {
			/// <summary>
			/// Optional structs like int must be nullable for Optional to make sense.
			/// </summary>
			[MessagePart(IsRequired = false)]
			internal int? optionalInt = 0;
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

		private void ParameterizedMessageTypeTest(Type messageType) {
			FieldInfo field = messageType.GetField("optionalInt", BindingFlags.NonPublic | BindingFlags.Instance);
			MessagePartAttribute attribute = field.GetCustomAttributes(typeof(MessagePartAttribute), true).OfType<MessagePartAttribute>().Single();
			new MessagePart(field, attribute);
		}
	}
}
