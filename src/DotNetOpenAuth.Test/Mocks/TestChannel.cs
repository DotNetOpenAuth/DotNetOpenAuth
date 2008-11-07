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

	internal class TestChannel : Channel {
		internal TestChannel()
			: this(new TestMessageTypeProvider()) {
		}

		internal TestChannel(IMessageTypeProvider messageTypeProvider, params IChannelBindingElement[] bindingElements)
			: base(messageTypeProvider, bindingElements) {
		}

		protected override IDictionary<string, string> ReadFromResponseInternal(Response response) {
			throw new NotImplementedException("ReadFromResponseInternal");
		}

		protected override HttpWebRequest CreateHttpRequest(IDirectedProtocolMessage request) {
			throw new NotImplementedException("CreateHttpRequest");
		}

		protected override Response SendDirectMessageResponse(IProtocolMessage response) {
			throw new NotImplementedException("SendDirectMessageResponse");
		}
	}
}
