//-----------------------------------------------------------------------
// <copyright file="ChannelTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Messaging {
	using System;
	using System.IO;
	using System.Net;
	using DotNetOAuth.Messaging;
	using DotNetOAuth.Test.Mocks;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class ChannelTests : TestBase {
		private Channel channel;

		[TestInitialize]
		public override void SetUp() {
			base.SetUp();

			this.channel = new TestChannel();
		}

		[TestMethod]
		public void DequeueIndirectOrResponseMessageReturnsNull() {
			Assert.IsNull(this.channel.DequeueIndirectOrResponseMessage());
		}

		[TestMethod]
		public void ReceiveFromQueryString() {
			Uri requestUri = new Uri("http://localhost/path?age=15&Name=Andrew&Location=http%3A%2F%2Fhostb%2FpathB");
			WebHeaderCollection headers = new WebHeaderCollection();
			HttpRequestInfo request = new HttpRequestInfo {
				HttpMethod = "GET",
				Url = requestUri,
				Headers = headers,
				InputStream = new MemoryStream(),
			};
			IProtocolMessage requestMessage = this.channel.Receive(request);
			Assert.IsNotNull(requestMessage);
			Assert.IsInstanceOfType(requestMessage, typeof(TestMessage));
			TestMessage testMessage = (TestMessage)requestMessage;
			Assert.AreEqual(15, testMessage.Age);
			Assert.AreEqual("Andrew", testMessage.Name);
			Assert.AreEqual("http://hostb/pathB", testMessage.Location.AbsoluteUri);
		}

		[TestMethod]
		public void ReceiveFromForm() {
			Uri requestUri = new Uri("http://localhost/path");
			WebHeaderCollection headers = new WebHeaderCollection();
			headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
			MemoryStream ms = new MemoryStream();
			StreamWriter sw = new StreamWriter(ms);
			sw.Write("age=15&Name=Andrew&Location=http%3A%2F%2Fhostb%2FpathB");
			sw.Flush();
			ms.Position = 0;
			HttpRequestInfo request = new HttpRequestInfo {
				HttpMethod = "POST",
				Url = requestUri,
				Headers = headers,
				InputStream = ms,
			};

			IProtocolMessage requestMessage = this.channel.Receive(request);
			Assert.IsNotNull(requestMessage);
			Assert.IsInstanceOfType(requestMessage, typeof(TestMessage));
			TestMessage testMessage = (TestMessage)requestMessage;
			Assert.AreEqual(15, testMessage.Age);
			Assert.AreEqual("Andrew", testMessage.Name);
			Assert.AreEqual("http://hostb/pathB", testMessage.Location.AbsoluteUri);
		}

		private static HttpRequestInfo CreateHttpRequest() {
			throw new NotImplementedException();
		}
	}
}
