using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace DotNetOAuth {
	/// <summary>
	/// An XmlWriter-looking object that actually saves data to a dictionary.
	/// </summary>
	class DictionaryXmlWriter {
		/// <summary>
		/// Creates an <see cref="XmlWriter"/> that actually writes to an <see cref="IDictionary<string, string>"/> instance.
		/// </summary>
		/// <param name="dictionary">The dictionary to save the written XML to.</param>
		/// <returns>The XmlWriter that will save data to the given dictionary.</returns>
		internal static XmlWriter Create(IDictionary<string, string> dictionary) {
			return new PseudoXmlWriter(dictionary);
		}

		/// <summary>
		/// Writes out a dictionary as if it were XML.
		/// </summary>
		class PseudoXmlWriter : XmlWriter {
			IDictionary<string, string> dictionary;
			string key;
			StringBuilder value = new StringBuilder();

			internal PseudoXmlWriter(IDictionary<string, string> dictionary) {
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
	}
}
