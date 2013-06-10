//-----------------------------------------------------------------------
// <copyright file="UriOrOobEncoding.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;

	/// <summary>
	/// An URI encoder that translates null <see cref="Uri"/> references as "oob" 
	/// instead of an empty/missing argument.
	/// </summary>
	internal class UriOrOobEncoding : IMessagePartNullEncoder {
		/// <summary>
		/// The string constant "oob", used to indicate an out-of-band configuration.
		/// </summary>
		private const string OutOfBandConfiguration = "oob";

		/// <summary>
		/// Initializes a new instance of the <see cref="UriOrOobEncoding"/> class.
		/// </summary>
		public UriOrOobEncoding() {
		}

		#region IMessagePartNullEncoder Members

		/// <summary>
		/// Gets the string representation to include in a serialized message
		/// when the message part has a <c>null</c> value.
		/// </summary>
		/// <value></value>
		public string EncodedNullValue {
			get { return OutOfBandConfiguration; }
		}

		#endregion

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
			return uriValue.AbsoluteUri;
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
