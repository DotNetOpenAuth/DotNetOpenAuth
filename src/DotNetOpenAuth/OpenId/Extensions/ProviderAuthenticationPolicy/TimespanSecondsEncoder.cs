//-----------------------------------------------------------------------
// <copyright file="TimespanSecondsEncoder.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy {
	using System;
	using System.Globalization;
	using DotNetOpenAuth.Messaging.Reflection;

	/// <summary>
	/// Encodes and decodes the <see cref="TimeSpan"/> as an integer of total seconds.
	/// </summary>
	internal class TimespanSecondsEncoder : IMessagePartEncoder {
		#region IMessagePartEncoder Members

		/// <summary>
		/// Encodes the specified value.
		/// </summary>
		/// <param name="value">The value.  Guaranteed to never be null.</param>
		/// <returns>
		/// The <paramref name="value"/> in string form, ready for message transport.
		/// </returns>
		public string Encode(object value) {
			TimeSpan? timeSpan = value as TimeSpan?;
			if (timeSpan.HasValue) {
				return timeSpan.Value.TotalSeconds.ToString(CultureInfo.InvariantCulture);
			} else {
				return null;
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
			return TimeSpan.FromSeconds(double.Parse(value, CultureInfo.InvariantCulture));
		}

		#endregion
	}
}
