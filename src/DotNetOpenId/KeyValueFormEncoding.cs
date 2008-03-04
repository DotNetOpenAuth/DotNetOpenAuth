using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Globalization;

namespace DotNetOpenId {
	/// <summary>
	/// Conversion to and from the Key-Value Form Encoding defined by
	/// OpenID Authentication 2.0 section 4.1.1.
	/// http://openid.net/specs/openid-authentication-2_0.html#anchor4
	/// </summary>
	internal static class KeyValueFormEncoding {
		static readonly char[] illegalKeyCharacters = { '\n', ':' };
		static readonly char[] illegalValueCharacters = { '\n' };
		const string newLineCharacters = "\n";
		static readonly Encoding textEncoding = Encoding.UTF8;
		public const ConformanceLevel DefaultConformanceLevel = ConformanceLevel.Loose;

		/// <summary>
		/// Encodes key/value pairs to Key-Value Form.
		/// </summary>
		/// <returns>The UTF8 byte array.</returns>
		public static byte[] GetBytes(IDictionary<string, string> dictionary) {
			MemoryStream ms = new MemoryStream();
			using (StreamWriter sw = new StreamWriter(ms, textEncoding)) {
				sw.NewLine = newLineCharacters;
				foreach (var pair in dictionary) {
					string key = pair.Key.Trim();
					string value = pair.Value.Trim();
					if (key.IndexOfAny(illegalKeyCharacters) >= 0)
						throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture,
							Strings.InvalidCharacterInKeyValueFormInput, pair.Key));
					if (value.IndexOfAny(illegalValueCharacters) >= 0)
						throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture,
							Strings.InvalidCharacterInKeyValueFormInput, pair.Value));

					sw.WriteLine("{0}:{1}", key, value);
				}
			}

			return ms.ToArray();
		}

		public static IDictionary<string, string> GetDictionary(byte[] buffer) {
			return GetDictionary(buffer, 0, buffer.Length, DefaultConformanceLevel);
		}

		public enum ConformanceLevel {
			Loose,
			OpenID11,
			OpenID20,
		}

		/// <summary>
		/// Decodes bytes in Key-Value Form to key/value pairs.
		/// </summary>
		/// <param name="buffer">The Key-Value Form encoded bytes.</param>
		/// <param name="offset">The offset into the byte array where reading should begin.</param>
		/// <param name="bufferLength">The number of bytes to read from the byte array.</param>
		/// <param name="enforcedSpec">Whether strict OpenID 2.0 spec validation will be performed </param>
		/// <returns>The deserialized dictionary.</returns>
		public static IDictionary<string, string> GetDictionary(byte[] buffer, int offset, int bufferLength, ConformanceLevel enforcedSpec) {
			MemoryStream ms = new MemoryStream(buffer, offset, bufferLength);
			using (StreamReader reader = new StreamReader(ms, textEncoding)) {
				var dict = new Dictionary<string, string>();
				int line_num = 0;
				string line;
				while ((line = reader.ReadLine()) != null) {
					line_num++;
					if (enforcedSpec == ConformanceLevel.Loose) {
						line = line.Trim();
						if (line.Length == 0) continue;
					}
					string[] parts = line.Split(new[] { ':' }, 2);
					if (parts.Length != 2) {
						throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture,
							Strings.InvalidCharacterInKeyValueFormInput, ':'));
					}
					if (enforcedSpec > ConformanceLevel.Loose) {
						if (char.IsWhiteSpace(parts[0], parts[0].Length-1) ||
							char.IsWhiteSpace(parts[1], 0)) {
							throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture,
								Strings.InvalidCharacterInKeyValueFormInput, ' '));
						}
					}
					if (enforcedSpec < ConformanceLevel.OpenID20) {
						parts[0] = parts[0].Trim();
						parts[1] = parts[1].Trim();
					}
					// calling Add method will throw if a key is encountered twice,
					// which we should do.
					dict.Add(parts[0], parts[1]);
				}
				if (enforcedSpec > ConformanceLevel.Loose) {
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
