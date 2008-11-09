//-----------------------------------------------------------------------
// <copyright file="CoordinatingOAuthChannel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Threading;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;

	/// <summary>
	/// A special channel used in test simulations to pass messages directly between two parties.
	/// </summary>
	internal class CoordinatingOAuthChannel : OAuthChannel {
		private EventWaitHandle incomingMessageSignal = new AutoResetEvent(false);
		private IProtocolMessage incomingMessage;
		private Response incomingRawResponse;

		/// <summary>
		/// Initializes a new instance of the <see cref="CoordinatingOAuthChannel"/> class for Consumers.
		/// </summary>
		/// <param name="signingBindingElement">
		/// The signing element for the Consumer to use.  Null for the Service Provider.
		/// </param>
		/// <param name="isConsumer">True if this channel is constructed for a Consumer.</param>
		/// <param name="tokenManager">The token manager to use.</param>
		internal CoordinatingOAuthChannel(ITamperProtectionChannelBindingElement signingBindingElement, bool isConsumer, ITokenManager tokenManager)
			: base(
			signingBindingElement,
			new NonceMemoryStore(StandardExpirationBindingElement.DefaultMaximumMessageAge),
			tokenManager,
			isConsumer ? (IMessageTypeProvider)new OAuthConsumerMessageTypeProvider() : new OAuthServiceProviderMessageTypeProvider(tokenManager)) {
		}

		/// <summary>
		/// Gets or sets the coordinating channel used by the other party.
		/// </summary>
		internal CoordinatingOAuthChannel RemoteChannel { get; set; }

		internal Response RequestProtectedResource(AccessProtectedResourceRequest request) {
			((ITamperResistantOAuthMessage)request).HttpMethod = this.GetHttpMethod(((ITamperResistantOAuthMessage)request).HttpMethods);
			this.PrepareMessageForSending(request);
			HttpRequestInfo requestInfo = this.SpoofHttpMethod(request);
			TestBase.TestLogger.InfoFormat("Sending protected resource request: {0}", requestInfo.Message);
			// Drop the outgoing message in the other channel's in-slot and let them know it's there.
			this.RemoteChannel.incomingMessage = requestInfo.Message;
			this.RemoteChannel.incomingMessageSignal.Set();
			return this.AwaitIncomingRawResponse();
		}

		internal void SendDirectRawResponse(Response response) {
			this.RemoteChannel.incomingRawResponse = response;
			this.RemoteChannel.incomingMessageSignal.Set();
		}

		protected internal override HttpRequestInfo GetRequestFromContext() {
			return new HttpRequestInfo(this.AwaitIncomingMessage());
		}

		protected override IProtocolMessage RequestInternal(IDirectedProtocolMessage request) {
			HttpRequestInfo requestInfo = this.SpoofHttpMethod(request);
			// Drop the outgoing message in the other channel's in-slot and let them know it's there.
			this.RemoteChannel.incomingMessage = requestInfo.Message;
			this.RemoteChannel.incomingMessageSignal.Set();
			// Now wait for a response...
			return this.AwaitIncomingMessage();
		}

		protected override Response SendDirectMessageResponse(IProtocolMessage response) {
			this.RemoteChannel.incomingMessage = CloneSerializedParts(response, null);
			this.RemoteChannel.incomingMessageSignal.Set();
			return null;
		}

		protected override Response SendIndirectMessage(IDirectedProtocolMessage message) {
			// In this mock transport, direct and indirect messages are the same.
			return this.SendDirectMessageResponse(message);
		}

		protected override IProtocolMessage ReadFromRequestInternal(HttpRequestInfo request) {
			return request.Message;
		}

		/// <summary>
		/// Spoof HTTP request information for signing/verification purposes.
		/// </summary>
		/// <param name="message">The message to add a pretend HTTP method to.</param>
		/// <returns>A spoofed HttpRequestInfo that wraps the new message.</returns>
		private HttpRequestInfo SpoofHttpMethod(IDirectedProtocolMessage message) {
			HttpRequestInfo requestInfo = new HttpRequestInfo(message);

			var signedMessage = message as ITamperResistantOAuthMessage;
			if (signedMessage != null) {
				string httpMethod = this.GetHttpMethod(signedMessage.HttpMethods);
				requestInfo.HttpMethod = httpMethod;
				requestInfo.Url = message.Recipient;
				signedMessage.HttpMethod = httpMethod;
			}

			requestInfo.Message = this.CloneSerializedParts(message, requestInfo);

			return requestInfo;
		}

		private IProtocolMessage AwaitIncomingMessage() {
			this.incomingMessageSignal.WaitOne();
			IProtocolMessage response = this.incomingMessage;
			this.incomingMessage = null;
			return response;
		}

		private Response AwaitIncomingRawResponse() {
			this.incomingMessageSignal.WaitOne();
			Response response = this.incomingRawResponse;
			this.incomingRawResponse = null;
			return response;
		}

		private T CloneSerializedParts<T>(T message, HttpRequestInfo requestInfo) where T : class, IProtocolMessage {
			if (message == null) {
				throw new ArgumentNullException("message");
			}

			MessageReceivingEndpoint recipient = null;
			IOAuthDirectedMessage directedMessage = message as IOAuthDirectedMessage;
			if (directedMessage != null && directedMessage.Recipient != null) {
				recipient = new MessageReceivingEndpoint(directedMessage.Recipient, directedMessage.HttpMethods);
			}

			MessageSerializer serializer = MessageSerializer.Get(message.GetType());
			return (T)serializer.Deserialize(serializer.Serialize(message), recipient);
		}

		private string GetHttpMethod(HttpDeliveryMethods methods) {
			return (methods & HttpDeliveryMethods.PostRequest) != 0 ? "POST" : "GET";
		}
	}
}
