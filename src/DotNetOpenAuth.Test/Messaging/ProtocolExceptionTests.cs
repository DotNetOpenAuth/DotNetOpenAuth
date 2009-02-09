//-----------------------------------------------------------------------
// <copyright file="ProtocolExceptionTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Messaging {
	using System;
	using DotNetOpenAuth.Messaging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class ProtocolExceptionTests : TestBase {
		[TestMethod]
		public void CtorDefault() {
			ProtocolException ex = new ProtocolException();
		}

		[TestMethod]
		public void CtorWithTextMessage() {
			ProtocolException ex = new ProtocolException("message");
			Assert.AreEqual("message", ex.Message);
		}

		[TestMethod]
		public void CtorWithTextMessageAndInnerException() {
			Exception innerException = new Exception();
			ProtocolException ex = new ProtocolException("message", innerException);
			Assert.AreEqual("message", ex.Message);
			Assert.AreSame(innerException, ex.InnerException);
		}

		[TestMethod]
		public void CtorWithProtocolMessage() {
			IProtocolMessage message = new Mocks.TestDirectedMessage();
			ProtocolException ex = new ProtocolException("message", message);
			Assert.AreSame(message, ex.FaultedMessage);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void CtorWithNullProtocolMessage() {
			new ProtocolException("message", (IProtocolMessage)null);
		}
	}
}
