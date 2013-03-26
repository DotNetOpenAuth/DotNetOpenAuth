//-----------------------------------------------------------------------
// <copyright file="KeyValueFormEncoding.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.IO;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Messaging;
	using Validation;

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
		/// </summary>
		/// <param name="keysAndValues">
		/// The dictionary of key/value pairs to convert to a byte stream.
		/// </param>
		/// <returns>The UTF8 byte array.</returns>
		/// <remarks>
		/// Enumerating a Dictionary&lt;TKey, TValue&gt; has undeterministic ordering.
		/// If ordering of the key=value pairs is important, a deterministic enumerator must
		/// be used.
		/// </remarks>
		[SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Not a problem for this type.")]
		public static byte[] GetBytes(IEnumerable<KeyValuePair<string, string>> keysAndValues) {
			Requires.NotNull(keysAndValues, "keysAndValues");

			using (MemoryStream ms = new MemoryStream()) {
				using (StreamWriter sw = new StreamWriter(ms, textEncoding)) {
					sw.NewLine = NewLineCharacters;
					foreach (var pair in keysAndValues) {
						if (pair.Key.IndexOfAny(IllegalKeyCharacters) >= 0) {
							throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, OpenIdStrings.InvalidCharacterInKeyValueFormInput, pair.Key));
						}
						if (pair.Value.IndexOfAny(IllegalValueCharacters) >= 0) {
							throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, OpenIdStrings.InvalidCharacterInKeyValueFormInput, pair.Value));
						}

						sw.Write(pair.Key);
						sw.Write(':');
						sw.Write(pair.Value);
						sw.WriteLine();
					}
				}

				return ms.ToArray();
			}
		}

		/// <summary>
		/// Decodes bytes in Key-Value Form to key/value pairs.
		/// </summary>
		/// <param name="data">The stream of Key-Value Form encoded bytes.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The deserialized dictionary.
		/// </returns>
		/// <exception cref="FormatException">Thrown when the data is not in the expected format.</exception>
		public async Task<IDictionary<string, string>> GetDictionaryAsync(Stream data, CancellationToken cancellationToken) {
			using (StreamReader reader = new StreamReader(data, textEncoding)) {
				var dict = new Dictionary<string, string>();
				int line_num = 0;
				string line;
				while ((line = await reader.ReadLineAsync()) != null) {
					cancellationToken.ThrowIfCancellationRequested();
					line_num++;
					if (this.ConformanceLevel == KeyValueFormConformanceLevel.Loose) {
						line = line.Trim();
						if (line.Length == 0) {
							continue;
						}
					}
					string[] parts = line.Split(new[] { ':' }, 2);
					ErrorUtilities.VerifyFormat(parts.Length == 2, OpenIdStrings.InvalidKeyValueFormCharacterMissing, ':', line_num, line);
					if (this.ConformanceLevel > KeyValueFormConformanceLevel.Loose) {
						ErrorUtilities.VerifyFormat(!(char.IsWhiteSpace(parts[0], parts[0].Length - 1) || char.IsWhiteSpace(parts[1], 0)), OpenIdStrings.InvalidCharacterInKeyValueFormInput, ' ', line_num, line);
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
					ErrorUtilities.VerifyFormat(reader.BaseStream.ReadByte() == '\n', OpenIdStrings.InvalidKeyValueFormCharacterMissing, "\\n", line_num, line);
				}
				return dict;
			}
		}
	}
}
