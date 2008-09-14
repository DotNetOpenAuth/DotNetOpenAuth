//-----------------------------------------------------------------------
// <copyright file="ChannelTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Messaging {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Net;
	using System.Web;
	using DotNetOAuth.Messaging;
	using DotNetOAuth.Messaging.Bindings;
	using DotNetOAuth.Test.Mocks;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class ChannelTests : MessagingTestBase {
		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNull() {
			// This bad channel is deliberately constructed to pass null to
			// its protected base class' constructor.
			new TestBadChannel(true);
		}

		[TestMethod]
		public void DequeueIndirectOrResponseMessageReturnsNull() {
			Assert.IsNull(this.Channel.DequeueIndirectOrResponseMessage());
		}

		[TestMethod]
		public void ReadFromRequestQueryString() {
			this.ParameterizedReceiveTest("GET");
		}

		[TestMethod]
		public void ReadFromRequestForm() {
			this.ParameterizedReceiveTest("POST");
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void SendNull() {
			this.Channel.Send(null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void SendIndirectedUndirectedMessage() {
			IProtocolMessage message = new TestMessage(MessageTransport.Indirect);
			this.Channel.Send(message);
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void SendDirectedNoRecipientMessage() {
			IProtocolMessage message = new TestDirectedMessage(MessageTransport.Indirect);
			this.Channel.Send(message);
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void SendInvalidMessageTransport() {
			IProtocolMessage message = new TestDirectedMessage((MessageTransport)100);
			this.Channel.Send(message);
		}

		[TestMethod]
		public void SendIndirectMessage301Get() {
			IProtocolMessage message = new TestDirectedMessage(MessageTransport.Indirect) {
				Age = 15,
				Name = "Andrew",
				Location = new Uri("http://host/path"),
				Recipient = new Uri("http://provider/path"),
			};
			this.Channel.Send(message);
			Response response = this.Channel.DequeueIndirectOrResponseMessage();
			Assert.AreEqual(HttpStatusCode.Redirect, response.Status);
			StringAssert.StartsWith(response.Headers[HttpResponseHeader.Location], "http://provider/path");
			StringAssert.Contains(response.Headers[HttpResponseHeader.Location], "age=15");
			StringAssert.Contains(response.Headers[HttpResponseHeader.Location], "Name=Andrew");
			StringAssert.Contains(response.Headers[HttpResponseHeader.Location], "Location=http%3a%2f%2fhost%2fpath");
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void SendIndirectMessage301GetNullMessage() {
			TestBadChannel badChannel = new TestBadChannel(false);
			badChannel.Create301RedirectResponse(null, new Dictionary<string, string>());
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void SendIndirectMessage301GetEmptyRecipient() {
			TestBadChannel badChannel = new TestBadChannel(false);
			var message = new TestDirectedMessage(MessageTransport.Indirect);
			badChannel.Create301RedirectResponse(message, new Dictionary<string, string>());
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void SendIndirectMessage301GetNullFields() {
			TestBadChannel badChannel = new TestBadChannel(false);
			var message = new TestDirectedMessage(MessageTransport.Indirect);
			message.Recipient = new Uri("http://someserver");
			badChannel.Create301RedirectResponse(message, null);
		}

		[TestMethod]
		public void SendIndirectMessageFormPost() {
			// We craft a very large message to force fallback to form POST.
			// We'll also stick some HTML reserved characters in the string value
			// to test proper character escaping.
			var message = new TestDirectedMessage(MessageTransport.Indirect) {
				Age = 15,
				Name = "c<b" + new string('a', 10 * 1024),
				Location = new Uri("http://host/path"),
				Recipient = new Uri("http://provider/path"),
			};
			this.Channel.Send(message);
			Response response = this.Channel.DequeueIndirectOrResponseMessage();
			Assert.AreEqual(HttpStatusCode.OK, response.Status, "A form redirect should be an HTTP successful response.");
			Assert.IsNull(response.Headers[HttpResponseHeader.Location], "There should not be a redirection header in the response.");
			string body = response.Body;
			StringAssert.Contains(body, "<form ");
			StringAssert.Contains(body, "action=\"http://provider/path\"");
			StringAssert.Contains(body, "method=\"post\"");
			StringAssert.Contains(body, "<input type=\"hidden\" name=\"age\" value=\"15\" />");
			StringAssert.Contains(body, "<input type=\"hidden\" name=\"Location\" value=\"http://host/path\" />");
			StringAssert.Contains(body, "<input type=\"hidden\" name=\"Name\" value=\"" + HttpUtility.HtmlEncode(message.Name) + "\" />");
			StringAssert.Contains(body, ".submit()", "There should be some javascript to automate form submission.");
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void SendIndirectMessageFormPostNullMessage() {
			TestBadChannel badChannel = new TestBadChannel(false);
			badChannel.CreateFormPostResponse(null, new Dictionary<string, string>());
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void SendIndirectMessageFormPostEmptyRecipient() {
			TestBadChannel badChannel = new TestBadChannel(false);
			var message = new TestDirectedMessage(MessageTransport.Indirect);
			badChannel.CreateFormPostResponse(message, new Dictionary<string, string>());
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void SendIndirectMessageFormPostNullFields() {
			TestBadChannel badChannel = new TestBadChannel(false);
			var message = new TestDirectedMessage(MessageTransport.Indirect);
			message.Recipient = new Uri("http://someserver");
			badChannel.CreateFormPostResponse(message, null);
		}

		/// <summary>
		/// Tests that a direct message is sent when the appropriate message type is provided.
		/// </summary>
		/// <remarks>
		/// Since this is a mock channel that doesn't actually formulate a direct message response,
		/// we just check that the right method was called.
		/// </remarks>
		[TestMethod, ExpectedException(typeof(NotImplementedException), "SendDirectMessageResponse")]
		public void SendDirectMessageResponse() {
			IProtocolMessage message = new TestMessage {
				Age = 15,
				Name = "Andrew",
				Location = new Uri("http://host/path"),
			};
			this.Channel.Send(message);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void QueueIndirectOrResponseMessageNull() {
			TestBadChannel badChannel = new TestBadChannel(false);
			badChannel.QueueIndirectOrResponseMessage(null);
		}

		[TestMethod, ExpectedException(typeof(InvalidOperationException))]
		public void QueueIndirectOrResponseMessageTwice() {
			TestBadChannel badChannel = new TestBadChannel(false);
			Response response = new Response();
			badChannel.QueueIndirectOrResponseMessage(new Response());
			badChannel.QueueIndirectOrResponseMessage(new Response());
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void SendIndirectMessageNull() {
			TestBadChannel badChannel = new TestBadChannel(false);
			badChannel.SendIndirectMessage(null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void ReceiveNull() {
			TestBadChannel badChannel = new TestBadChannel(false);
			badChannel.Receive(null);
		}

		[TestMethod]
		public void ReceiveUnrecognizedMessage() {
			TestBadChannel badChannel = new TestBadChannel(false);
			Assert.IsNull(badChannel.Receive(new Dictionary<string, string>()));
		}

		[TestMethod]
		public void ReadFromRequestWithContext() {
			// TODO: make this a request with a message in it.
			HttpRequest request = new HttpRequest("somefile", "http://someurl", "age=15");
			HttpContext.Current = new HttpContext(request, new HttpResponse(new StringWriter()));
			IProtocolMessage message = this.Channel.ReadFromRequest();
			Assert.IsNotNull(message);
			Assert.IsInstanceOfType(message, typeof(TestMessage));
			Assert.AreEqual(15, ((TestMessage)message).Age);
		}

		[TestMethod, ExpectedException(typeof(InvalidOperationException))]
		public void ReadFromRequestNoContext() {
			TestBadChannel badChannel = new TestBadChannel(false);
			badChannel.ReadFromRequest();
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void ReadFromRequestNull() {
			TestBadChannel badChannel = new TestBadChannel(false);
			badChannel.ReadFromRequest(null);
		}

		[TestMethod]
		public void SendReplayProtectedMessageSetsNonce() {
			TestReplayProtectedMessage message = new TestReplayProtectedMessage(MessageTransport.Indirect);
			message.Recipient = new Uri("http://localtest");

			this.Channel = CreateChannel(MessageProtection.ReplayProtection);
			this.Channel.Send(message);
			Assert.IsNotNull(((IReplayProtectedProtocolMessage)message).Nonce);
		}

		[TestMethod, ExpectedException(typeof(InvalidSignatureException))]
		public void ReceivedInvalidSignature() {
			this.Channel = CreateChannel(MessageProtection.TamperProtection);
			this.ParameterizedReceiveProtectedTest(DateTime.UtcNow, true);
		}

		[TestMethod]
		public void ReceivedReplayProtectedMessageJustOnce() {
			this.Channel = CreateChannel(MessageProtection.ReplayProtection);
			this.ParameterizedReceiveProtectedTest(DateTime.UtcNow, false);
		}

		[TestMethod, ExpectedException(typeof(ReplayedMessageException))]
		public void ReceivedReplayProtectedMessageTwice() {
			this.Channel = CreateChannel(MessageProtection.ReplayProtection);
			this.ParameterizedReceiveProtectedTest(DateTime.UtcNow, false);
			this.ParameterizedReceiveProtectedTest(DateTime.UtcNow, false);
		}

		[TestMethod, ExpectedException(typeof(ProtocolException))]
		public void MessageExpirationWithoutTamperResistance() {
			new TestChannel(
				new TestMessageTypeProvider(),
				new StandardExpirationBindingElement());
		}

		[TestMethod, ExpectedException(typeof(ProtocolException))]
		public void TooManyBindingElementsProvidingSameProtection() {
			new TestChannel(
				new TestMessageTypeProvider(),
				new MockSigningBindingElement(),
				new MockSigningBindingElement());
		}

		[TestMethod]
		public void BindingElementsOrdering() {
			IChannelBindingElement transformA = new MockTransformationBindingElement("a");
			IChannelBindingElement transformB = new MockTransformationBindingElement("b");
			IChannelBindingElement sign = new MockSigningBindingElement();
			IChannelBindingElement replay = new MockReplayProtectionBindingElement();
			IChannelBindingElement expire = new StandardExpirationBindingElement();

			Channel channel = new TestChannel(
				new TestMessageTypeProvider(),
				sign,
				replay,
				expire,
				transformB,
				transformA);

			Assert.AreEqual(5, channel.BindingElements.Count);
			Assert.AreSame(transformB, channel.BindingElements[0]);
			Assert.AreSame(transformA, channel.BindingElements[1]);
			Assert.AreSame(replay, channel.BindingElements[2]);
			Assert.AreSame(expire, channel.BindingElements[3]);
			Assert.AreSame(sign, channel.BindingElements[4]);
		}

		[TestMethod, ExpectedException(typeof(UnprotectedMessageException))]
		public void InsufficientlyProtectedMessageSent() {
			var message = new TestSignedDirectedMessage(MessageTransport.Direct);
			message.Recipient = new Uri("http://localtest");
			this.Channel.Send(message);
		}

		[TestMethod, ExpectedException(typeof(UnprotectedMessageException))]
		public void InsufficientlyProtectedMessageReceived() {
			this.Channel = CreateChannel(MessageProtection.None, MessageProtection.TamperProtection);
			this.ParameterizedReceiveProtectedTest(DateTime.Now, false);
		}
	}
}
