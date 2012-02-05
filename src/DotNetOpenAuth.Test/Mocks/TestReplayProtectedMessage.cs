//-----------------------------------------------------------------------
// <copyright file="TestReplayProtectedMessage.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System.Runtime.Serialization;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.Messaging.Reflection;

	internal class TestReplayProtectedMessage : TestExpiringMessage, IReplayProtectedProtocolMessage {
		internal TestReplayProtectedMessage() { }

		internal TestReplayProtectedMessage(MessageTransport transport)
			: base(transport) {
		}

		#region IReplayProtectedProtocolMessage Members

		string IReplayProtectedProtocolMessage.NonceContext {
			get { return string.Empty; }
		}

		[MessagePart("Nonce")]
		string IReplayProtectedProtocolMessage.Nonce {
			get;
			set;
		}

		#endregion
	}
}
