//-----------------------------------------------------------------------
// <copyright file="KeyValueFormEncoding.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.IO;
	using System.Text;

	/// <summary>
	/// Indicates the level of strictness to require when decoding a
	/// Key-Value Form encoded dictionary.
	/// </summary>
	public enum KeyValueFormConformanceLevel {
		/// <summary>
		/// Be as forgiving as possible to errors made while encoding.
		/// </summary>
		Loose,

		/// <summary>
		/// Allow for certain errors in encoding attributable to ambiguities
		/// in the OpenID 1.1 spec's description of the encoding.
		/// </summary>
		OpenId11,

		/// <summary>
		/// The strictest mode.  The decoder requires the encoded dictionary
		/// to be in strict compliance with OpenID 2.0's description of
		/// the encoding.
		/// </summary>
		OpenId20,
	}

	/// <summary>
	/// Performs conversion to and from the Key-Value Form Encoding defined by
	/// OpenID Authentication 2.0 section 4.1.1.
	/// http://openid.net/specs/openid-authentication-2_0.html#anchor4
	/// </summary>
	/// <remarks>
	/// This class is thread safe and immutable.
	/// </remarks>
	internal class KeyValueFormEncoding {
		/// <summary>
		/// Characters that must not appear in parameter names.
		/// </summary>
		private static readonly char[] IllegalKeyCharacters = { '\n', ':' };

		/// <summary>
		/// Characters that must not appaer in parameter values.
		/// </summary>
		private static readonly char[] IllegalValueCharacters = { '\n' };

		/// <summary>
		/// The newline character sequence to use.
		/// </summary>
		private const string NewLineCharacters = "\n";

		/// <summary>
		/// The character encoding to use.
		/// </summary>
		private static readonly Encoding textEncoding = new UTF8Encoding(false);

		/// <summary>
		/// Initializes a new instance of the <see cref="KeyValueFormEncoding"/> class.
		/// </summary>
		public KeyValueFormEncoding() {
			this.ConformanceLevel = KeyValueFormConformanceLevel.Loose;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="KeyValueFormEncoding"/> class.
		/// </summary>
		/// <param name="conformanceLevel">How strictly an incoming Key-Value Form message will be held to the spec.</param>
		public KeyValueFormEncoding(KeyValueFormConformanceLevel conformanceLevel) {
			this.ConformanceLevel = conformanceLevel;
		}

		/// <summary>
		/// Gets a value controlling how strictly an incoming Key-Value Form message will be held to the spec.
		/// </summary>
		public KeyValueFormConformanceLevel ConformanceLevel { get; private set; }

		/// <summary>
		/// Encodes key/value pairs to Key-Value Form.
		/// Do not use for dictionaries of signed fields!  Instead use the overload
		/// that accepts a list of in-order keys.
		/// </summary>
		/// <param name="dictionary">The dictionary with key/value pairs to encode in Key-Value Form.</param>
		/// <returns>The UTF8 byte array.</returns>
		/// <remarks>
		/// Because dictionaries do not guarantee ordering,
		/// encoding a dictionary without an explicitly given key order
		/// is useless in OpenID scenarios where a signature must match.
		/// </remarks>
		public byte[] GetBytes(IDictionary<string, string> dictionary) {
			string[] keys = new string[dictionary.Count];
			dictionary.Keys.CopyTo(keys, 0);
			return this.GetBytes(dictionary, keys);
		}

		/// <summary>
		/// Encodes key/value pairs to Key-Value Form.
		/// </summary>
		/// <param name="dictionary">
		/// The dictionary of key/value pairs to convert to a byte stream.
		/// </param>
		/// <param name="keyOrder">
		/// The order in which to encode the key/value pairs.
		/// Useful in scenarios where a byte[] must be exactly reproduced.
		/// </param>
		/// <returns>The UTF8 byte array.</returns>
		public byte[] GetBytes(IDictionary<string, string> dictionary, IList<string> keyOrder) {
			if (dictionary == null) {
				throw new ArgumentNullException("dictionary");
			}
			if (keyOrder == null) {
				throw new ArgumentNullException("keyOrder");
			}
			if (dictionary.Count != keyOrder.Count) {
				throw new ArgumentException(OpenIdStrings.KeysListAndDictionaryDoNotMatch);
			}

			MemoryStream ms = new MemoryStream();
			using (StreamWriter sw = new StreamWriter(ms, textEncoding)) {
				sw.NewLine = NewLineCharacters;
				foreach (string keyInOrder in keyOrder) {
					string key = keyInOrder.Trim();
					string value = dictionary[key].Trim();
					if (key.IndexOfAny(IllegalKeyCharacters) >= 0) {
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, OpenIdStrings.InvalidCharacterInKeyValueFormInput, key));
					}
					if (value.IndexOfAny(IllegalValueCharacters) >= 0) {
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, OpenIdStrings.InvalidCharacterInKeyValueFormInput, value));
					}

					sw.Write(key);
					sw.Write(':');
					sw.Write(value);
					sw.WriteLine();
				}
			}
			return ms.ToArray();
		}

		/// <summary>
		/// Decodes bytes in Key-Value Form to key/value pairs.
		/// </summary>
		/// <param name="data">The stream of Key-Value Form encoded bytes.</param>
		/// <returns>The deserialized dictionary.</returns>
		public IDictionary<string, string> GetDictionary(Stream data) {
			using (StreamReader reader = new StreamReader(data, textEncoding)) {
				var dict = new Dictionary<string, string>();
				int line_num = 0;
				string line;
				while ((line = reader.ReadLine()) != null) {
					line_num++;
					if (this.ConformanceLevel == KeyValueFormConformanceLevel.Loose) {
						line = line.Trim();
						if (line.Length == 0) {
							continue;
						}
					}
					string[] parts = line.Split(new[] { ':' }, 2);
					if (parts.Length != 2) {
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, OpenIdStrings.InvalidKeyValueFormCharacterMissing, ':', line_num, line));
					}
					if (this.ConformanceLevel > KeyValueFormConformanceLevel.Loose) {
						if (char.IsWhiteSpace(parts[0], parts[0].Length - 1) ||
							char.IsWhiteSpace(parts[1], 0)) {
							throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, OpenIdStrings.InvalidCharacterInKeyValueFormInput, ' ', line_num, line));
						}
					}
					if (this.ConformanceLevel < KeyValueFormConformanceLevel.OpenId20) {
						parts[0] = parts[0].Trim();
						parts[1] = parts[1].Trim();
					}

					// calling Add method will throw if a key is encountered twice,
					// which we should do.
					dict.Add(parts[0], parts[1]);
				}
				if (this.ConformanceLevel > KeyValueFormConformanceLevel.Loose) {
					reader.BaseStream.Seek(-1, SeekOrigin.End);
					if (reader.BaseStream.ReadByte() != '\n') {
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, OpenIdStrings.InvalidKeyValueFormCharacterMissing, "\\n", line_num, line));
					}
				}
				return dict;
			}
		}
	}
}
