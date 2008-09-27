//-----------------------------------------------------------------------
// <copyright file="CoordinatingOAuthChannel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Scenarios {
	using System;
	using System.Reflection;
	using System.Threading;
	using DotNetOAuth.ChannelElements;
	using DotNetOAuth.Messages;
	using DotNetOAuth.Messaging;
	using DotNetOAuth.Messaging.Bindings;
	using DotNetOAuth.Messaging.Reflection;
	using DotNetOAuth.Test.Mocks;

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
		internal CoordinatingOAuthChannel(ITamperProtectionChannelBindingElement signingBindingElement)
			: base(
			signingBindingElement,
			new NonceMemoryStore(StandardExpirationBindingElement.DefaultMaximumMessageAge),
			new OAuthMessageTypeProvider(new InMemoryTokenManager()),
			new TestWebRequestHandler()) {
		}

		/// <summary>
		/// Gets or sets the coordinating channel used by the other party.
		/// </summary>
		internal CoordinatingOAuthChannel RemoteChannel { get; set; }

		internal Response RequestProtectedResource(AccessProtectedResourcesMessage request) {
			TestBase.TestLogger.InfoFormat("Sending protected resource request: {0}", request);
			PrepareMessageForSending(request);
			// Drop the outgoing message in the other channel's in-slot and let them know it's there.
			this.RemoteChannel.incomingMessage = request;
			this.RemoteChannel.incomingMessageSignal.Set();
			return this.AwaitIncomingRawResponse();
		}

		internal void SendDirectRawResponse(Response response) {
			this.RemoteChannel.incomingRawResponse = response;
			this.RemoteChannel.incomingMessageSignal.Set();
		}

		protected override IProtocolMessage RequestInternal(IDirectedProtocolMessage request) {
			TestBase.TestLogger.InfoFormat("Sending request: {0}", request);
			// Drop the outgoing message in the other channel's in-slot and let them know it's there.
			this.RemoteChannel.incomingMessage = CloneSerializedParts(request);
			this.RemoteChannel.incomingMessageSignal.Set();
			// Now wait for a response...
			return this.AwaitIncomingMessage();
		}

		protected override void SendDirectMessageResponse(IProtocolMessage response) {
			TestBase.TestLogger.InfoFormat("Sending response: {0}", response);
			this.RemoteChannel.incomingMessage = CloneSerializedParts(response);
			this.RemoteChannel.incomingMessageSignal.Set();
		}

		protected override void SendIndirectMessage(IDirectedProtocolMessage message) {
			TestBase.TestLogger.Info("Next response is an indirect message...");
			// In this mock transport, direct and indirect messages are the same.
			this.SendDirectMessageResponse(message);
		}

		protected override HttpRequestInfo GetRequestFromContext() {
			return new HttpRequestInfo(this.AwaitIncomingMessage());
		}

		protected override IProtocolMessage ReadFromRequestInternal(HttpRequestInfo request) {
			return request.Message ?? base.ReadFromRequestInternal(request);
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

		private T CloneSerializedParts<T>(T message) where T : class, IProtocolMessage {
			if (message == null) {
				throw new ArgumentNullException("message");
			}

			T cloned;
			var directedMessage = message as IOAuthDirectedMessage;
			if (directedMessage != null) {
				// Some OAuth messages take just the recipient, while others take the whole endpoint
				ConstructorInfo ctor;
				if ((ctor = message.GetType().GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(Uri) }, null)) != null) {
					cloned = (T)ctor.Invoke(new object[] { directedMessage.Recipient });
				} else if ((ctor = message.GetType().GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ServiceProviderEndpoint) }, null)) != null) {
					ServiceProviderEndpoint endpoint = new ServiceProviderEndpoint(
						directedMessage.Recipient,
						directedMessage.HttpMethods);
					cloned = (T)ctor.Invoke(new object[] { endpoint });
				} else if ((ctor = message.GetType().GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[0], null)) != null) {
					cloned = (T)ctor.Invoke(new object[0]);
				} else {
					throw new InvalidOperationException("Unrecognized constructor signature on type " + message.GetType());
				}
			} else {
				cloned = (T)Activator.CreateInstance(message.GetType(), true);
			}

			var messageDictionary = new MessageDictionary(message);
			var clonedDictionary = new MessageDictionary(cloned);

			foreach (var pair in messageDictionary) {
				clonedDictionary[pair.Key] = pair.Value;
			}

			return cloned;
		}
	}
}
