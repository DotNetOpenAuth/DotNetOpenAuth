//-----------------------------------------------------------------------
// <copyright file="TestMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using DotNetOAuth.Messaging;

	[DataContract(Namespace = Protocol.DataContractNamespaceV10)]
	internal class TestMessage : IProtocolMessage {
		private MessageTransport transport;
		private Dictionary<string, string> extraData = new Dictionary<string, string>();

		internal TestMessage()
			: this(MessageTransport.Direct) {
		}

		internal TestMessage(MessageTransport transport) {
			this.transport = transport;
		}

		[DataMember(Name = "age", IsRequired = true)]
		[MessagePart("age")]
		public int Age { get; set; }
		[DataMember]
		[MessagePart(Optional = true)]
		public string Name { get; set; }
		[DataMember]
		[MessagePart(Optional = true)]
		public string EmptyMember { get; set; }
		[DataMember]
		[MessagePart(Optional = true)]
		public Uri Location { get; set; }
		[DataMember]
		[MessagePart(Optional = true)]
		public DateTime Timestamp { get; set; }

		#region IProtocolMessage Members

		Version IProtocolMessage.ProtocolVersion {
			get { return new Version(1, 0); }
		}

		MessageProtection IProtocolMessage.RequiredProtection {
			get { return MessageProtection.None; }
		}

		MessageTransport IProtocolMessage.Transport {
			get { return this.transport; }
		}

		IDictionary<string, string> IProtocolMessage.ExtraData {
			get { return this.extraData; }
		}

		void IProtocolMessage.EnsureValidMessage() {
			if (this.EmptyMember != null || this.Age < 0) {
				throw new ProtocolException();
			}
		}

		#endregion
	}
}
