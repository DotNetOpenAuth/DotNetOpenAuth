//-----------------------------------------------------------------------
// <copyright file="TestDirectedMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using DotNetOAuth.Messaging;
	using DotNetOAuth.Messaging.Reflection;

	[DataContract(Namespace = Protocol.DataContractNamespaceV10)]
	internal class TestDirectedMessage : TestMessage, IDirectedProtocolMessage {
		internal TestDirectedMessage() {
		}

		internal TestDirectedMessage(MessageTransport transport) : base(transport) {
		}

		#region IDirectedProtocolMessage Members

		public Uri Recipient { get; internal set; }

		#endregion

		#region IProtocolMessage Properties

		MessageProtection IProtocolMessage.RequiredProtection {
			get { return this.RequiredProtection; }
		}

		#endregion

		protected virtual MessageProtection RequiredProtection {
			get { return MessageProtection.None; }
		}
	}
}
