//-----------------------------------------------------------------------
// <copyright file="CoordinatingChannel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using DotNetOpenAuth.Messaging;

	internal class CoordinatingChannel : Channel {
		private Channel wrappedChannel;
		private EventWaitHandle incomingMessageSignal = new AutoResetEvent(false);
		private IProtocolMessage incomingMessage;
		private Response incomingRawResponse;

		internal CoordinatingChannel(Channel wrappedChannel)
			: base(GetMessageTypeProvider(wrappedChannel), wrappedChannel.BindingElements.ToArray()) {
			ErrorUtilities.VerifyArgumentNotNull(wrappedChannel, "wrappedChannel");

			this.wrappedChannel = wrappedChannel;
		}

		/// <summary>
		/// Gets or sets the coordinating channel used by the other party.
		/// </summary>
		internal CoordinatingChannel RemoteChannel { get; set; }

		protected internal override HttpRequestInfo GetRequestFromContext() {
			return new HttpRequestInfo((IDirectedProtocolMessage)this.AwaitIncomingMessage());
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

		protected override IDirectedProtocolMessage ReadFromRequestInternal(HttpRequestInfo request) {
			return request.Message;
		}

		protected override IDictionary<string, string> ReadFromResponseInternal(Response response) {
			Channel_Accessor accessor = Channel_Accessor.AttachShadow(this.wrappedChannel);
			return accessor.ReadFromResponseInternal(response);
		}

		/// <summary>
		/// Spoof HTTP request information for signing/verification purposes.
		/// </summary>
		/// <param name="message">The message to add a pretend HTTP method to.</param>
		/// <returns>A spoofed HttpRequestInfo that wraps the new message.</returns>
		protected virtual HttpRequestInfo SpoofHttpMethod(IDirectedProtocolMessage message) {
			HttpRequestInfo requestInfo = new HttpRequestInfo(message);

			requestInfo.Message = this.CloneSerializedParts(message, requestInfo);

			return requestInfo;
		}

		protected virtual T CloneSerializedParts<T>(T message, HttpRequestInfo requestInfo) where T : class, IProtocolMessage {
			if (message == null) {
				throw new ArgumentNullException("message");
			}

			MessageReceivingEndpoint recipient = null;
			IDirectedProtocolMessage directedMessage = message as IDirectedProtocolMessage;
			if (directedMessage != null && directedMessage.Recipient != null) {
				recipient = new MessageReceivingEndpoint(directedMessage.Recipient, directedMessage.HttpMethods);
			}

			MessageSerializer serializer = MessageSerializer.Get(message.GetType());
			return (T)serializer.Deserialize(serializer.Serialize(message), recipient);
		}

		private static IMessageTypeProvider GetMessageTypeProvider(Channel channel) {
			ErrorUtilities.VerifyArgumentNotNull(channel, "channel");

			Channel_Accessor accessor = Channel_Accessor.AttachShadow(channel);
			return accessor.MessageTypeProvider;
		}

		private IProtocolMessage AwaitIncomingMessage() {
			this.incomingMessageSignal.WaitOne();
			IProtocolMessage response = this.incomingMessage;
			this.incomingMessage = null;
			return response;
		}
	}
}
