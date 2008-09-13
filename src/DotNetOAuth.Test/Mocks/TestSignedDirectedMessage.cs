//-----------------------------------------------------------------------
// <copyright file="TestSignedDirectedMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Mocks {
	using System.Runtime.Serialization;
	using DotNetOAuth.Messaging;

	[DataContract(Namespace = Protocol.DataContractNamespaceV10)]
	internal class TestSignedDirectedMessage : TestDirectedMessage, ISignedProtocolMessage {
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
	}
}
