//-----------------------------------------------------------------------
// <copyright file="TestMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Mocks {
	using System;
	using System.Runtime.Serialization;
	using DotNetOAuth.Messaging;

	[DataContract(Namespace = Protocol.DataContractNamespaceV10)]
	internal class TestMessage : IProtocolMessage {
		private MessageTransport transport;

		internal TestMessage() : this(MessageTransport.Direct) {
		}

		internal TestMessage(MessageTransport transport) {
			this.transport = transport;
		}

		[DataMember(Name = "age", IsRequired = true)]
		public int Age { get; set; }
		[DataMember]
		public string Name { get; set; }
		[DataMember]
		public string EmptyMember { get; set; }
		[DataMember]
		public Uri Location { get; set; }
		[DataMember]
		public DateTime Timestamp { get; set; }

		#region IProtocolMessage Members

		Protocol IProtocolMessage.Protocol {
			get { return Protocol.V10; }
		}

		MessageTransport IProtocolMessage.Transport {
			get { return this.transport; }
		}

		void IProtocolMessage.EnsureValidMessage() {
			if (this.EmptyMember != null || this.Age < 0) {
				throw new ProtocolException();
			}
		}

		#endregion
	}
}
