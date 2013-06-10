//-----------------------------------------------------------------------
// <copyright file="TimespanSecondsEncoder.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Globalization;
	using DotNetOpenAuth.Messaging.Reflection;

	/// <summary>
	/// Encodes and decodes the <see cref="TimeSpan"/> as an integer of total seconds.
	/// </summary>
	internal class TimespanSecondsEncoder : IMessagePartEncoder, IMessagePartFormattingEncoder {
		/// <summary>
		/// Initializes a new instance of the <see cref="TimespanSecondsEncoder"/> class.
		/// </summary>
		public TimespanSecondsEncoder() {
			// Note that this constructor is public so it can be instantiated via Activator.
		}

		#region IMessagePartFormattingEncoder members

		/// <summary>
		/// Gets the type of the encoded values produced by this encoder, as they would appear in their preferred form.
		/// </summary>
		public Type FormattingType {
			get { return typeof(int); }
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
