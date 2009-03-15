//-----------------------------------------------------------------------
// <copyright file="TestChannel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
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
