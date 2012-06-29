//-----------------------------------------------------------------------
// <copyright file="ProtocolExceptionTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Messaging {
	using System;
	using DotNetOpenAuth.Messaging;
	using NUnit.Framework;

	[TestFixture]
	public class ProtocolExceptionTests : TestBase {
		[Test]
		public void CtorDefault() {
			ProtocolException ex = new ProtocolException();
		}

		[Test]
		public void CtorWithTextMessage() {
			ProtocolException ex = new ProtocolException("message");
			Assert.AreEqual("message", ex.Message);
		}

		[Test]
		public void CtorWithTextMessageAndInnerException() {
			Exception innerException = new Exception();
			ProtocolException ex = new ProtocolException("message", innerException);
			Assert.AreEqual("message", ex.Message);
			Assert.AreSame(innerException, ex.InnerException);
		}

		[Test]
		public void CtorWithProtocolMessage() {
			IProtocolMessage message = new Mocks.TestDirectedMessage();
			ProtocolException ex = new ProtocolException("message", message);
			Assert.AreSame(message, ex.FaultedMessage);
		}

		[Test]
		public void CtorWithNullProtocolMessage() {
			new ProtocolException("message", (IProtocolMessage)null);
		}
	}
}
