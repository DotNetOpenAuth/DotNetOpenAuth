//-----------------------------------------------------------------------
// <copyright file="TestExpiringMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Mocks {
	using System;
	using System.Diagnostics;
	using System.Runtime.Serialization;
	using DotNetOAuth.Messaging;
	using DotNetOAuth.Messaging.Bindings;
	using DotNetOAuth.Messaging.Reflection;

	[DataContract(Namespace = Protocol.DataContractNamespaceV10)]
	internal class TestExpiringMessage : TestSignedDirectedMessage, IExpiringProtocolMessage {
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private DateTime utcCreationDate;

		internal TestExpiringMessage() { }

		internal TestExpiringMessage(MessageTransport transport)
			: base(transport) {
		}

		#region IExpiringProtocolMessage Members

		[MessagePart(Name = "created_on")]
		DateTime IExpiringProtocolMessage.UtcCreationDate {
			get { return this.utcCreationDate; }
			set { this.utcCreationDate = value.ToUniversalTime(); }
		}

		#endregion
	}
}
