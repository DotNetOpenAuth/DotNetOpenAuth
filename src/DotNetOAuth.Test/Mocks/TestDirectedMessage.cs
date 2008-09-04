//-----------------------------------------------------------------------
// <copyright file="TestDirectedMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Mocks {
	using System;
	using System.Runtime.Serialization;
	using DotNetOAuth.Messaging;

	[DataContract(Namespace = Protocol.DataContractNamespaceV10)]
	internal class TestDirectedMessage : IDirectedProtocolMessage {
		[DataMember(Name = "age", IsRequired = true)]
		public int Age { get; set; }
		[DataMember]
		public string Name { get; set; }
		[DataMember]
		public string EmptyMember { get; set; }
		[DataMember]
		public Uri Location { get; set; }

		#region IDirectedProtocolMessage Members

		public Uri Recipient { get; internal set; }

		#endregion

		#region IProtocolMessage Members

		Protocol IProtocolMessage.Protocol {
			get { return Protocol.V10; }
		}

		MessageTransport IProtocolMessage.Transport {
			get { return MessageTransport.Direct; }
		}

		void IProtocolMessage.EnsureValidMessage() {
			if (this.EmptyMember != null || this.Age < 0) {
				throw new ProtocolException();
			}
		}

		#endregion
	}
}
