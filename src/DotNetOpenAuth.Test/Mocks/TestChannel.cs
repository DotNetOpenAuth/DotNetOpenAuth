//-----------------------------------------------------------------------
// <copyright file="TestChannel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	internal class TestChannel : Channel {
		internal TestChannel()
			: this(new TestMessageTypeProvider()) {
		}

		internal TestChannel(IMessageTypeProvider messageTypeProvider, params IChannelBindingElement[] bindingElements)
			: base(messageTypeProvider, bindingElements) {
		}

		protected override IProtocolMessage RequestInternal(IDirectedProtocolMessage request) {
			throw new NotImplementedException("Request");
		}

		protected override IProtocolMessage ReadFromResponseInternal(System.IO.Stream responseStream) {
			throw new NotImplementedException("ReadFromResponse");
		}

		protected override Response SendDirectMessageResponse(IProtocolMessage response) {
			throw new NotImplementedException("SendDirectMessageResponse");
		}
	}
}
