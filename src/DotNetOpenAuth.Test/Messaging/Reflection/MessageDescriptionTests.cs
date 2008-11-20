//-----------------------------------------------------------------------
// <copyright file="MessageDescriptionTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Messaging.Reflection {
	using System;
	using DotNetOpenAuth.Messaging.Reflection;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class MessageDescriptionTests : MessagingTestBase {
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void GetNullType() {
			MessageDescription.Get(null, new Version(1, 0));
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void GetNullVersion() {
			MessageDescription.Get(typeof(Mocks.TestMessage), null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void GetNonMessageType() {
			MessageDescription.Get(typeof(string), new Version(1, 0));
		}
	}
}
