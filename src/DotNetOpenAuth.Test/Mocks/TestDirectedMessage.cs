//-----------------------------------------------------------------------
// <copyright file="TestDirectedMessage.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth.ChannelElements;

	internal class TestDirectedMessage : TestMessage, IDirectedProtocolMessage {
		internal TestDirectedMessage() {
			this.HttpMethods = HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.PostRequest | HttpDeliveryMethods.HeadRequest;
		}

		internal TestDirectedMessage(MessageTransport transport) : base(transport) {
			this.HttpMethods = HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.PostRequest | HttpDeliveryMethods.HeadRequest;
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
