//-----------------------------------------------------------------------
// <copyright file="CoordinatingOAuthChannel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Scenarios {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using DotNetOAuth.ChannelElements;
	using DotNetOAuth.Messaging.Bindings;
	using DotNetOAuth.Messaging;
	using System.Threading;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using DotNetOAuth.Messaging.Reflection;

	/// <summary>
	/// A special channel used in test simulations to pass messages directly between two parties.
	/// </summary>
	internal class CoordinatingOAuthChannel : OAuthChannel {
		/// <summary>
		/// Initializes a new instance of the <see cref="CoordinatingOAuthChannel"/> class for Consumers.
		/// </summary>
		/// <param name="signingBindingElement">
		/// The signing element for the Consumer to use.  Null for the Service Provider.
		/// </param>
		internal CoordinatingOAuthChannel(SigningBindingElementBase signingBindingElement)
			: base(signingBindingElement, new NonceMemoryStore(StandardExpirationBindingElement.DefaultMaximumMessageAge), new OAuthMessageTypeProvider(), new Mocks.TestWebRequestHandler()) {
		}

		/// <summary>
		/// Gets or sets the coordinating channel used by the other party.
		/// </summary>
		internal CoordinatingOAuthChannel RemoteChannel { get; set; }

		private EventWaitHandle incomingMessageSignal = new AutoResetEvent(false);
		private IProtocolMessage incomingMessage;

		protected override IProtocolMessage RequestInternal(IDirectedProtocolMessage request) {
			TestBase.TestLogger.InfoFormat("Sending request: {0}", request);
			// Drop the outgoing message in the other channel's in-slot and let them know it's there.
			RemoteChannel.incomingMessage = CloneSerializedParts(request);
			RemoteChannel.incomingMessageSignal.Set();
			// Now wait for a response...
			return AwaitIncomingMessage();
		}

		protected override void SendDirectMessageResponse(IProtocolMessage response) {
			TestBase.TestLogger.InfoFormat("Sending response: {0}", response);
			RemoteChannel.incomingMessage = CloneSerializedParts(response);
			RemoteChannel.incomingMessageSignal.Set();
		}

		protected override void SendIndirectMessage(IDirectedProtocolMessage message) {
			TestBase.TestLogger.Info("Next response is an indirect message...");
			// In this mock transport, direct and indirect messages are the same.
			SendDirectMessageResponse(message);
		}

		protected override HttpRequestInfo GetRequestFromContext() {
			return new HttpRequestInfo(AwaitIncomingMessage());
		}

		protected override IProtocolMessage ReadFromRequestInternal(HttpRequestInfo request) {
			return request.Message;
		}

		private IProtocolMessage AwaitIncomingMessage() {
			this.incomingMessageSignal.WaitOne();
			IProtocolMessage response = this.incomingMessage;
			this.incomingMessage = null;
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
				} else {
					throw new InvalidOperationException("Unrecognized constructor signature.");
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
