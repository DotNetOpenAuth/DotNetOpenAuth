//-----------------------------------------------------------------------
// <copyright file="TestDirectedMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using DotNetOAuth.Messaging;

	[DataContract(Namespace = Protocol.DataContractNamespaceV10)]
	internal class TestDirectedMessage : IDirectedProtocolMessage {
		private MessageTransport transport;

		private Dictionary<string, string> extraData = new Dictionary<string, string>();

		internal TestDirectedMessage(MessageTransport transport) {
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

		#region IDirectedProtocolMessage Members

		public Uri Recipient { get; internal set; }

		#endregion

		#region IProtocolMessage Properties

		Version IProtocolMessage.ProtocolVersion {
			get { return new Version(1, 0); }
		}

		MessageProtection IProtocolMessage.RequiredProtection {
			get { return this.RequiredProtection; }
		}

		MessageTransport IProtocolMessage.Transport {
			get { return this.transport; }
		}

		IDictionary<string, string> IProtocolMessage.ExtraData {
			get { return this.extraData; }
		}

		#endregion

		protected virtual MessageProtection RequiredProtection {
			get { return MessageProtection.None; }
		}

		#region IProtocolMessage Methods

		void IProtocolMessage.EnsureValidMessage() {
			if (this.EmptyMember != null || this.Age < 0) {
				throw new ProtocolException();
			}
		}

		#endregion
	}
}
