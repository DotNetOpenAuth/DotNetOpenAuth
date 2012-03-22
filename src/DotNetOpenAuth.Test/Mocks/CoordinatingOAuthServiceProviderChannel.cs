//-----------------------------------------------------------------------
// <copyright file="CoordinatingOAuthServiceProviderChannel.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Diagnostics.Contracts;
	using System.Threading;
	using System.Web;

	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;
	using NUnit.Framework;

	/// <summary>
	/// A special channel used in test simulations to pass messages directly between two parties.
	/// </summary>
	internal class CoordinatingOAuthServiceProviderChannel : OAuthServiceProviderChannel {
		private EventWaitHandle incomingMessageSignal = new AutoResetEvent(false);

		/// <summary>
		/// Initializes a new instance of the <see cref="CoordinatingOAuthServiceProviderChannel"/> class.
		/// </summary>
		/// <param name="signingBindingElement">The signing element for the Consumer to use.  Null for the Service Provider.</param>
		/// <param name="tokenManager">The token manager to use.</param>
		/// <param name="securitySettings">The security settings.</param>
		internal CoordinatingOAuthServiceProviderChannel(ITamperProtectionChannelBindingElement signingBindingElement, IServiceProviderTokenManager tokenManager, DotNetOpenAuth.OAuth.ServiceProviderSecuritySettings securitySettings)
			: base(
			signingBindingElement,
			new NonceMemoryStore(StandardExpirationBindingElement.MaximumMessageAge),
			tokenManager,
			securitySettings,
			new OAuthServiceProviderMessageFactory(tokenManager)) {
		}

		internal EventWaitHandle IncomingMessageSignal {
			get { return this.incomingMessageSignal; }
		}

		internal IProtocolMessage IncomingMessage { get; set; }

		internal OutgoingWebResponse IncomingRawResponse { get; set; }

		/// <summary>
		/// Gets or sets the coordinating channel used by the other party.
		/// </summary>
		internal CoordinatingOAuthConsumerChannel RemoteChannel { get; set; }

		internal OutgoingWebResponse RequestProtectedResource(AccessProtectedResourceRequest request) {
			((ITamperResistantOAuthMessage)request).HttpMethod = GetHttpMethod(((ITamperResistantOAuthMessage)request).HttpMethods);
			this.ProcessOutgoingMessage(request);
			var requestInfo = this.SpoofHttpMethod(request);
			TestBase.TestLogger.InfoFormat("Sending protected resource request: {0}", requestInfo.Message);
			// Drop the outgoing message in the other channel's in-slot and let them know it's there.
			this.RemoteChannel.IncomingMessage = requestInfo.Message;
			this.RemoteChannel.IncomingMessageSignal.Set();
			return this.AwaitIncomingRawResponse();
		}

		internal void SendDirectRawResponse(OutgoingWebResponse response) {
			this.RemoteChannel.IncomingRawResponse = response;
			this.RemoteChannel.IncomingMessageSignal.Set();
		}

		protected internal override HttpRequestBase GetRequestFromContext() {
			var directedMessage = (IDirectedProtocolMessage)this.AwaitIncomingMessage();
			return new CoordinatingHttpRequestInfo(directedMessage, directedMessage.HttpMethods);
		}

		protected override IProtocolMessage RequestCore(IDirectedProtocolMessage request) {
			var requestInfo = this.SpoofHttpMethod(request);
			// Drop the outgoing message in the other channel's in-slot and let them know it's there.
			this.RemoteChannel.IncomingMessage = requestInfo.Message;
			this.RemoteChannel.IncomingMessageSignal.Set();
			// Now wait for a response...
			return this.AwaitIncomingMessage();
		}

		protected override OutgoingWebResponse PrepareDirectResponse(IProtocolMessage response) {
			this.RemoteChannel.IncomingMessage = this.CloneSerializedParts(response);
			this.RemoteChannel.IncomingMessageSignal.Set();
			return new OutgoingWebResponse(); // not used, but returning null is not allowed
		}

		protected override OutgoingWebResponse PrepareIndirectResponse(IDirectedProtocolMessage message) {
			// In this mock transport, direct and indirect messages are the same.
			return this.PrepareDirectResponse(message);
		}

		protected override IDirectedProtocolMessage ReadFromRequestCore(HttpRequestBase request) {
			var mockRequest = (CoordinatingHttpRequestInfo)request;
			return mockRequest.Message;
		}

		private static string GetHttpMethod(HttpDeliveryMethods methods) {
			return (methods & HttpDeliveryMethods.PostRequest) != 0 ? "POST" : "GET";
		}

		/// <summary>
		/// Spoof HTTP request information for signing/verification purposes.
		/// </summary>
		/// <param name="message">The message to add a pretend HTTP method to.</param>
		/// <returns>A spoofed HttpRequestInfo that wraps the new message.</returns>
		private CoordinatingHttpRequestInfo SpoofHttpMethod(IDirectedProtocolMessage message) {
			var signedMessage = message as ITamperResistantOAuthMessage;
			if (signedMessage != null) {
				string httpMethod = GetHttpMethod(signedMessage.HttpMethods);
				signedMessage.HttpMethod = httpMethod;
			}

			var requestInfo = new CoordinatingHttpRequestInfo(this.CloneSerializedParts(message), message.HttpMethods);
			return requestInfo;
		}

		private IProtocolMessage AwaitIncomingMessage() {
			this.IncomingMessageSignal.WaitOne();
			Assert.That(this.IncomingMessage, Is.Not.Null, "Incoming message signaled, but none supplied.");
			IProtocolMessage response = this.IncomingMessage;
			this.IncomingMessage = null;
			return response;
		}

		private OutgoingWebResponse AwaitIncomingRawResponse() {
			this.IncomingMessageSignal.WaitOne();
			OutgoingWebResponse response = this.IncomingRawResponse;
			this.IncomingRawResponse = null;
			return response;
		}

		private T CloneSerializedParts<T>(T message) where T : class, IProtocolMessage {
			Requires.NotNull(message, "message");

			IProtocolMessage clonedMessage;
			var messageAccessor = this.MessageDescriptions.GetAccessor(message);
			var fields = messageAccessor.Serialize();

			MessageReceivingEndpoint recipient = null;
			var directedMessage = message as IDirectedProtocolMessage;
			var directResponse = message as IDirectResponseProtocolMessage;
			if (directedMessage != null && directedMessage.IsRequest()) {
				if (directedMessage.Recipient != null) {
					recipient = new MessageReceivingEndpoint(directedMessage.Recipient, directedMessage.HttpMethods);
				}

				clonedMessage = this.RemoteChannel.MessageFactoryTestHook.GetNewRequestMessage(recipient, fields);
			} else if (directResponse != null && directResponse.IsDirectResponse()) {
				clonedMessage = this.RemoteChannel.MessageFactoryTestHook.GetNewResponseMessage(directResponse.OriginatingRequest, fields);
			} else {
				throw new InvalidOperationException("Totally expected a message to implement one of the two derived interface types.");
			}

			// Fill the cloned message with data.
			var clonedMessageAccessor = this.MessageDescriptions.GetAccessor(clonedMessage);
			clonedMessageAccessor.Deserialize(fields);

			return (T)clonedMessage;
		}
	}
}
