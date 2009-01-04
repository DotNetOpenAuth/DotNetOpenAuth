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
		/// <summary>
		/// A lock to use when checking and setting the <see cref="waitingForMessage"/> 
		/// or the <see cref="simulationCompleted"/> fields.
		/// </summary>
		/// <remarks>
		/// This is a static member so that all coordinating channels share a lock
		/// since they peak at each others fields.
		/// </remarks>
		private static readonly object waitingForMessageCoordinationLock = new object();

		/// <summary>
		/// The original product channel whose behavior is being modified to work
		/// better in automated testing.
		/// </summary>
		private Channel wrappedChannel;

		/// <summary>
		/// A flag set to true when this party in a two-party test has completed
		/// its part of the testing.
		/// </summary>
		private bool simulationCompleted;

		/// <summary>
		/// A thread-coordinating signal that is set when another thread has a 
		/// message ready for this channel to receive.
		/// </summary>
		private EventWaitHandle incomingMessageSignal = new AutoResetEvent(false);

		/// <summary>
		/// A flag used to indicate when this channel is waiting for a message
		/// to arrive.
		/// </summary>
		private bool waitingForMessage;

		/// <summary>
		/// An incoming message that has been posted by a remote channel and 
		/// is waiting for receipt by this channel.
		/// </summary>
		private IProtocolMessage incomingMessage;

		/// <summary>
		/// A delegate that gets a chance to peak at and fiddle with all 
		/// incoming messages.
		/// </summary>
		private Action<IProtocolMessage> incomingMessageFilter;

		/// <summary>
		/// A delegate that gets a chance to peak at and fiddle with all 
		/// outgoing messages.
		/// </summary>
		private Action<IProtocolMessage> outgoingMessageFilter;

		/// <summary>
		/// Initializes a new instance of the <see cref="CoordinatingChannel"/> class.
		/// </summary>
		/// <param name="wrappedChannel">The wrapped channel.  Must not be null.</param>
		/// <param name="incomingMessageFilter">The incoming message filter.  May be null.</param>
		/// <param name="outgoingMessageFilter">The outgoing message filter.  May be null.</param>
		internal CoordinatingChannel(Channel wrappedChannel, Action<IProtocolMessage> incomingMessageFilter, Action<IProtocolMessage> outgoingMessageFilter)
			: base(GetMessageFactory(wrappedChannel), wrappedChannel.BindingElements.ToArray()) {
			ErrorUtilities.VerifyArgumentNotNull(wrappedChannel, "wrappedChannel");

			this.wrappedChannel = wrappedChannel;
			this.incomingMessageFilter = incomingMessageFilter;
			this.outgoingMessageFilter = outgoingMessageFilter;

			// Preserve any customized binding element ordering.
			this.CustomizeBindingElementOrder(this.wrappedChannel.OutgoingBindingElements, this.wrappedChannel.IncomingBindingElements);
		}

		/// <summary>
		/// Gets or sets the coordinating channel used by the other party.
		/// </summary>
		internal CoordinatingChannel RemoteChannel { get; set; }

		/// <summary>
		/// Indicates that the simulation that uses this channel has completed work.
		/// </summary>
		/// <remarks>
		/// Calling this method is not strictly necessary, but it gives the channel
		/// coordination a chance to recognize when another channel is left dangling
		/// waiting for a message from another channel that may never come.
		/// </remarks>
		internal void Close() {
			lock (waitingForMessageCoordinationLock) {
				this.simulationCompleted = true;
				ErrorUtilities.VerifyInternal(!this.RemoteChannel.waitingForMessage || this.RemoteChannel.incomingMessage != null, "This channel is shutting down, yet the remote channel is expecting a message to arrive from us that won't be coming!");
			}
		}

		/// <summary>
		/// Replays the specified message as if it were received again.
		/// </summary>
		/// <param name="message">The message to replay.</param>
		internal void Replay(IProtocolMessage message) {
			this.VerifyMessageAfterReceiving(CloneSerializedParts(message));
		}

		internal void PostMessage(IProtocolMessage message) {
			this.incomingMessage = CloneSerializedParts(message);
			this.incomingMessageSignal.Set();
		}

		protected internal override HttpRequestInfo GetRequestFromContext() {
			return new HttpRequestInfo((IDirectedProtocolMessage)this.AwaitIncomingMessage());
		}

		protected override IProtocolMessage RequestInternal(IDirectedProtocolMessage request) {
			this.ProcessMessageFilter(request, true);
			HttpRequestInfo requestInfo = this.SpoofHttpMethod(request);
			// Drop the outgoing message in the other channel's in-slot and let them know it's there.
			this.RemoteChannel.incomingMessage = requestInfo.Message;
			this.RemoteChannel.incomingMessageSignal.Set();
			// Now wait for a response...
			IProtocolMessage response = this.AwaitIncomingMessage();
			this.ProcessMessageFilter(response, false);
			return response;
		}

		protected override UserAgentResponse SendDirectMessageResponse(IProtocolMessage response) {
			this.ProcessMessageFilter(response, true);
			return new CoordinatingUserAgentResponse(CloneSerializedParts(response), this.RemoteChannel);
		}

		protected override UserAgentResponse SendIndirectMessage(IDirectedProtocolMessage message) {
			this.ProcessMessageFilter(message, true);
			// In this mock transport, direct and indirect messages are the same.
			return this.SendDirectMessageResponse(message);
		}

		protected override IDirectedProtocolMessage ReadFromRequestInternal(HttpRequestInfo request) {
			this.ProcessMessageFilter(request.Message, false);
			return request.Message;
		}

		protected override IDictionary<string, string> ReadFromResponseInternal(DirectWebResponse response) {
			Channel_Accessor accessor = Channel_Accessor.AttachShadow(this.wrappedChannel);
			return accessor.ReadFromResponseInternal(response);
		}

		protected override void VerifyMessageAfterReceiving(IProtocolMessage message) {
			Channel_Accessor accessor = Channel_Accessor.AttachShadow(this.wrappedChannel);
			accessor.VerifyMessageAfterReceiving(message);
		}

		/// <summary>
		/// Spoof HTTP request information for signing/verification purposes.
		/// </summary>
		/// <param name="message">The message to add a pretend HTTP method to.</param>
		/// <returns>A spoofed HttpRequestInfo that wraps the new message.</returns>
		protected virtual HttpRequestInfo SpoofHttpMethod(IDirectedProtocolMessage message) {
			HttpRequestInfo requestInfo = new HttpRequestInfo(message);

			requestInfo.Message = this.CloneSerializedParts(message);

			return requestInfo;
		}

		protected virtual T CloneSerializedParts<T>(T message) where T : class, IProtocolMessage {
			ErrorUtilities.VerifyArgumentNotNull(message, "message");

			IProtocolMessage clonedMessage;
			MessageSerializer serializer = MessageSerializer.Get(message.GetType());
			var fields = serializer.Serialize(message);

			MessageReceivingEndpoint recipient = null;
			var directedMessage = message as IDirectedProtocolMessage;
			var directResponse = message as IDirectResponseProtocolMessage;
			if (directedMessage != null && directedMessage.IsRequest()) {
				if (directedMessage.Recipient != null) {
					recipient = new MessageReceivingEndpoint(directedMessage.Recipient, directedMessage.HttpMethods);
				}

				clonedMessage = this.RemoteChannel.MessageFactory.GetNewRequestMessage(recipient, fields);
			} else if (directResponse != null && directResponse.IsDirectResponse()) {
				clonedMessage = this.RemoteChannel.MessageFactory.GetNewResponseMessage(directResponse.OriginatingRequest, fields);
			} else {
				throw new InvalidOperationException("Totally expected a message to implement one of the two derived interface types.");
			}

			ErrorUtilities.VerifyInternal(clonedMessage != null, "Message factory did not generate a message instance for " + message.GetType().Name);

			// Fill the cloned message with data.
			serializer.Deserialize(fields, clonedMessage);

			return (T)clonedMessage;
		}

		private static IMessageFactory GetMessageFactory(Channel channel) {
			ErrorUtilities.VerifyArgumentNotNull(channel, "channel");

			Channel_Accessor accessor = Channel_Accessor.AttachShadow(channel);
			return accessor.MessageFactory;
		}

		private IProtocolMessage AwaitIncomingMessage() {
			// Special care should be taken so that we don't indefinitely 
			// wait for a message that may never come due to a bug in the product
			// or the test.
			// There are two scenarios that we need to watch out for:
			//  1. Two channels are waiting to receive messages from each other.
			//  2. One channel is waiting for a message that will never come because
			//     the remote party has already finished executing.
			lock (waitingForMessageCoordinationLock) {
				// It's possible that a message was just barely transmitted either to this
				// or the remote channel.  So it's ok for the remote channel to be waiting
				// if either it or we are already about to receive a message.
				ErrorUtilities.VerifyInternal(!this.RemoteChannel.waitingForMessage || this.RemoteChannel.incomingMessage != null || this.incomingMessage != null, "This channel is expecting an incoming message from another channel that is also blocked waiting for an incoming message from us!");

				// It's permissible that the remote channel has already closed if it left a message
				// for us already.
				ErrorUtilities.VerifyInternal(!this.RemoteChannel.simulationCompleted || this.incomingMessage != null, "This channel is expecting an incoming message from another channel that has already been closed.");
				this.waitingForMessage = true;
			}

			this.incomingMessageSignal.WaitOne();

			lock (waitingForMessageCoordinationLock) {
				this.waitingForMessage = false;
			}

			IProtocolMessage response = this.incomingMessage;
			this.incomingMessage = null;
			return response;
		}

		private void ProcessMessageFilter(IProtocolMessage message, bool outgoing) {
			if (outgoing) {
				if (this.outgoingMessageFilter != null) {
					this.outgoingMessageFilter(message);
				}
			} else {
				if (this.incomingMessageFilter != null) {
					this.incomingMessageFilter(message);
				}
			}
		}
	}
}
