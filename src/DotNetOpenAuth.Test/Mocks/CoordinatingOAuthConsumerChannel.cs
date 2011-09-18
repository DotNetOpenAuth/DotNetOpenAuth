//-----------------------------------------------------------------------
// <copyright file="CoordinatingOAuthConsumerChannel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Diagnostics.Contracts;
	using System.Threading;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;

	/// <summary>
	/// A special channel used in test simulations to pass messages directly between two parties.
	/// </summary>
	internal class CoordinatingOAuthConsumerChannel : OAuthConsumerChannel {
		private EventWaitHandle incomingMessageSignal = new AutoResetEvent(false);

		/// <summary>
		/// Initializes a new instance of the <see cref="CoordinatingOAuthConsumerChannel"/> class.
		/// </summary>
		/// <param name="signingBindingElement">The signing element for the Consumer to use.  Null for the Service Provider.</param>
		/// <param name="tokenManager">The token manager to use.</param>
		/// <param name="securitySettings">The security settings.</param>
		internal CoordinatingOAuthConsumerChannel(ITamperProtectionChannelBindingElement signingBindingElement, IConsumerTokenManager tokenManager, DotNetOpenAuth.OAuth.ConsumerSecuritySettings securitySettings)
			: base(
			signingBindingElement,
			new NonceMemoryStore(StandardExpirationBindingElement.MaximumMessageAge),
			tokenManager,
			securitySettings) {
		}

		internal EventWaitHandle IncomingMessageSignal {
			get { return this.incomingMessageSignal; }
		}

		internal IProtocolMessage IncomingMessage { get; set; }

		internal OutgoingWebResponse IncomingRawResponse { get; set; }

		/// <summary>
		/// Gets or sets the coordinating channel used by the other party.
		/// </summary>
		internal CoordinatingOAuthServiceProviderChannel RemoteChannel { get; set; }

		internal OutgoingWebResponse RequestProtectedResource(AccessProtectedResourceRequest request) {
			((ITamperResistantOAuthMessage)request).HttpMethod = this.GetHttpMethod(((ITamperResistantOAuthMessage)request).HttpMethods);
			this.ProcessOutgoingMessage(request);
			HttpRequestInfo requestInfo = this.SpoofHttpMethod(request);
			TestBase.TestLogger.InfoFormat("Sending protected resource request: {0}", requestInfo.Message);
			// Drop the outgoing message in the other channel's in-slot and let them know it's there.
			this.RemoteChannel.IncomingMessage = requestInfo.Message;
			this.RemoteChannel.IncomingMessageSignal.Set();
			return this.AwaitIncomingRawResponse();
		}

		protected internal override HttpRequestInfo GetRequestFromContext() {
			var directedMessage = (IDirectedProtocolMessage)this.AwaitIncomingMessage();
			return new HttpRequestInfo(directedMessage, directedMessage.HttpMethods);
		}

		protected override IProtocolMessage RequestCore(IDirectedProtocolMessage request) {
			HttpRequestInfo requestInfo = this.SpoofHttpMethod(request);
			// Drop the outgoing message in the other channel's in-slot and let them know it's there.
			this.RemoteChannel.IncomingMessage = requestInfo.Message;
			this.RemoteChannel.IncomingMessageSignal.Set();
			// Now wait for a response...
			return this.AwaitIncomingMessage();
		}

		protected override OutgoingWebResponse PrepareDirectResponse(IProtocolMessage response) {
			this.RemoteChannel.IncomingMessage = CloneSerializedParts(response, null);
			this.RemoteChannel.IncomingMessageSignal.Set();
			return new OutgoingWebResponse(); // not used, but returning null is not allowed
		}

		protected override OutgoingWebResponse PrepareIndirectResponse(IDirectedProtocolMessage message) {
			// In this mock transport, direct and indirect messages are the same.
			return this.PrepareDirectResponse(message);
		}

		protected override IDirectedProtocolMessage ReadFromRequestCore(HttpRequestInfo request) {
			return request.Message;
		}

		/// <summary>
		/// Spoof HTTP request information for signing/verification purposes.
		/// </summary>
		/// <param name="message">The message to add a pretend HTTP method to.</param>
		/// <returns>A spoofed HttpRequestInfo that wraps the new message.</returns>
		private HttpRequestInfo SpoofHttpMethod(IDirectedProtocolMessage message) {
			HttpRequestInfo requestInfo = new HttpRequestInfo(message, message.HttpMethods);

			var signedMessage = message as ITamperResistantOAuthMessage;
			if (signedMessage != null) {
				string httpMethod = this.GetHttpMethod(signedMessage.HttpMethods);
				requestInfo.HttpMethod = httpMethod;
				requestInfo.UrlBeforeRewriting = message.Recipient;
				signedMessage.HttpMethod = httpMethod;
			}

			requestInfo.Message = this.CloneSerializedParts(message, requestInfo);

			return requestInfo;
		}

		private IProtocolMessage AwaitIncomingMessage() {
			this.incomingMessageSignal.WaitOne();
			IProtocolMessage response = this.IncomingMessage;
			this.IncomingMessage = null;
			return response;
		}

		private OutgoingWebResponse AwaitIncomingRawResponse() {
			this.incomingMessageSignal.WaitOne();
			OutgoingWebResponse response = this.IncomingRawResponse;
			this.IncomingRawResponse = null;
			return response;
		}

		private T CloneSerializedParts<T>(T message, HttpRequestInfo requestInfo) where T : class, IProtocolMessage {
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

		private string GetHttpMethod(HttpDeliveryMethods methods) {
			return (methods & HttpDeliveryMethods.PostRequest) != 0 ? "POST" : "GET";
		}
	}
}
