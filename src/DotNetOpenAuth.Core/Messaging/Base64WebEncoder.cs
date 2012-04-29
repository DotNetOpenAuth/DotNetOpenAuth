//-----------------------------------------------------------------------
// <copyright file="Base64WebEncoder.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging.Reflection;

	/// <summary>
	/// A message part encoder that translates between <c>byte[]</c> and base64web encoded strings.
	/// </summary>
	internal class Base64WebEncoder : IMessagePartEncoder {
		/// <summary>
		/// Encodes the specified value.
		/// </summary>
		/// <param name="value">The value.  Guaranteed to never be null.</param>
		/// <returns>The <paramref name="value"/> in string form, ready for message transport.</returns>
		public string Encode(object value) {
			return MessagingUtilities.ConvertToBase64WebSafeString((byte[])value);
		}

		/// <summary>
		/// Decodes the specified value.
		/// </summary>
		/// <param name="value">The string value carried by the transport.  Guaranteed to never be null, although it may be empty.</param>
		/// <returns>The deserialized form of the given string.</returns>
		/// <exception cref="FormatException">Thrown when the string value given cannot be decoded into the required object type.</exception>
		public object Decode(string value) {
			return MessagingUtilities.FromBase64WebSafeString(value);
		}
	}
}
