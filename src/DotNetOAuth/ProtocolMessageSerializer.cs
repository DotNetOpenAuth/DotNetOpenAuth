using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace DotNetOAuth {
	/// <summary>
	/// Serializes/deserializes OAuth messages for/from transit.
	/// </summary>
	internal class ProtocolMessageSerializer<T> where T : IProtocolMessage {
		DataContractSerializer serializer;
		const string rootName = "root";

		internal ProtocolMessageSerializer() {
			serializer = new DataContractSerializer(typeof(T), rootName, Protocol.DataContractNamespace);
		}

		/// <summary>
		/// Reads the data from a message instance and returns a series of name=value pairs for the fields that must be included in the message.
		/// </summary>
		/// <param name="message">The message to be serialized.</param>
		internal IDictionary<string, string> Serialize(T message) {
			if (message == null) throw new ArgumentNullException("message");

			var fields = new Dictionary<string, string>();
			XmlWriter writer = new ProtocolMessageSerializer<T>.PseudoXmlWriter(fields);
			serializer.WriteObjectContent(writer, message);
			writer.Close();
			return fields;
		}

		/// <summary>
		/// Reads name=value pairs into an OAuth message.
		/// </summary>
		/// <param name="fields">The name=value pairs that were read in from the transport.</param>
		internal T Deserialize(IDictionary<string, string> fields) {
			if (fields == null) throw new ArgumentNullException("fields");

			var reader = new PseudoXmlReader(fields);
			//var reader = new XmlTextReader(new StringReader("<root xmlns='http://oauth.net/core/1.0/'><Name>Andrew</Name><age>15</age></root>"));
			T result = (T)serializer.ReadObject(reader, false);
			return result;
		}

		/// <summary>
		/// Writes out a dictionary as if it were XML.
		/// </summary>
		class PseudoXmlWriter : XmlWriter {
			Dictionary<string, string> dictionary;
			string key;
			StringBuilder value = new StringBuilder();

			internal PseudoXmlWriter(Dictionary<string, string> dictionary) {
				this.dictionary = dictionary;
			}

			public override void WriteStartElement(string prefix, string localName, string ns) {
				key = localName;
				value.Length = 0;
			}

			public override void WriteString(string text) {
				if (!string.IsNullOrEmpty(key)) {
					value.Append(text);
				}
			}

			public override void WriteEndElement() {
				if (key != null) {
					dictionary[key] = value.ToString();
					key = null;
					value.Length = 0;
				}
			}

			public override WriteState WriteState {
				get { return WriteState.Element; }
			}

			public override void WriteStartAttribute(string prefix, string localName, string ns) {
				key = null;
			}

			public override void WriteEndAttribute() { }

			public override void Close() { }

			#region Unimplemented methods

			public override void Flush() {
				throw new NotImplementedException();
			}

			public override string LookupPrefix(string ns) {
				throw new NotImplementedException();
			}

			public override void WriteBase64(byte[] buffer, int index, int count) {
				throw new NotImplementedException();
			}

			public override void WriteCData(string text) {
				throw new NotImplementedException();
			}

			public override void WriteCharEntity(char ch) {
				throw new NotImplementedException();
			}

			public override void WriteChars(char[] buffer, int index, int count) {
				throw new NotImplementedException();
			}

			public override void WriteComment(string text) {
				throw new NotImplementedException();
			}

			public override void WriteDocType(string name, string pubid, string sysid, string subset) {
				throw new NotImplementedException();
			}

			public override void WriteEndDocument() {
				throw new NotImplementedException();
			}

			public override void WriteEntityRef(string name) {
				throw new NotImplementedException();
			}

			public override void WriteFullEndElement() {
				throw new NotImplementedException();
			}

			public override void WriteProcessingInstruction(string name, string text) {
				throw new NotImplementedException();
			}

			public override void WriteRaw(string data) {
				throw new NotImplementedException();
			}

			public override void WriteRaw(char[] buffer, int index, int count) {
				throw new NotImplementedException();
			}

			public override void WriteStartDocument(bool standalone) {
				throw new NotImplementedException();
			}

			public override void WriteStartDocument() {
				throw new NotImplementedException();
			}

			public override void WriteSurrogateCharEntity(char lowChar, char highChar) {
				throw new NotImplementedException();
			}

			public override void WriteWhitespace(string ws) {
				throw new NotImplementedException();
			}

			#endregion
		}

		/// <summary>
		/// Reads in a dictionary as if it were XML.
		/// </summary>
		class PseudoXmlReader : XmlReader {
			IDictionary<string, string> dictionary;
			IEnumerator<string> keyEnumerator;
			bool isFinished;
			int depth;
			XmlNodeType currentNode = XmlNodeType.None;

			internal PseudoXmlReader(IDictionary<string, string> dictionary) {
				this.dictionary = dictionary;
				keyEnumerator = dictionary.Keys.OrderBy(key => key, StringComparer.Ordinal).GetEnumerator();
			}

			public override int AttributeCount {
				get { return 0; }
			}

			public override bool IsEmptyElement {
				get {
					if (keyEnumerator.Current == null) {
						return isFinished;
					}
					string value;
					bool result = !dictionary.TryGetValue(keyEnumerator.Current, out value) || String.IsNullOrEmpty(value);
					return result;
				}
			}

			public override string LocalName {
				get { return keyEnumerator.Current; }
			}

			public override bool MoveToElement() {
				bool result = currentNode != XmlNodeType.Element && depth > 0;
				currentNode = depth > 0 ? XmlNodeType.Element : XmlNodeType.None;
				return result;
			}

			public override bool MoveToNextAttribute() {
				if (depth == 1 && currentNode != XmlNodeType.Attribute) {
					currentNode = XmlNodeType.Attribute;
					return true;
				} else {
					return false;
				}
			}

			public override string NamespaceURI {
				get {
					string result = depth == 1 && currentNode == XmlNodeType.Attribute ?
						"http://www.w3.org/2000/xmlns/" : Protocol.DataContractNamespace;
					return result;
				}
			}

			public override XmlNodeType NodeType {
				get { return currentNode; }
			}

			public override bool Read() {
				if (isFinished) {
					if (depth > 0) {
						depth--;
					}
					return depth > 0;
				}
				switch (depth) {
					case 0: // moving to root node
						depth++; // -> 1
						currentNode = XmlNodeType.Element;
						return true;
					case 1: // moving to first content node
						depth++; // -> 2
						isFinished = !keyEnumerator.MoveNext();
						currentNode = isFinished ? XmlNodeType.EndElement : XmlNodeType.Element;
						return true;
					case 2: // content node
						switch (currentNode) {
							case XmlNodeType.Element:
								currentNode = XmlNodeType.Text;
								return true;
							case XmlNodeType.Text:
								currentNode = XmlNodeType.EndElement;
								return true;
							case XmlNodeType.EndElement:
								bool result = keyEnumerator.MoveNext();
								if (!result) {
									isFinished = true;
									depth--;
									currentNode = XmlNodeType.EndElement;
								} else {
									currentNode = XmlNodeType.Element;
								}
								return true;
						}
						break;
				}
				throw new InvalidOperationException();
			}

			public override string Value {
				get { return dictionary[keyEnumerator.Current]; }
			}

			#region Unimplemented methods

			public override string BaseURI {
				get { throw new NotImplementedException(); }
			}

			public override void Close() {
				throw new NotImplementedException();
			}

			public override int Depth {
				get { throw new NotImplementedException(); }
			}

			public override bool EOF {
				get { throw new NotImplementedException(); }
			}

			public override string GetAttribute(int i) {
				throw new NotImplementedException();
			}

			public override string GetAttribute(string name, string namespaceURI) {
				throw new NotImplementedException();
			}

			public override string GetAttribute(string name) {
				throw new NotImplementedException();
			}

			public override bool HasValue {
				get { throw new NotImplementedException(); }
			}

			public override string LookupNamespace(string prefix) {
				throw new NotImplementedException();
			}

			public override bool MoveToAttribute(string name, string ns) {
				throw new NotImplementedException();
			}

			public override bool MoveToAttribute(string name) {
				throw new NotImplementedException();
			}

			public override bool MoveToFirstAttribute() {
				throw new NotImplementedException();
			}

			public override XmlNameTable NameTable {
				get { throw new NotImplementedException(); }
			}

			public override string Prefix {
				get { throw new NotImplementedException(); }
			}

			public override ReadState ReadState {
				get { throw new NotImplementedException(); }
			}

			public override bool ReadAttributeValue() {
				throw new NotImplementedException();
			}

			public override void ResolveEntity() {
				throw new NotImplementedException();
			}

			#endregion
		}
	}
}
