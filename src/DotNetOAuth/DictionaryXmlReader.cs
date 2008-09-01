namespace DotNetOAuth {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Xml;
	using System.Xml.Linq;

	/// <summary>
	/// An XmlReader-looking object that actually reads from a dictionary.
	/// </summary>
	class DictionaryXmlReader {
		/// <summary>
		/// Creates an XmlReader that reads data out of a dictionary instead of XML.
		/// </summary>
		/// <param name="rootElement">The name of the root XML element.</param>
		/// <param name="fields">The dictionary to read data from.</param>
		/// <returns>The XmlReader that will read the data out of the given dictionary.</returns>
		internal static XmlReader Create(XName rootElement, IDictionary<string, string> fields) {
			if (fields == null) {
				throw new ArgumentNullException("fields");
			}

			return CreateRoundtripReader(rootElement, fields);
			// The pseudo reader MAY be more efficient, but it's buggy.
			//return CreatePseudoReader(rootElement, fields);
		}

		private static XmlReader CreateRoundtripReader(XName rootElement, IDictionary<string, string> fields) {
			MemoryStream stream = new MemoryStream();
			XmlWriter writer = XmlWriter.Create(stream);
			SerializeDictionaryToXml(writer, rootElement, fields);
			writer.Flush();
			stream.Seek(0, SeekOrigin.Begin);

			// For debugging purposes.
			StreamReader sr = new StreamReader(stream);
			Trace.WriteLine(sr.ReadToEnd());
			stream.Seek(0, SeekOrigin.Begin);

			return XmlReader.Create(stream);
		}

		static XmlReader CreatePseudoReader(XName rootElement, IDictionary<string, string> fields) {
			return new PseudoXmlReader(fields);
		}

		static void SerializeDictionaryToXml(XmlWriter writer, XName rootElement, IDictionary<string, string> fields) {
			if (writer == null) {
				throw new ArgumentNullException("writer");
			}
			if (fields == null) {
				throw new ArgumentNullException("fields");
			}

			writer.WriteStartElement(rootElement.LocalName, rootElement.NamespaceName);
			// The elements must be serialized in alphabetical order so the DataContractSerializer will see them.
			foreach (var pair in fields.OrderBy(pair => pair.Key, StringComparer.Ordinal)) {
				writer.WriteStartElement(pair.Key, rootElement.NamespaceName);
				writer.WriteValue(pair.Value);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
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
				Trace.WriteLine("MoveToElement()");
				bool result = currentNode != XmlNodeType.Element && depth > 0;
				currentNode = depth > 0 ? XmlNodeType.Element : XmlNodeType.None;
				return result;
			}

			public override bool MoveToNextAttribute() {
				Trace.WriteLine("MoveToNextAttribute()");
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
				Trace.WriteLine("Read()");
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
				get {
					Trace.WriteLine("Depth: " + (depth - 1));
					return depth - 1;
				}
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
				get {
					Trace.WriteLine("ReadState");
					return ReadState.Interactive;
				}
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
