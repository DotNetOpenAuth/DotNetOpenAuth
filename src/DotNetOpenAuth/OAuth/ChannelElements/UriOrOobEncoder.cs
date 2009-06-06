//-----------------------------------------------------------------------
// <copyright file="UriOrOobEncoder.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging.Reflection;

	/// <summary>
	/// An URI encoder that translates null <see cref="Uri"/> references as "oob" 
	/// instead of an empty/missing argument.
	/// </summary>
	internal class UriOrOobEncoder : IMessagePartEncoder {
		/// <summary>
		/// The string constant "oob", used to indicate an out-of-band configuration.
		/// </summary>
		private const string OutOfBandConfiguration = "oob";

		/// <summary>
		/// Initializes a new instance of the <see cref="UriOrOobEncoder"/> class.
		/// </summary>
		internal UriOrOobEncoder() {
		}

		#region IMessagePartEncoder Members

		/// <summary>
		/// Encodes the specified value.
		/// </summary>
		/// <param name="value">The value.  Guaranteed to never be null.</param>
		/// <returns>
		/// The <paramref name="value"/> in string form, ready for message transport.
		/// </returns>
		public string Encode(object value) {
			Uri uriValue = (Uri)value;
			return uriValue != null ? uriValue.AbsoluteUri : OutOfBandConfiguration;
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
			if (string.Equals(value, OutOfBandConfiguration, StringComparison.Ordinal)) {
				return null;
			} else {
				return new Uri(value, UriKind.Absolute);
			}
		}

		#endregion
	}
}
