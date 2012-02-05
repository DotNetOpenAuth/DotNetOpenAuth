//-----------------------------------------------------------------------
// <copyright file="TestExpiringMessage.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Diagnostics;
	using System.Runtime.Serialization;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.Messaging.Reflection;

	internal class TestExpiringMessage : TestSignedDirectedMessage, IExpiringProtocolMessage {
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private DateTime utcCreationDate;

		internal TestExpiringMessage() { }

		internal TestExpiringMessage(MessageTransport transport)
			: base(transport) {
		}

		#region IExpiringProtocolMessage Members

		[MessagePart("created_on", IsRequired = true)]
		DateTime IExpiringProtocolMessage.UtcCreationDate {
			get { return this.utcCreationDate; }
			set { this.utcCreationDate = value.ToUniversalTime(); }
		}

		#endregion
	}
}
