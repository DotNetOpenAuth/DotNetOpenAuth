//-----------------------------------------------------------------------
// <copyright file="TestSignedDirectedMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Mocks {
	using DotNetOAuth.Messaging;
	using DotNetOAuth.Messaging.Bindings;
	using DotNetOAuth.Messaging.Reflection;

	internal class TestSignedDirectedMessage : TestDirectedMessage, ITamperResistantProtocolMessage {
		internal TestSignedDirectedMessage() { }

		internal TestSignedDirectedMessage(MessageTransport transport)
			: base(transport) {
		}

		#region ISignedProtocolMessage Members

		[MessagePart]
		public string Signature {
			get;
			set;
		}

		#endregion

		protected override MessageProtection RequiredProtection {
			get { return MessageProtection.TamperProtection; }
		}
	}
}
