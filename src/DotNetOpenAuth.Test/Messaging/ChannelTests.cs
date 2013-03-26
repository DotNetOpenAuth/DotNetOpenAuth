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
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.Test.Mocks;
	using NUnit.Framework;

	[TestFixture]
	public class ChannelTests : MessagingTestBase {
		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullFirstParameter() {
			new TestBadChannel(null, new IChannelBindingElement[0], new DefaultOpenIdHostFactories());
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullSecondParameter() {
			new TestBadChannel(new TestMessageFactory(), null, new DefaultOpenIdHostFactories());
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullThirdParameter() {
			new TestBadChannel(new TestMessageFactory(), new IChannelBindingElement[0], null);
		}

		[Test]
		public async Task ReadFromRequestQueryString() {
			await this.ParameterizedReceiveTestAsync(HttpMethod.Get);
		}

		[Test]
		public async Task ReadFromRequestForm() {
			await this.ParameterizedReceiveTestAsync(HttpMethod.Post);
		}

		/// <summary>
		/// Verifies compliance to OpenID 2.0 section 5.1.1 by verifying the channel
		/// will reject messages that come with an unexpected HTTP verb.
		/// </summary>
		[Test, ExpectedException(typeof(ProtocolException))]
		public async Task ReadFromRequestDisallowedHttpMethod() {
			var fields = GetStandardTestFields(FieldFill.CompleteBeforeBindings);
			fields["GetOnly"] = "true";
			await this.Channel.ReadFromRequestAsync(CreateHttpRequestInfo(HttpMethod.Post, fields), CancellationToken.None);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public async Task SendNull() {
			await this.Channel.PrepareResponseAsync(null);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public async Task SendIndirectedUndirectedMessage() {
			IProtocolMessage message = new TestDirectedMessage(MessageTransport.Indirect);
			await this.Channel.PrepareResponseAsync(message);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public async Task SendDirectedNoRecipientMessage() {
			IProtocolMessage message = new TestDirectedMessage(MessageTransport.Indirect);
			await this.Channel.PrepareResponseAsync(message);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public async Task SendInvalidMessageTransport() {
			IProtocolMessage message = new TestDirectedMessage((MessageTransport)100);
			await this.Channel.PrepareResponseAsync(message);
		}

		[Test]
		public async Task SendIndirectMessage301Get() {
			TestDirectedMessage message = new TestDirectedMessage(MessageTransport.Indirect);
			GetStandardTestMessage(FieldFill.CompleteBeforeBindings, message);
			message.Recipient = new Uri("http://provider/path");
			var expected = GetStandardTestFields(FieldFill.CompleteBeforeBindings);

			var response = await this.Channel.PrepareResponseAsync(message);
			Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
			Assert.AreEqual("text/html; charset=utf-8", response.Content.Headers.ContentType.ToString());
			Assert.IsTrue(response.Content != null && response.Content.Headers.ContentLength > 0); // a non-empty body helps get passed filters like WebSense
			StringAssert.StartsWith("http://provider/path", response.Headers.Location.AbsoluteUri);
			foreach (var pair in expected) {
				string key = MessagingUtilities.EscapeUriDataStringRfc3986(pair.Key);
				string value = MessagingUtilities.EscapeUriDataStringRfc3986(pair.Value);
				string substring = string.Format("{0}={1}", key, value);
				StringAssert.Contains(substring, response.Headers.Location.AbsoluteUri);
			}
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void SendIndirectMessage301GetNullMessage() {
			TestBadChannel badChannel = new TestBadChannel();
			badChannel.Create301RedirectResponse(null, new Dictionary<string, string>());
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void SendIndirectMessage301GetEmptyRecipient() {
			TestBadChannel badChannel = new TestBadChannel();
			var message = new TestDirectedMessage(MessageTransport.Indirect);
			badChannel.Create301RedirectResponse(message, new Dictionary<string, string>());
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void SendIndirectMessage301GetNullFields() {
			TestBadChannel badChannel = new TestBadChannel();
			var message = new TestDirectedMessage(MessageTransport.Indirect);
			message.Recipient = new Uri("http://someserver");
			badChannel.Create301RedirectResponse(message, null);
		}

		[Test]
		public async Task SendIndirectMessageFormPost() {
			// We craft a very large message to force fallback to form POST.
			// We'll also stick some HTML reserved characters in the string value
			// to test proper character escaping.
			var message = new TestDirectedMessage(MessageTransport.Indirect) {
				Age = 15,
				Name = "c<b" + new string('a', 10 * 1024),
				Location = new Uri("http://host/path"),
				Recipient = new Uri("http://provider/path"),
			};
			var response = await this.Channel.PrepareResponseAsync(message);
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "A form redirect should be an HTTP successful response.");
			Assert.IsNull(response.Headers.Location, "There should not be a redirection header in the response.");
			string body = await response.Content.ReadAsStringAsync();
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
			TestBadChannel badChannel = new TestBadChannel();
			badChannel.CreateFormPostResponse(null, new Dictionary<string, string>());
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void SendIndirectMessageFormPostEmptyRecipient() {
			TestBadChannel badChannel = new TestBadChannel();
			var message = new TestDirectedMessage(MessageTransport.Indirect);
			badChannel.CreateFormPostResponse(message, new Dictionary<string, string>());
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void SendIndirectMessageFormPostNullFields() {
			TestBadChannel badChannel = new TestBadChannel();
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
		public async Task SendDirectMessageResponse() {
			IProtocolMessage message = new TestDirectedMessage {
				Age = 15,
				Name = "Andrew",
				Location = new Uri("http://host/path"),
			};
			await this.Channel.PrepareResponseAsync(message);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void SendIndirectMessageNull() {
			TestBadChannel badChannel = new TestBadChannel();
			badChannel.PrepareIndirectResponse(null);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ReceiveNull() {
			TestBadChannel badChannel = new TestBadChannel();
			badChannel.Receive(null, null);
		}

		[Test]
		public void ReceiveUnrecognizedMessage() {
			TestBadChannel badChannel = new TestBadChannel();
			Assert.IsNull(badChannel.Receive(new Dictionary<string, string>(), null));
		}

		[Test]
		public async Task ReadFromRequestWithContext() {
			var fields = GetStandardTestFields(FieldFill.AllRequired);
			TestMessage expectedMessage = GetStandardTestMessage(FieldFill.AllRequired);
			HttpRequest request = new HttpRequest("somefile", "http://someurl", MessagingUtilities.CreateQueryString(fields));
			HttpContext.Current = new HttpContext(request, new HttpResponse(new StringWriter()));
			var requestBase = this.Channel.GetRequestFromContext();
			IProtocolMessage message = await this.Channel.ReadFromRequestAsync(requestBase.AsHttpRequestMessage(), CancellationToken.None);
			Assert.IsNotNull(message);
			Assert.IsInstanceOf<TestMessage>(message);
			Assert.AreEqual(expectedMessage.Age, ((TestMessage)message).Age);
		}

		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void GetRequestFromContextNoContext() {
			HttpContext.Current = null;
			TestBadChannel badChannel = new TestBadChannel();
			badChannel.GetRequestFromContext();
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public async Task ReadFromRequestNull() {
			TestBadChannel badChannel = new TestBadChannel();
			await badChannel.ReadFromRequestAsync(null, CancellationToken.None);
		}

		[Test]
		public async Task SendReplayProtectedMessageSetsNonce() {
			TestReplayProtectedMessage message = new TestReplayProtectedMessage(MessageTransport.Indirect);
			message.Recipient = new Uri("http://localtest");

			this.Channel = CreateChannel(MessageProtections.ReplayProtection);
			await this.Channel.PrepareResponseAsync(message);
			Assert.IsNotNull(((IReplayProtectedProtocolMessage)message).Nonce);
		}

		[Test, ExpectedException(typeof(InvalidSignatureException))]
		public async Task ReceivedInvalidSignature() {
			this.Channel = CreateChannel(MessageProtections.TamperProtection);
			await this.ParameterizedReceiveProtectedTestAsync(DateTime.UtcNow, true);
		}

		[Test]
		public async Task ReceivedReplayProtectedMessageJustOnce() {
			this.Channel = CreateChannel(MessageProtections.ReplayProtection);
			await this.ParameterizedReceiveProtectedTestAsync(DateTime.UtcNow, false);
		}

		[Test, ExpectedException(typeof(ReplayedMessageException))]
		public async Task ReceivedReplayProtectedMessageTwice() {
			this.Channel = CreateChannel(MessageProtections.ReplayProtection);
			await this.ParameterizedReceiveProtectedTestAsync(DateTime.UtcNow, false);
			await this.ParameterizedReceiveProtectedTestAsync(DateTime.UtcNow, false);
		}

		[Test, ExpectedException(typeof(ProtocolException))]
		public void MessageExpirationWithoutTamperResistance() {
			new TestChannel(
				new TestMessageFactory(),
				new IChannelBindingElement[] { new StandardExpirationBindingElement() },
				new DefaultOpenIdHostFactories());
		}

		[Test, ExpectedException(typeof(ProtocolException))]
		public async Task TooManyBindingElementsProvidingSameProtection() {
			Channel channel = new TestChannel(
				new TestMessageFactory(),
				new IChannelBindingElement[] { new MockSigningBindingElement(), new MockSigningBindingElement() },
				new DefaultOpenIdHostFactories());
			await channel.ProcessOutgoingMessageTestHookAsync(new TestSignedDirectedMessage());
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
				new[] { sign, replay, expire, transformB, transformA },
				new DefaultOpenIdHostFactories());

			Assert.AreEqual(5, channel.BindingElements.Count);
			Assert.AreSame(transformB, channel.BindingElements[0]);
			Assert.AreSame(transformA, channel.BindingElements[1]);
			Assert.AreSame(replay, channel.BindingElements[2]);
			Assert.AreSame(expire, channel.BindingElements[3]);
			Assert.AreSame(sign, channel.BindingElements[4]);
		}

		[Test, ExpectedException(typeof(UnprotectedMessageException))]
		public async Task InsufficientlyProtectedMessageSent() {
			var message = new TestSignedDirectedMessage(MessageTransport.Direct);
			message.Recipient = new Uri("http://localtest");
			await this.Channel.PrepareResponseAsync(message);
		}

		[Test, ExpectedException(typeof(UnprotectedMessageException))]
		public async Task InsufficientlyProtectedMessageReceived() {
			this.Channel = CreateChannel(MessageProtections.None, MessageProtections.TamperProtection);
			await this.ParameterizedReceiveProtectedTestAsync(DateTime.Now, false);
		}

		[Test, ExpectedException(typeof(ProtocolException))]
		public async Task IncomingMessageMissingRequiredParameters() {
			var fields = GetStandardTestFields(FieldFill.IdentifiableButNotAllRequired);
			await this.Channel.ReadFromRequestAsync(CreateHttpRequestInfo(HttpMethod.Get, fields), CancellationToken.None);
		}
	}
}
