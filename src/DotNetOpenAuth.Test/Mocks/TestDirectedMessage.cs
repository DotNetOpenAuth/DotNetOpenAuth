//-----------------------------------------------------------------------
// <copyright file="TestDirectedMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth.ChannelElements;

	internal class TestDirectedMessage : TestMessage, IDirectedProtocolMessage {
		internal TestDirectedMessage() {
			this.HttpMethods = HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.PostRequest;
		}

		internal TestDirectedMessage(MessageTransport transport) : base(transport) {
			this.HttpMethods = HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.PostRequest;
		}

		#region IDirectedProtocolMessage Members

		public Uri Recipient { get; set; }

		public HttpDeliveryMethods HttpMethods { get; internal set; }

		#endregion

		#region IProtocolMessage Properties

		MessageProtections IProtocolMessage.RequiredProtection {
			get { return this.RequiredProtection; }
		}

		#endregion

		protected virtual MessageProtections RequiredProtection {
			get { return MessageProtections.None; }
		}
	}
}
