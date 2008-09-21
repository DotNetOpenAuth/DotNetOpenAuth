//-----------------------------------------------------------------------
// <copyright file="TestSignedDirectedMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Mocks {
	using System.Runtime.Serialization;
	using DotNetOAuth.Messaging;
	using DotNetOAuth.Messaging.Reflection;
	using DotNetOAuth.Messaging.Bindings;

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
