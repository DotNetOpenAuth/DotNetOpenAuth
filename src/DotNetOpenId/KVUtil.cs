using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;

namespace DotNetOpenId {
	internal static class DictionarySerializer {
		/// <summary>
		/// Serializes a string dictionary as a UTF8 string with colons and newlines as delimiters.
		/// </summary>
		/// <returns>The UTF8 byte array.</returns>
		public static byte[] Serialize(IDictionary<string, string> seq) {
			MemoryStream ms = new MemoryStream();

			foreach (var pair in seq) {
				if (pair.Key.IndexOf('\n') >= 0)
					throw new ArgumentException("Invalid input for SeqToKV: key contains newline");

				byte[] line = Encoding.UTF8.GetBytes(pair.Key.Trim() + ":" + pair.Value.Trim() + "\n");
				ms.Write(line, 0, line.Length);
			}

			return ms.ToArray();
		}

		/// <summary>
		/// Deserializes a string dictionary from a UTF8 string with colons and newlines as delimiters.
		/// </summary>
		/// <param name="utf8Buffer">The UTF8 encoded byte buffer.</param>
		/// <param name="bufferLength">The number of bytes to read from the byte array.</param>
		/// <returns>The deserialized dictionary.</returns>
		public static IDictionary<string, string> Deserialize(byte[] utf8Buffer, int bufferLength) {
			StringReader reader = new StringReader(UTF8Encoding.UTF8.GetString(utf8Buffer, 0, bufferLength));
			var dict = new Dictionary<string, string>();
			int line_num = 0;
			string line;

			while ((line = reader.ReadLine()) != null) {
				line_num++;
				line = line.Trim();
				if (line.Length == 0) continue;
				string[] parts = line.Split(new char[] { ':' }, 2);
				if (parts.Length != 2) {
					System.Diagnostics.Trace.WriteLine("Line " + line_num.ToString() + " does not contain a colon.");
				} else {
					dict[parts[0].Trim()] = parts[1].Trim();
				}
			}
			return dict;
		}
	}
}
