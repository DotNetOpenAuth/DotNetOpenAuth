//-----------------------------------------------------------------------
// <copyright file="ChannelTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Messaging {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Net;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.Test.Mocks;
	using NUnit.Framework;

	[TestFixture]
	public class ChannelTests : MessagingTestBase {
		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNull() {
			// This bad channel is deliberately constructed to pass null to
			// its protected base class' constructor.
			new TestBadChannel(true);
		}

		[Test]
		public void ReadFromRequestQueryString() {
			this.ParameterizedReceiveTest("GET");
		}

		[Test]
		public void ReadFromRequestForm() {
			this.ParameterizedReceiveTest("POST");
		}

		/// <summary>
		/// Verifies compliance to OpenID 2.0 section 5.1.1 by verifying the channel
		/// will reject messages that come with an unexpected HTTP verb.
		/// </summary>
		[Test, ExpectedException(typeof(ProtocolException))]
		public void ReadFromRequestDisallowedHttpMethod() {
			var fields = GetStandardTestFields(FieldFill.CompleteBeforeBindings);
			fields["GetOnly"] = "true";
			this.Channel.ReadFromRequest(CreateHttpRequestInfo("POST", fields));
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void SendNull() {
			this.Channel.PrepareResponse(null);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void SendIndirectedUndirectedMessage() {
			IProtocolMessage message = new TestDirectedMessage(MessageTransport.Indirect);
			this.Channel.PrepareResponse(message);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void SendDirectedNoRecipientMessage() {
			IProtocolMessage message = new TestDirectedMessage(MessageTransport.Indirect);
			this.Channel.PrepareResponse(message);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void SendInvalidMessageTransport() {
			IProtocolMessage message = new TestDirectedMessage((MessageTransport)100);
			this.Channel.PrepareResponse(message);
		}

		[Test]
		public void SendIndirectMessage301Get() {
			TestDirectedMessage message = new TestDirectedMessage(MessageTransport.Indirect);
			GetStandardTestMessage(FieldFill.CompleteBeforeBindings, message);
			message.Recipient = new Uri("http://provider/path");
			var expected = GetStandardTestFields(FieldFill.CompleteBeforeBindings);

			OutgoingWebResponse response = this.Channel.PrepareResponse(message);
			Assert.AreEqual(HttpStatusCode.Redirect, response.Status);
			Assert.AreEqual("text/html; charset=utf-8", response.Headers[HttpResponseHeader.ContentType]);
			Assert.IsTrue(response.Body != null && response.Body.Length > 0); // a non-empty body helps get passed filters like WebSense
			StringAssert.StartsWith("http://provider/path", response.Headers[HttpResponseHeader.Location]);
			foreach (var pair in expected) {
				string key = MessagingUtilities.EscapeUriDataStringRfc3986(pair.Key);
				string value = MessagingUtilities.EscapeUriDataStringRfc3986(pair.Value);
				string substring = string.Format("{0}={1}", key, value);
				StringAssert.Contains(substring, response.Headers[HttpResponseHeader.Location]);
			}
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void SendIndirectMessage301GetNullMessage() {
			TestBadChannel badChannel = new TestBadChannel(false);
			badChannel.Create301RedirectResponse(null, new Dictionary<string, string>());
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void SendIndirectMessage301GetEmptyRecipient() {
			TestBadChannel badChannel = new TestBadChannel(false);
			var message = new TestDirectedMessage(MessageTransport.Indirect);
			badChannel.Create301RedirectResponse(message, new Dictionary<string, string>());
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void SendIndirectMessage301GetNullFields() {
			TestBadChannel badChannel = new TestBadChannel(false);
			var message = new TestDirectedMessage(MessageTransport.Indirect);
			message.Recipient = new Uri("http://someserver");
			badChannel.Create301RedirectResponse(message, null);
		}

		[Test]
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
			OutgoingWebResponse response = this.Channel.PrepareResponse(message);
			Assert.AreEqual(HttpStatusCode.OK, response.Status, "A form redirect should be an HTTP successful response.");
			Assert.IsNull(response.Headers[HttpResponseHeader.Location], "There should not be a redirection header in the response.");
			string body = response.Body;
			StringAssert.Contains("<form ", body);
			StringAssert.Contains("action=\"http://provider/path\"", body);
			StringAssert.Contains("method=\"post\"", body);
			StringAssert.Contains("<input type=\"hidden\" name=\"age\" value=\"15\" />", body);
			StringAssert.Contains("<input type=\"hidden\" name=\"Location\" value=\"http://host/path\" />", body);
			StringAssert.Contains("<input type=\"hidden\" name=\"Name\" value=\"" + HttpUtility.HtmlEncode(message.Name) + "\" />", body);
			StringAssert.Contains(".submit()", body, "There should be some javascript to automate form submission.");
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void SendIndirectMessageFormPostNullMessage() {
			TestBadChannel badChannel = new TestBadChannel(false);
			badChannel.CreateFormPostResponse(null, new Dictionary<string, string>());
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void SendIndirectMessageFormPostEmptyRecipient() {
			TestBadChannel badChannel = new TestBadChannel(false);
			var message = new TestDirectedMessage(MessageTransport.Indirect);
			badChannel.CreateFormPostResponse(message, new Dictionary<string, string>());
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
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
		[Test, ExpectedException(typeof(NotImplementedException))]
		public void SendDirectMessageResponse() {
			IProtocolMessage message = new TestDirectedMessage {
				Age = 15,
				Name = "Andrew",
				Location = new Uri("http://host/path"),
			};
			this.Channel.PrepareResponse(message);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void SendIndirectMessageNull() {
			TestBadChannel badChannel = new TestBadChannel(false);
			badChannel.PrepareIndirectResponse(null);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ReceiveNull() {
			TestBadChannel badChannel = new TestBadChannel(false);
			badChannel.Receive(null, null);
		}

		[Test]
		public void ReceiveUnrecognizedMessage() {
			TestBadChannel badChannel = new TestBadChannel(false);
			Assert.IsNull(badChannel.Receive(new Dictionary<string, string>(), null));
		}

		[Test]
		public void ReadFromRequestWithContext() {
			var fields = GetStandardTestFields(FieldFill.AllRequired);
			TestMessage expectedMessage = GetStandardTestMessage(FieldFill.AllRequired);
			HttpRequest request = new HttpRequest("somefile", "http://someurl", MessagingUtilities.CreateQueryString(fields));
			HttpContext.Current = new HttpContext(request, new HttpResponse(new StringWriter()));
			IProtocolMessage message = this.Channel.ReadFromRequest();
			Assert.IsNotNull(message);
			Assert.IsInstanceOf<TestMessage>(message);
			Assert.AreEqual(expectedMessage.Age, ((TestMessage)message).Age);
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void ReadFromRequestNoContext() {
			HttpContext.Current = null;
			TestBadChannel badChannel = new TestBadChannel(false);
			badChannel.ReadFromRequest();
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ReadFromRequestNull() {
			TestBadChannel badChannel = new TestBadChannel(false);
			badChannel.ReadFromRequest(null);
		}

		[Test]
		public void SendReplayProtectedMessageSetsNonce() {
			TestReplayProtectedMessage message = new TestReplayProtectedMessage(MessageTransport.Indirect);
			message.Recipient = new Uri("http://localtest");

			this.Channel = CreateChannel(MessageProtections.ReplayProtection);
			this.Channel.PrepareResponse(message);
			Assert.IsNotNull(((IReplayProtectedProtocolMessage)message).Nonce);
		}

		[Test, ExpectedException(typeof(InvalidSignatureException))]
		public void ReceivedInvalidSignature() {
			this.Channel = CreateChannel(MessageProtections.TamperProtection);
			this.ParameterizedReceiveProtectedTest(DateTime.UtcNow, true);
		}

		[Test]
		public void ReceivedReplayProtectedMessageJustOnce() {
			this.Channel = CreateChannel(MessageProtections.ReplayProtection);
			this.ParameterizedReceiveProtectedTest(DateTime.UtcNow, false);
		}

		[Test, ExpectedException(typeof(ReplayedMessageException))]
		public void ReceivedReplayProtectedMessageTwice() {
			this.Channel = CreateChannel(MessageProtections.ReplayProtection);
			this.ParameterizedReceiveProtectedTest(DateTime.UtcNow, false);
			this.ParameterizedReceiveProtectedTest(DateTime.UtcNow, false);
		}

		[Test, ExpectedException(typeof(ProtocolException))]
		public void MessageExpirationWithoutTamperResistance() {
			new TestChannel(
				new TestMessageFactory(),
				new StandardExpirationBindingElement());
		}

		[Test, ExpectedException(typeof(ProtocolException))]
		public void TooManyBindingElementsProvidingSameProtection() {
			Channel channel = new TestChannel(
				new TestMessageFactory(),
				new MockSigningBindingElement(),
				new MockSigningBindingElement());
			channel.ProcessOutgoingMessageTestHook(new TestSignedDirectedMessage());
		}

		[Test]
		public void BindingElementsOrdering() {
			IChannelBindingElement transformA = new MockTransformationBindingElement("a");
			IChannelBindingElement transformB = new MockTransformationBindingElement("b");
			IChannelBindingElement sign = new MockSigningBindingElement();
			IChannelBindingElement replay = new MockReplayProtectionBindingElement();
			IChannelBindingElement expire = new StandardExpirationBindingElement();

			Channel channel = new TestChannel(
				new TestMessageFactory(),
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

		[Test, ExpectedException(typeof(UnprotectedMessageException))]
		public void InsufficientlyProtectedMessageSent() {
			var message = new TestSignedDirectedMessage(MessageTransport.Direct);
			message.Recipient = new Uri("http://localtest");
			this.Channel.PrepareResponse(message);
		}

		[Test, ExpectedException(typeof(UnprotectedMessageException))]
		public void InsufficientlyProtectedMessageReceived() {
			this.Channel = CreateChannel(MessageProtections.None, MessageProtections.TamperProtection);
			this.ParameterizedReceiveProtectedTest(DateTime.Now, false);
		}

		[Test, ExpectedException(typeof(ProtocolException))]
		public void IncomingMessageMissingRequiredParameters() {
			var fields = GetStandardTestFields(FieldFill.IdentifiableButNotAllRequired);
			this.Channel.ReadFromRequest(CreateHttpRequestInfo("GET", fields));
		}
	}
}
