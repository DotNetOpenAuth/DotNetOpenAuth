//-----------------------------------------------------------------------
// <copyright file="TestSignedDirectedMessage.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.Messaging.Reflection;

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

		protected override MessageProtections RequiredProtection {
			get { return MessageProtections.TamperProtection; }
		}
	}
}
