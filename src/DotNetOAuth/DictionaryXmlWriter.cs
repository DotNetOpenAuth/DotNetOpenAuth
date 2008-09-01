//-----------------------------------------------------------------------
// <copyright file="DictionaryXmlWriter.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth {
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Xml;

	/// <summary>
	/// An XmlWriter-looking object that actually saves data to a dictionary.
	/// </summary>
	internal class DictionaryXmlWriter {
		/// <summary>
		/// Creates an <see cref="XmlWriter"/> that actually writes to an IDictionary&lt;string, string&gt; instance.
		/// </summary>
		/// <param name="dictionary">The dictionary to save the written XML to.</param>
		/// <returns>The XmlWriter that will save data to the given dictionary.</returns>
		internal static XmlWriter Create(IDictionary<string, string> dictionary) {
			return new PseudoXmlWriter(dictionary);
		}

		/// <summary>
		/// Writes out a dictionary as if it were XML.
		/// </summary>
		private class PseudoXmlWriter : XmlWriter {
			/// <summary>
			/// The dictionary to write values to.
			/// </summary>
			private IDictionary<string, string> dictionary;

			/// <summary>
			/// The key being written at the moment.
			/// </summary>
			private string key;

			/// <summary>
			/// The value being written out at the moment.
			/// </summary>
			private StringBuilder value = new StringBuilder();

			/// <summary>
			/// Initializes a new instance of the <see cref="PseudoXmlWriter"/> class.
			/// </summary>
			/// <param name="dictionary">The dictionary that will be written to.</param>
			internal PseudoXmlWriter(IDictionary<string, string> dictionary) {
				if (dictionary == null) {
					throw new ArgumentNullException("dictionary");
				}

				this.dictionary = dictionary;
			}

			/// <summary>
			/// Gets the spoofed state of the <see cref="XmlWriter"/>.
			/// </summary>
			public override WriteState WriteState {
				get { return WriteState.Element; }
			}

			/// <summary>
			/// Prepares to write out a new key/value pair with the given key name to the dictionary.
			/// </summary>
			/// <param name="prefix">This parameter is ignored.</param>
			/// <param name="localName">The key to store in the dictionary.</param>
			/// <param name="ns">This parameter is ignored.</param>
			public override void WriteStartElement(string prefix, string localName, string ns) {
				this.key = localName;
				this.value.Length = 0;
			}

			/// <summary>
			/// Appends some text to the value that is to be stored in the dictionary.
			/// </summary>
			/// <param name="text">The text to append to the value.</param>
			public override void WriteString(string text) {
				if (!string.IsNullOrEmpty(this.key)) {
					this.value.Append(text);
				}
			}

			/// <summary>
			/// Writes out a completed key/value to the dictionary.
			/// </summary>
			public override void WriteEndElement() {
				if (this.key != null) {
					this.dictionary[this.key] = this.value.ToString();
					this.key = null;
					this.value.Length = 0;
				}
			}

			/// <summary>
			/// Clears the internal key/value building state.
			/// </summary>
			/// <param name="prefix">This parameter is ignored.</param>
			/// <param name="localName">This parameter is ignored.</param>
			/// <param name="ns">This parameter is ignored.</param>
			public override void WriteStartAttribute(string prefix, string localName, string ns) {
				this.key = null;
			}

			/// <summary>
			/// This method does not do anything.
			/// </summary>
			public override void WriteEndAttribute() { }

			/// <summary>
			/// This method does not do anything.
			/// </summary>
			public override void Close() { }

			#region Unimplemented methods

			/// <summary>
			/// Throws <see cref="NotImplementedException"/>.
			/// </summary>
			public override void Flush() {
				throw new NotImplementedException();
			}

			/// <summary>
			/// Throws <see cref="NotImplementedException"/>.
			/// </summary>
			/// <param name="ns">This parameter is ignored.</param>
			/// <returns>None, since an exception is always thrown.</returns>
			public override string LookupPrefix(string ns) {
				throw new NotImplementedException();
			}

			/// <summary>
			/// Throws <see cref="NotImplementedException"/>.
			/// </summary>
			/// <param name="buffer">This parameter is ignored.</param>
			/// <param name="index">This parameter is ignored.</param>
			/// <param name="count">This parameter is ignored.</param>
			public override void WriteBase64(byte[] buffer, int index, int count) {
				throw new NotImplementedException();
			}

			/// <summary>
			/// Throws <see cref="NotImplementedException"/>.
			/// </summary>
			/// <param name="text">This parameter is ignored.</param>
			public override void WriteCData(string text) {
				throw new NotImplementedException();
			}

			/// <summary>
			/// Throws <see cref="NotImplementedException"/>.
			/// </summary>
			/// <param name="ch">This parameter is ignored.</param>
			public override void WriteCharEntity(char ch) {
				throw new NotImplementedException();
			}

			/// <summary>
			/// Throws <see cref="NotImplementedException"/>.
			/// </summary>
			/// <param name="buffer">This parameter is ignored.</param>
			/// <param name="index">This parameter is ignored.</param>
			/// <param name="count">This parameter is ignored.</param>
			public override void WriteChars(char[] buffer, int index, int count) {
				throw new NotImplementedException();
			}

			/// <summary>
			/// Throws <see cref="NotImplementedException"/>.
			/// </summary>
			/// <param name="text">This parameter is ignored.</param>
			public override void WriteComment(string text) {
				throw new NotImplementedException();
			}

			/// <summary>
			/// Throws <see cref="NotImplementedException"/>.
			/// </summary>
			/// <param name="name">This parameter is ignored.</param>
			/// <param name="pubid">This parameter is ignored.</param>
			/// <param name="sysid">This parameter is ignored.</param>
			/// <param name="subset">This parameter is ignored.</param>
			public override void WriteDocType(string name, string pubid, string sysid, string subset) {
				throw new NotImplementedException();
			}

			/// <summary>
			/// Throws <see cref="NotImplementedException"/>.
			/// </summary>
			public override void WriteEndDocument() {
				throw new NotImplementedException();
			}

			/// <summary>
			/// Throws <see cref="NotImplementedException"/>.
			/// </summary>
			/// <param name="name">This parameter is ignored.</param>
			public override void WriteEntityRef(string name) {
				throw new NotImplementedException();
			}

			/// <summary>
			/// Throws <see cref="NotImplementedException"/>.
			/// </summary>
			public override void WriteFullEndElement() {
				throw new NotImplementedException();
			}

			/// <summary>
			/// Throws <see cref="NotImplementedException"/>.
			/// </summary>
			/// <param name="name">This parameter is ignored.</param>
			/// <param name="text">This parameter is ignored.</param>
			public override void WriteProcessingInstruction(string name, string text) {
				throw new NotImplementedException();
			}

			/// <summary>
			/// Throws <see cref="NotImplementedException"/>.
			/// </summary>
			/// <param name="data">This parameter is ignored.</param>
			public override void WriteRaw(string data) {
				throw new NotImplementedException();
			}

			/// <summary>
			/// Throws <see cref="NotImplementedException"/>.
			/// </summary>
			/// <param name="buffer">This parameter is ignored.</param>
			/// <param name="index">This parameter is ignored.</param>
			/// <param name="count">This parameter is ignored.</param>
			public override void WriteRaw(char[] buffer, int index, int count) {
				throw new NotImplementedException();
			}

			/// <summary>
			/// Throws <see cref="NotImplementedException"/>.
			/// </summary>
			/// <param name="standalone">This parameter is ignored.</param>
			public override void WriteStartDocument(bool standalone) {
				throw new NotImplementedException();
			}

			/// <summary>
			/// Throws <see cref="NotImplementedException"/>.
			/// </summary>
			public override void WriteStartDocument() {
				throw new NotImplementedException();
			}

			/// <summary>
			/// Throws <see cref="NotImplementedException"/>.
			/// </summary>
			/// <param name="lowChar">This parameter is ignored.</param>
			/// <param name="highChar">This parameter is ignored.</param>
			public override void WriteSurrogateCharEntity(char lowChar, char highChar) {
				throw new NotImplementedException();
			}

			/// <summary>
			/// Throws <see cref="NotImplementedException"/>.
			/// </summary>
			/// <param name="ws">This parameter is ignored.</param>
			public override void WriteWhitespace(string ws) {
				throw new NotImplementedException();
			}

			#endregion
		}
	}
}
