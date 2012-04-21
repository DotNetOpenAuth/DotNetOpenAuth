//-----------------------------------------------------------------------
// <copyright file="CoordinatingChannel.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Net;
	using System.Text;
	using System.Threading;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.Test.OpenId;
	using NUnit.Framework;

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
		/// A thread-coordinating signal that is set briefly by this thread whenever
		/// a message is picked up.
		/// </summary>
		private EventWaitHandle messageReceivedSignal = new AutoResetEvent(false);

		/// <summary>
		/// A flag used to indicate when this channel is waiting for a message
		/// to arrive.
		/// </summary>
		private bool waitingForMessage;

		/// <summary>
		/// An incoming message that has been posted by a remote channel and 
		/// is waiting for receipt by this channel.
		/// </summary>
		private IDictionary<string, string> incomingMessage;

		/// <summary>
		/// The recipient URL of the <see cref="incomingMessage"/>, where applicable.
		/// </summary>
		private MessageReceivingEndpoint incomingMessageRecipient;

		/// <summary>
		/// The headers of the <see cref="incomingMessage"/>, where applicable.
		/// </summary>
		private WebHeaderCollection incomingMessageHttpHeaders;

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
			Requires.NotNull(wrappedChannel, "wrappedChannel");

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
				if (this.RemoteChannel.waitingForMessage && this.RemoteChannel.incomingMessage == null) {
					TestUtilities.TestLogger.Debug("CoordinatingChannel is closing while remote channel is waiting for an incoming message.  Signaling channel to unblock it to receive a null message.");
					this.RemoteChannel.incomingMessageSignal.Set();
				}

				this.Dispose();
			}
		}

		/// <summary>
		/// Replays the specified message as if it were received again.
		/// </summary>
		/// <param name="message">The message to replay.</param>
		internal void Replay(IProtocolMessage message) {
			this.ProcessIncomingMessage(this.CloneSerializedParts(message));
		}

		/// <summary>
		/// Called from a remote party's thread to post a message to this channel for processing.
		/// </summary>
		/// <param name="message">The message that this channel should receive.  This message will be cloned.</param>
		internal void PostMessage(IProtocolMessage message) {
			if (this.incomingMessage != null) {
				// The remote party hasn't picked up the last message we sent them.
				// Wait for a short period for them to pick it up before failing.
				TestBase.TestLogger.Warn("We're blocked waiting to send a message to the remote party and they haven't processed the last message we sent them.");
				this.RemoteChannel.messageReceivedSignal.WaitOne(500);
			}
			ErrorUtilities.VerifyInternal(this.incomingMessage == null, "Oops, a message is already waiting for the remote party!");
			this.incomingMessage = this.MessageDescriptions.GetAccessor(message).Serialize();
			var directedMessage = message as IDirectedProtocolMessage;
			this.incomingMessageRecipient = (directedMessage != null && directedMessage.Recipient != null) ? new MessageReceivingEndpoint(directedMessage.Recipient, directedMessage.HttpMethods) : null;
			var httpMessage = message as IHttpDirectRequest;
			this.incomingMessageHttpHeaders = (httpMessage != null) ? httpMessage.Headers.Clone() : null;
			this.incomingMessageSignal.Set();
		}

		protected internal override HttpRequestBase GetRequestFromContext() {
			MessageReceivingEndpoint recipient;
			WebHeaderCollection headers;
			var messageData = this.AwaitIncomingMessage(out recipient, out headers);
			CoordinatingHttpRequestInfo result;
			if (messageData != null) {
				result = new CoordinatingHttpRequestInfo(this, this.MessageFactory, messageData, recipient);
			} else {
				result = new CoordinatingHttpRequestInfo(recipient);
			}

			if (headers != null) {
				headers.ApplyTo(result.Headers);
			}

			return result;
		}

		protected override IProtocolMessage RequestCore(IDirectedProtocolMessage request) {
			this.ProcessMessageFilter(request, true);

			// Drop the outgoing message in the other channel's in-slot and let them know it's there.
			this.RemoteChannel.PostMessage(request);

			// Now wait for a response...
			MessageReceivingEndpoint recipient;
			WebHeaderCollection headers;
			IDictionary<string, string> responseData = this.AwaitIncomingMessage(out recipient, out headers);
			ErrorUtilities.VerifyInternal(recipient == null, "The recipient is expected to be null for direct responses.");

			// And deserialize it.
			IDirectResponseProtocolMessage responseMessage = this.MessageFactory.GetNewResponseMessage(request, responseData);
			if (responseMessage == null) {
				return null;
			}

			var responseAccessor = this.MessageDescriptions.GetAccessor(responseMessage);
			responseAccessor.Deserialize(responseData);
			var responseMessageHttpRequest = responseMessage as IHttpDirectRequest;
			if (headers != null && responseMessageHttpRequest != null) {
				headers.ApplyTo(responseMessageHttpRequest.Headers);
			}

			this.ProcessMessageFilter(responseMessage, false);
			return responseMessage;
		}

		protected override OutgoingWebResponse PrepareDirectResponse(IProtocolMessage response) {
			this.ProcessMessageFilter(response, true);
			return new CoordinatingOutgoingWebResponse(response, this.RemoteChannel);
		}

		protected override OutgoingWebResponse PrepareIndirectResponse(IDirectedProtocolMessage message) {
			this.ProcessMessageFilter(message, true);
			// In this mock transport, direct and indirect messages are the same.
			return this.PrepareDirectResponse(message);
		}

		protected override IDirectedProtocolMessage ReadFromRequestCore(HttpRequestBase request) {
			var mockRequest = (CoordinatingHttpRequestInfo)request;
			if (mockRequest.Message != null) {
				this.ProcessMessageFilter(mockRequest.Message, false);
			}

			return mockRequest.Message;
		}

		protected override IDictionary<string, string> ReadFromResponseCore(IncomingWebResponse response) {
			return this.wrappedChannel.ReadFromResponseCoreTestHook(response);
		}

		protected override void ProcessIncomingMessage(IProtocolMessage message) {
			this.wrappedChannel.ProcessIncomingMessageTestHook(message);
		}

		/// <summary>
		/// Clones a message, instantiating the new instance using <i>this</i> channel's
		/// message factory.
		/// </summary>
		/// <typeparam name="T">The type of message to clone.</typeparam>
		/// <param name="message">The message to clone.</param>
		/// <returns>The new instance of the message.</returns>
		/// <remarks>
		/// This Clone method should <i>not</i> be used to send message clones to the remote
		/// channel since their message factory is not used.
		/// </remarks>
		protected virtual T CloneSerializedParts<T>(T message) where T : class, IProtocolMessage {
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

				clonedMessage = this.MessageFactory.GetNewRequestMessage(recipient, fields);
			} else if (directResponse != null && directResponse.IsDirectResponse()) {
				clonedMessage = this.MessageFactory.GetNewResponseMessage(directResponse.OriginatingRequest, fields);
			} else {
				throw new InvalidOperationException("Totally expected a message to implement one of the two derived interface types.");
			}

			ErrorUtilities.VerifyInternal(clonedMessage != null, "Message factory did not generate a message instance for " + message.GetType().Name);

			// Fill the cloned message with data.
			var clonedMessageAccessor = this.MessageDescriptions.GetAccessor(clonedMessage);
			clonedMessageAccessor.Deserialize(fields);

			return (T)clonedMessage;
		}

		private static IMessageFactory GetMessageFactory(Channel channel) {
			Requires.NotNull(channel, "channel");

			return channel.MessageFactoryTestHook;
		}

		private IDictionary<string, string> AwaitIncomingMessage(out MessageReceivingEndpoint recipient, out WebHeaderCollection headers) {
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
				var response = this.incomingMessage;
				recipient = this.incomingMessageRecipient;
				headers = this.incomingMessageHttpHeaders;
				this.incomingMessage = null;
				this.incomingMessageRecipient = null;
				this.incomingMessageHttpHeaders = null;

				// Briefly signal to another thread that might be waiting for our inbox to be empty
				this.messageReceivedSignal.Set();
				this.messageReceivedSignal.Reset();

				return response;
			}
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
