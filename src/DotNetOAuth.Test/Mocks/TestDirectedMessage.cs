//-----------------------------------------------------------------------
// <copyright file="TestDirectedMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Mocks {
	using System;
	using DotNetOAuth.ChannelElements;
	using DotNetOAuth.Messaging;

	internal class TestDirectedMessage : TestMessage, IOAuthDirectedMessage {
		internal TestDirectedMessage() {
		}

		internal TestDirectedMessage(MessageTransport transport) : base(transport) {
		}

		#region IDirectedProtocolMessage Members

		public Uri Recipient { get; set; }

		#endregion

		#region IProtocolMessage Properties

		MessageProtection IProtocolMessage.RequiredProtection {
			get { return this.RequiredProtection; }
		}

		#endregion

		#region IOAuthDirectedMessage Members

		public HttpDeliveryMethod HttpMethods { get; internal set; }

		#endregion

		protected virtual MessageProtection RequiredProtection {
			get { return MessageProtection.None; }
		}
	}
}
