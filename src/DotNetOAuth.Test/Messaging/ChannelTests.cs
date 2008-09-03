//-----------------------------------------------------------------------
// <copyright file="ChannelTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using DotNetOAuth.Messaging;
	using DotNetOAuth.Test.Mocks;
	using System.Web;

	[TestClass]
	public class ChannelTests : TestBase {
		Channel channel;

		[TestInitialize]
		public override void SetUp() {
			base.SetUp();

			channel = new TestChannel();
		}

		[TestMethod]
		public void DequeueIndirectOrResponseMessageReturnsNull() {
			Assert.IsNull(this.channel.DequeueIndirectOrResponseMessage());
		}

		[TestMethod]
		public void ReceiveFromQueryString() {
			string queryString = "age=15&Name=Andrew&Location=http%3A%2F%2Fhostb%2FpathB";
			var httpRequest = new HttpRequest("filename", "http://localhost/path?" + queryString, queryString);
			IProtocolMessage requestMessage = this.channel.Receive(httpRequest);
			Assert.IsNotNull(requestMessage);
			Assert.IsInstanceOfType(requestMessage, typeof(TestMessage));
			TestMessage testMessage = (TestMessage)requestMessage;
			Assert.AreEqual(15, testMessage.Age);
			Assert.AreEqual("Andrew", testMessage.Name);
			Assert.AreEqual("http://hostb/pathB", testMessage.Location.AbsoluteUri);
		}
	}
}
