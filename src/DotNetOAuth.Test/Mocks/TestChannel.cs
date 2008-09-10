//-----------------------------------------------------------------------
// <copyright file="TestChannel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOAuth.Messaging;

	internal class TestChannel : Channel {
		internal TestChannel()
			: base(new TestMessageTypeProvider()) {
		}

		protected internal override IProtocolMessage Request(IDirectedProtocolMessage request) {
			throw new NotImplementedException("Request");
		}

		protected internal override IProtocolMessage ReadFromResponse(System.IO.Stream responseStream) {
			throw new NotImplementedException("ReadFromResponse");
		}

		protected override void SendDirectMessageResponse(IProtocolMessage response) {
			throw new NotImplementedException("SendDirectMessageResponse");
		}
	}
}
