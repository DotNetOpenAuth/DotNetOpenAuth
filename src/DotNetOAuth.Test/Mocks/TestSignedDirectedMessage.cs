//-----------------------------------------------------------------------
// <copyright file="TestSignedDirectedMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Mocks {
	using System.Runtime.Serialization;
	using DotNetOAuth.Messaging;
	using DotNetOAuth.Messaging.Bindings;

	[DataContract(Namespace = Protocol.DataContractNamespaceV10)]
	internal class TestSignedDirectedMessage : TestDirectedMessage, ITamperResistantProtocolMessage {
		internal TestSignedDirectedMessage(MessageTransport transport)
			: base(transport) {
		}

		#region ISignedProtocolMessage Members

		[DataMember]
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
