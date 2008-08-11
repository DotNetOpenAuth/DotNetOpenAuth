using System.Runtime.Serialization;

namespace DotNetOAuth.Test.Mocks {
	[DataContract(Namespace = Protocol.DataContractNamespace)]
	class TestMessage : IProtocolMessage {
		[DataMember(Name = "age")]
		public int Age { get; set; }
		[DataMember]
		public string Name { get; set; }
		[DataMember]
		public string EmptyMember { get; set; }
	}
}
