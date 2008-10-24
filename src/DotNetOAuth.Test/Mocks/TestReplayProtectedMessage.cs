//-----------------------------------------------------------------------
// <copyright file="TestReplayProtectedMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Mocks {
	using System.Runtime.Serialization;
	using DotNetOAuth.Messaging;
	using DotNetOAuth.Messaging.Bindings;
	using DotNetOAuth.Messaging.Reflection;

	internal class TestReplayProtectedMessage : TestExpiringMessage, IReplayProtectedProtocolMessage {
		internal TestReplayProtectedMessage() { }

		internal TestReplayProtectedMessage(MessageTransport transport)
			: base(transport) {
		}

		#region IReplayProtectedProtocolMessage Members

		[MessagePart("Nonce")]
		string IReplayProtectedProtocolMessage.Nonce {
			get;
			set;
		}

		#endregion
	}
}
