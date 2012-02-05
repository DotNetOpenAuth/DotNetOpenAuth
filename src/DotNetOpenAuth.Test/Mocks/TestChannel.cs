//-----------------------------------------------------------------------
// <copyright file="TestChannel.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Net;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;

	internal class TestChannel : Channel {
		internal TestChannel()
			: this(new TestMessageFactory()) {
		}

		internal TestChannel(MessageDescriptionCollection messageDescriptions)
			: this() {
			this.MessageDescriptions = messageDescriptions;
		}

		internal TestChannel(IMessageFactory messageTypeProvider, params IChannelBindingElement[] bindingElements)
			: base(messageTypeProvider, bindingElements) {
		}

		/// <summary>
		/// Deserializes a dictionary of values into a message.
		/// </summary>
		/// <param name="fields">The dictionary of values that were read from an HTTP request or response.</param>
		/// <param name="recipient">Information about where the message was directed.  Null for direct response messages.</param>
		/// <returns>
		/// The deserialized message, or null if no message could be recognized in the provided data.
		/// </returns>
		/// <remarks>
		/// This internal method exposes Receive directly to unit tests for easier deserialization of custom (possibly malformed) messages.
		/// </remarks>
		internal new IProtocolMessage Receive(Dictionary<string, string> fields, MessageReceivingEndpoint recipient) {
			return base.Receive(fields, recipient);
		}

		protected override IDictionary<string, string> ReadFromResponseCore(IncomingWebResponse response) {
			throw new NotImplementedException("ReadFromResponseInternal");
		}

		protected override HttpWebRequest CreateHttpRequest(IDirectedProtocolMessage request) {
			throw new NotImplementedException("CreateHttpRequest");
		}

		protected override OutgoingWebResponse PrepareDirectResponse(IProtocolMessage response) {
			throw new NotImplementedException("SendDirectMessageResponse");
		}
	}
}
