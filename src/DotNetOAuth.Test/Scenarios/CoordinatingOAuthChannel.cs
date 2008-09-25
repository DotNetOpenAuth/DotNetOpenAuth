//-----------------------------------------------------------------------
// <copyright file="CoordinatingOAuthChannel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Scenarios {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOAuth.ChannelElements;
	using DotNetOAuth.Messaging.Bindings;
	using DotNetOAuth.Messaging;
using System.Threading;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

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
			RemoteChannel.incomingMessage = request;
			RemoteChannel.incomingMessageSignal.Set();
			// Now wait for a response...
			return AwaitIncomingMessage();
		}

		protected override void SendDirectMessageResponse(IProtocolMessage response) {
			TestBase.TestLogger.InfoFormat("Sending response: {0}", response);
			RemoteChannel.incomingMessage = response;
			RemoteChannel.incomingMessageSignal.Set();
		}

		protected override void SendIndirectMessage(IDirectedProtocolMessage message) {
			TestBase.TestLogger.InfoFormat("Sending indirect message: {0}", message);
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
	}
}
