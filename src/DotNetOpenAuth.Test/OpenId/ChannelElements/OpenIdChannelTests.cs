//-----------------------------------------------------------------------
// <copyright file="OpenIdChannelTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Test.Mocks;
	using NUnit.Framework;

	[TestFixture]
	public class OpenIdChannelTests : OpenIdTestBase {
		private static readonly TimeSpan maximumMessageAge = TimeSpan.FromHours(3); // good for tests, too long for production
		private OpenIdChannel channel;

		[SetUp]
		public void Setup() {
			this.channel = new OpenIdRelyingPartyChannel(new MemoryCryptoKeyStore(), new MemoryNonceStore(maximumMessageAge), new RelyingPartySecuritySettings(), this.HostFactories);
		}

		[Test]
		public void Ctor() {
			// Verify that the channel stack includes the expected types.
			// While other binding elements may be substituted for these, we'd then have
			// to test them.  Since we're not testing them in the OpenID battery of tests,
			// we make sure they are the standard ones so that we trust they are tested
			// elsewhere by the testing library.
			var replayElement = (StandardReplayProtectionBindingElement)this.channel.BindingElements.SingleOrDefault(el => el is StandardReplayProtectionBindingElement);
			Assert.IsTrue(this.channel.BindingElements.Any(el => el is StandardExpirationBindingElement));
			Assert.IsNotNull(replayElement);

			// Verify that empty nonces are allowed, since OpenID 2.0 allows this.
			Assert.IsTrue(replayElement.AllowZeroLengthNonce);
		}

		/// <summary>
		/// Verifies that the channel sends direct message requests as HTTP POST requests.
		/// </summary>
		[Test]
		public async Task DirectRequestsUsePost() {
			IDirectedProtocolMessage requestMessage = new Mocks.TestDirectedMessage(MessageTransport.Direct) {
				Recipient = new Uri("http://host"),
				Name = "Andrew",
			};
			var httpRequest = this.channel.CreateHttpRequestTestHook(requestMessage);
			Assert.AreEqual(HttpMethod.Post, httpRequest.Method);
			StringAssert.Contains("Name=Andrew", await httpRequest.Content.ReadAsStringAsync());
		}

		/// <summary>
		/// Verifies that direct response messages are encoded using Key Value Form
		/// per OpenID 2.0 section 5.1.2.
		/// </summary>
		/// <remarks>
		/// The validity of the actual KVF encoding is not checked here.  We assume that the KVF encoding
		/// class is verified elsewhere.  We're only checking that the KVF class is being used by the 
		/// <see cref="OpenIdChannel.SendDirectMessageResponse"/> method.
		/// </remarks>
		[Test]
		public async Task DirectResponsesSentUsingKeyValueForm() {
			IProtocolMessage message = MessagingTestBase.GetStandardTestMessage(MessagingTestBase.FieldFill.AllRequired);
			MessageDictionary messageFields = this.MessageDescriptions.GetAccessor(message);
			byte[] expectedBytes = KeyValueFormEncoding.GetBytes(messageFields);
			string expectedContentType = OpenIdChannel.KeyValueFormContentType;

			var directResponse = this.channel.PrepareDirectResponseTestHook(message);
			Assert.AreEqual(expectedContentType, directResponse.Content.Headers.ContentType.MediaType);
			byte[] actualBytes = await directResponse.Content.ReadAsByteArrayAsync();
			Assert.IsTrue(MessagingUtilities.AreEquivalent(expectedBytes, actualBytes));
		}

		/// <summary>
		/// Verifies that direct message responses are read in using the Key Value Form decoder.
		/// </summary>
		[Test]
		public async Task DirectResponsesReceivedAsKeyValueForm() {
			var fields = new Dictionary<string, string> {
				{ "var1", "value1" },
				{ "var2", "value2" },
			};
			var response = new HttpResponseMessage {
				Content = new StreamContent(new MemoryStream(KeyValueFormEncoding.GetBytes(fields))),
			};
			Assert.IsTrue(MessagingUtilities.AreEquivalent(fields, await this.channel.ReadFromResponseCoreAsyncTestHook(response, CancellationToken.None)));
		}

		/// <summary>
		/// Verifies that messages asking for special HTTP status codes get them.
		/// </summary>
		[Test]
		public void SendDirectMessageResponseHonorsHttpStatusCodes() {
			IProtocolMessage message = MessagingTestBase.GetStandardTestMessage(MessagingTestBase.FieldFill.AllRequired);
			var directResponse = this.channel.PrepareDirectResponseTestHook(message);
			Assert.AreEqual(HttpStatusCode.OK, directResponse.StatusCode);

			var httpMessage = new TestDirectResponseMessageWithHttpStatus();
			MessagingTestBase.GetStandardTestMessage(MessagingTestBase.FieldFill.AllRequired, httpMessage);
			httpMessage.HttpStatusCode = HttpStatusCode.NotAcceptable;
			directResponse = this.channel.PrepareDirectResponseTestHook(httpMessage);
			Assert.AreEqual(HttpStatusCode.NotAcceptable, directResponse.StatusCode);
		}
	}
}
