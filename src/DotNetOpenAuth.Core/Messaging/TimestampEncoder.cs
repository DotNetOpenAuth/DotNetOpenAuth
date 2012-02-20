//-----------------------------------------------------------------------
// <copyright file="TimestampEncoder.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Globalization;
	using DotNetOpenAuth.Messaging.Reflection;

	/// <summary>
	/// Translates between a <see cref="DateTime"/> and the number of seconds between it and 1/1/1970 12 AM
	/// </summary>
	internal class TimestampEncoder : IMessagePartEncoder {
		/// <summary>
		/// The reference date and time for calculating time stamps.
		/// </summary>
		internal static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>
		/// Initializes a new instance of the <see cref="TimestampEncoder"/> class.
		/// </summary>
		public TimestampEncoder() {
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

			var timestamp = (DateTime)value;
			TimeSpan secondsSinceEpoch = timestamp - Epoch;
			return ((int)secondsSinceEpoch.TotalSeconds).ToString(CultureInfo.InvariantCulture);
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
			if (value == null) {
				return null;
			}

			var secondsSinceEpoch = int.Parse(value, CultureInfo.InvariantCulture);
			return Epoch.AddSeconds(secondsSinceEpoch);
		}
	}
}
