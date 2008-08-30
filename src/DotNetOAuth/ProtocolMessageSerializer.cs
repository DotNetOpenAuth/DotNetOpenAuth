using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace DotNetOAuth {
	/// <summary>
	/// Serializes/deserializes OAuth messages for/from transit.
	/// </summary>
	internal class ProtocolMessageSerializer<T> where T : IProtocolMessage {
		DataContractSerializer serializer;
		readonly XName rootElement = XName.Get("root", Protocol.DataContractNamespace);

		internal ProtocolMessageSerializer() {
			serializer = new DataContractSerializer(typeof(T), rootElement.LocalName, rootElement.NamespaceName);
		}

		/// <summary>
		/// Reads the data from a message instance and returns a series of name=value pairs for the fields that must be included in the message.
		/// </summary>
		/// <param name="message">The message to be serialized.</param>
		internal IDictionary<string, string> Serialize(T message) {
			if (message == null) throw new ArgumentNullException("message");

			var fields = new Dictionary<string, string>();
			Serialize(fields, message);
			return fields;
		}

		internal void Serialize(IDictionary<string, string> fields, T message) {
			if (fields == null) throw new ArgumentNullException("fields");
			if (message == null) throw new ArgumentNullException("message");

			message.EnsureValidMessage();
			using (XmlWriter writer = DictionaryXmlWriter.Create(fields)) {
				serializer.WriteObjectContent(writer, message);
			}
		}

		/// <summary>
		/// Reads name=value pairs into an OAuth message.
		/// </summary>
		/// <param name="fields">The name=value pairs that were read in from the transport.</param>
		internal T Deserialize(IDictionary<string, string> fields) {
			if (fields == null) throw new ArgumentNullException("fields");

			var reader = DictionaryXmlReader.Create(rootElement, fields);
			T result = (T)serializer.ReadObject(reader, false);
			result.EnsureValidMessage();
			return result;
		}
	}
}
