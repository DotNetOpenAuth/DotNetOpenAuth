//-----------------------------------------------------------------------
// <copyright file="MessageDescriptionTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Messaging.Reflection {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;
	using NUnit.Framework;

	[TestFixture]
	public class MessageDescriptionTests : MessagingTestBase {
		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullType() {
			new MessageDescription(null, new Version(1, 0));
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullVersion() {
			new MessageDescription(typeof(Mocks.TestMessage), null);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void CtorNonMessageType() {
			new MessageDescription(typeof(string), new Version(1, 0));
		}

		[Test]
		public void MultiVersionedMessageTest() {
			var v10 = new MessageDescription(typeof(MultiVersionMessage), new Version(1, 0));
			var v20 = new MessageDescription(typeof(MultiVersionMessage), new Version(2, 0));
			var v25 = new MessageDescription(typeof(MultiVersionMessage), new Version(2, 5));
			var v30 = new MessageDescription(typeof(MultiVersionMessage), new Version(3, 0));

			// Verify that the AllVersion member appears in every version.
			Assert.IsTrue(v10.Mapping.ContainsKey("AllVersion"));
			Assert.IsTrue(v20.Mapping.ContainsKey("AllVersion"));
			Assert.IsTrue(v25.Mapping.ContainsKey("AllVersion"));
			Assert.IsTrue(v30.Mapping.ContainsKey("AllVersion"));

			// Verify that UpThru25 disappears in 3.0.
			Assert.IsTrue(v10.Mapping.ContainsKey("UpThru25"));
			Assert.IsTrue(v20.Mapping.ContainsKey("UpThru25"));
			Assert.IsTrue(v25.Mapping.ContainsKey("UpThru25"));
			Assert.IsFalse(v30.Mapping.ContainsKey("UpThru25"));

			// Verify that NewIn20 doesn't appear before that version.
			Assert.IsFalse(v10.Mapping.ContainsKey("NewIn20"));
			Assert.IsTrue(v20.Mapping.ContainsKey("NewIn20"));
			Assert.IsTrue(v25.Mapping.ContainsKey("NewIn20"));
			Assert.IsTrue(v30.Mapping.ContainsKey("NewIn20"));

			// Verify that an optional field in 1.0 becomes required in 2.0
			Assert.IsTrue(v10.Mapping.ContainsKey("RequiredIn20"));
			Assert.IsFalse(v10.Mapping["RequiredIn20"].IsRequired);
			Assert.IsTrue(v20.Mapping.ContainsKey("RequiredIn20"));
			Assert.IsTrue(v20.Mapping["RequiredIn20"].IsRequired);
			Assert.IsTrue(v25.Mapping.ContainsKey("RequiredIn20"));
			Assert.IsTrue(v25.Mapping["RequiredIn20"].IsRequired);
			Assert.IsTrue(v30.Mapping.ContainsKey("RequiredIn20"));
			Assert.IsTrue(v30.Mapping["RequiredIn20"].IsRequired);

			// Verify that one (odd) field appeared in 1.0 as optional,
			// disappeared in 2.0, and then reappeared in 2.5 and later as required.
			Assert.IsTrue(v10.Mapping.ContainsKey("OptionalIn10RequiredIn25AndLater"));
			Assert.IsFalse(v10.Mapping["OptionalIn10RequiredIn25AndLater"].IsRequired);
			Assert.IsFalse(v20.Mapping.ContainsKey("OptionalIn10RequiredIn25AndLater"));
			Assert.IsTrue(v25.Mapping.ContainsKey("OptionalIn10RequiredIn25AndLater"));
			Assert.IsTrue(v25.Mapping["OptionalIn10RequiredIn25AndLater"].IsRequired);
			Assert.IsTrue(v30.Mapping.ContainsKey("OptionalIn10RequiredIn25AndLater"));
			Assert.IsTrue(v30.Mapping["OptionalIn10RequiredIn25AndLater"].IsRequired);
		}

		/// <summary>
		/// Verifies that the constructors cache is properly initialized.
		/// </summary>
		[Test]
		public void CtorsCache() {
			var message = new MessageDescription(typeof(MultiVersionMessage), new Version(1, 0));
			Assert.IsNotNull(message.Constructors);
			Assert.AreEqual(1, message.Constructors.Length);
		}

		private class MultiVersionMessage : Mocks.TestBaseMessage {
#pragma warning disable 0649 // these fields are never written to, but part of the test
			[MessagePart]
			public string AllVersion;

			[MessagePart(MaxVersion = "2.5")]
			public string UpThru25;

			[MessagePart(MinVersion = "2.0")]
			public string NewIn20;

			[MessagePart(IsRequired = false)]
			[MessagePart(IsRequired = true, MinVersion = "2.0")]
			public string RequiredIn20;

			[MessagePart(MinVersion = "1.0", MaxVersion = "1.0", IsRequired = false)]
			[MessagePart(MinVersion = "2.5", IsRequired = true)]
			public string OptionalIn10RequiredIn25AndLater;
#pragma warning restore 0649
		}
	}
}
