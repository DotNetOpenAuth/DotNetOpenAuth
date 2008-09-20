namespace DotNetOAuth.Test.Messaging.Reflection {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using DotNetOAuth.Messaging.Reflection;

	[TestClass]
	public class MessageDescriptionTests : MessagingTestBase {
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void GetNull() {
			MessageDescription.Get(null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void GetNonMessageType() {
			MessageDescription.Get(typeof(string));
		}
	}
}
