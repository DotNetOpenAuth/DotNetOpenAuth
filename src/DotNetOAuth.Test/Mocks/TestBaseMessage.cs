//-----------------------------------------------------------------------
// <copyright file="TestBaseMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Mocks {
	using System;
	using System.Runtime.Serialization;
	using DotNetOAuth.Messaging;

	internal interface IBaseMessageExplicitMembers {
		string ExplicitProperty { get; set; }
	}

	[DataContract(Namespace = Protocol.DataContractNamespaceV10)]
	internal class TestBaseMessage : IProtocolMessage, IBaseMessageExplicitMembers {
		[DataMember(Name = "age", IsRequired = true)]
		public int Age { get; set; }

		[DataMember]
		public string Name { get; set; }

		[DataMember(Name = "explicit")]
		string IBaseMessageExplicitMembers.ExplicitProperty { get; set; }

		Version IProtocolMessage.ProtocolVersion {
			get { return new Version(1, 0); }
		}

		MessageProtection IProtocolMessage.RequiredProtection {
			get { return MessageProtection.None; }
		}

		MessageTransport IProtocolMessage.Transport {
			get { return MessageTransport.Indirect; }
		}

		internal string PrivatePropertyAccessor {
			get { return this.PrivateProperty; }
			set { this.PrivateProperty = value; }
		}

		[DataMember(Name = "private")]
		private string PrivateProperty { get; set; }

		void IProtocolMessage.EnsureValidMessage() { }
	}
}
