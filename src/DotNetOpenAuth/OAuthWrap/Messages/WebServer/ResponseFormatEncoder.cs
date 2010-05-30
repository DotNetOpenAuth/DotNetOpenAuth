//-----------------------------------------------------------------------
// <copyright file="ResponseFormatEncoder.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages.WebServer {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;

	/// <summary>
	/// Provides encoding/decoding of the json/xml/form enum type.
	/// </summary>
	public class ResponseFormatEncoder : IMessagePartEncoder {
		/// <summary>
		/// Initializes a new instance of the <see cref="ResponseFormatEncoder"/> class.
		/// </summary>
		public ResponseFormatEncoder() {
		}

		/// <summary>
		/// Encodes the specified value.
		/// </summary>
		/// <param name="value">The value.  Guaranteed to never be null.</param>
		/// <returns>
		/// The <paramref name="value"/> in string form, ready for message transport.
		/// </returns>
		public string Encode(object value) {
			if (value == null) {
				return null;
			}

			var format = (ResponseFormat)value;
			switch (format) {
				case ResponseFormat.Xml:
					return Protocol.ResponseFormats.Xml;
				case ResponseFormat.Json:
					return Protocol.ResponseFormats.Json;
				case ResponseFormat.Form:
					return Protocol.ResponseFormats.Form;
				default:
					throw new ArgumentOutOfRangeException("value");
			}
		}

		/// <summary>
		/// Decodes the specified value.
		/// </summary>
		/// <param name="value">The string value carried by the transport.  Guaranteed to never be null, although it may be empty.</param>
		/// <returns>
		/// The deserialized form of the given string.
		/// </returns>
		/// <exception cref="FormatException">Thrown when the string value given cannot be decoded into the required object type.</exception>
		public object Decode(string value) {
			switch (value) {
				case Protocol.ResponseFormats.Xml:
					return ResponseFormat.Xml;
				case Protocol.ResponseFormats.Json:
					return ResponseFormat.Json;
				case Protocol.ResponseFormats.Form:
					return ResponseFormat.Form;
				default:
					throw new ArgumentOutOfRangeException("value");
			}
		}
	}
}
