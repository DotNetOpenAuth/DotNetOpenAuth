using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Globalization;

namespace DotNetOpenId {
	public enum KeyValueFormConformanceLevel {
		Loose,
		OpenID11,
		OpenID20,
	}

	/// <summary>
	/// Conversion to and from the Key-Value Form Encoding defined by
	/// OpenID Authentication 2.0 section 4.1.1.
	/// http://openid.net/specs/openid-authentication-2_0.html#anchor4
	/// </summary>
	internal class KeyValueFormEncoding : IProtocolMessageEncoding {
		static readonly char[] illegalKeyCharacters = { '\n', ':' };
		static readonly char[] illegalValueCharacters = { '\n' };
		const string newLineCharacters = "\n";
		static readonly Encoding textEncoding = Encoding.UTF8;

		public KeyValueFormEncoding() {
			ConformanceLevel = KeyValueFormConformanceLevel.Loose;
		}
		public KeyValueFormEncoding(KeyValueFormConformanceLevel conformanceLevel) {
			ConformanceLevel = conformanceLevel;
		}
		public KeyValueFormConformanceLevel ConformanceLevel { get; private set; }

		/// <summary>
		/// Encodes key/value pairs to Key-Value Form.
		/// Do not use for dictionaries of signed fields!  Instead use the overload
		/// that accepts a list of in-order keys.
		/// </summary>
		/// <returns>The UTF8 byte array.</returns>
		/// <remarks>
		/// Because dictionaries do not guarantee ordering,
		/// encoding a dictionary without an explicitly given key order
		/// is useless in OpenID scenarios where a signature must match.
		/// </remarks>
		public byte[] GetBytes(IDictionary<string, string> dictionary) {
			string[] keys = new string[dictionary.Count];
			dictionary.Keys.CopyTo(keys, 0);
			return GetBytes(dictionary, keys);
		}

		/// <summary>
		/// Encodes key/value pairs to Key-Value Form.
		/// </summary>
		/// <param name="keyOrder">
		/// The order in which to encode the key/value pairs.
		/// Useful in scenarios where a byte[] must be exactly reproduced.
		/// </param>
		/// <returns>The UTF8 byte array.</returns>
		public byte[] GetBytes(IDictionary<string, string> dictionary, IList<string> keyOrder) {
			if (dictionary == null) throw new ArgumentNullException("dictionary");
			if (keyOrder == null) throw new ArgumentNullException("keyOrder");
			if (dictionary.Count != keyOrder.Count) throw new ArgumentException(Strings.KeysListAndDictionaryDoNotMatch);
			MemoryStream ms = new MemoryStream();
			using (StreamWriter sw = new StreamWriter(ms, textEncoding)) {
				sw.NewLine = newLineCharacters;
				foreach (string keyInOrder in keyOrder) {
					string key = keyInOrder.Trim();
					string value = dictionary[key].Trim();
					if (key.IndexOfAny(illegalKeyCharacters) >= 0)
						throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture,
							Strings.InvalidCharacterInKeyValueFormInput, key));
					if (value.IndexOfAny(illegalValueCharacters) >= 0)
						throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture,
							Strings.InvalidCharacterInKeyValueFormInput, value));

					sw.Write(key);
					sw.Write(':');
					sw.Write(value);
					sw.WriteLine();
				}
				sw.Flush();
			
				// Remove the text encoding preamble
				int preambleLength = textEncoding.GetPreamble().Length;
				byte[] bytes = new byte[ms.Length - preambleLength];
				ms.Seek(preambleLength, SeekOrigin.Begin);
				ms.Read(bytes, 0, (int)ms.Length - preambleLength);

				return bytes;
			}
		}

		public IDictionary<string, string> GetDictionary(byte[] buffer) {
			return GetDictionary(buffer, 0, buffer.Length);
		}

		/// <summary>
		/// Decodes bytes in Key-Value Form to key/value pairs.
		/// </summary>
		/// <param name="buffer">The Key-Value Form encoded bytes.</param>
		/// <param name="offset">The offset into the byte array where reading should begin.</param>
		/// <param name="bufferLength">The number of bytes to read from the byte array.</param>
		/// <param name="enforcedSpec">Whether strict OpenID 2.0 spec validation will be performed </param>
		/// <returns>The deserialized dictionary.</returns>
		public IDictionary<string, string> GetDictionary(byte[] buffer, int offset, int bufferLength) {
			MemoryStream ms = new MemoryStream(buffer, offset, bufferLength);
			using (StreamReader reader = new StreamReader(ms, textEncoding)) {
				var dict = new Dictionary<string, string>();
				int line_num = 0;
				string line;
				while ((line = reader.ReadLine()) != null) {
					line_num++;
					if (ConformanceLevel == KeyValueFormConformanceLevel.Loose) {
						line = line.Trim();
						if (line.Length == 0) continue;
					}
					string[] parts = line.Split(new[] { ':' }, 2);
					if (parts.Length != 2) {
						throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture,
							Strings.InvalidKeyValueFormCharacterMissing, ':'));
					}
					if (ConformanceLevel > KeyValueFormConformanceLevel.Loose) {
						if (char.IsWhiteSpace(parts[0], parts[0].Length-1) ||
							char.IsWhiteSpace(parts[1], 0)) {
							throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture,
								Strings.InvalidCharacterInKeyValueFormInput, ' '));
						}
					}
					if (ConformanceLevel < KeyValueFormConformanceLevel.OpenID20) {
						parts[0] = parts[0].Trim();
						parts[1] = parts[1].Trim();
					}
					// calling Add method will throw if a key is encountered twice,
					// which we should do.
					dict.Add(parts[0], parts[1]);
				}
				if (ConformanceLevel > KeyValueFormConformanceLevel.Loose) {
					ms.Seek(-1, SeekOrigin.End);
					if (ms.ReadByte() != '\n') {
						throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture,
							Strings.InvalidKeyValueFormCharacterMissing, "\\n"));
					}
				}
				return dict;
			}
		}
	}
}
