//-----------------------------------------------------------------------
// <copyright file="DateTimeEncoder.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy {
	using System;
	using System.Globalization;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;

	/// <summary>
	/// An encoder/decoder design for DateTimes that must conform to the PAPE spec.
	/// </summary>
	/// <remarks>
	/// The timestamp MUST be formatted as specified in section 5.6 of [RFC3339] (Klyne, G. and C. Newman, “Date and Time on the Internet: Timestamps,” .), with the following restrictions:
	///  * All times must be in the UTC timezone, indicated with a "Z".
	///  * No fractional seconds are allowed
	/// For example: 2005-05-15T17:11:51Z
	/// </remarks>
	internal class DateTimeEncoder : IMessagePartEncoder {
		/// <summary>
		/// An array of the date/time formats allowed by the PAPE extension.
		/// </summary>
		/// <remarks>
		/// TODO: This array of formats is not yet a complete list.
		/// </remarks>
		private static readonly string[] PermissibleDateTimeFormats = { "yyyy-MM-ddTHH:mm:ssZ" };

		#region IMessagePartEncoder Members

		/// <summary>
		/// Encodes the specified value.
		/// </summary>
		/// <param name="value">The value.  Guaranteed to never be null.</param>
		/// <returns>
		/// The <paramref name="value"/> in string form, ready for message transport.
		/// </returns>
		public string Encode(object value) {
			DateTime? dateTime = value as DateTime?;
			if (dateTime.HasValue) {
				return dateTime.Value.ToUniversalTimeSafe().ToString(PermissibleDateTimeFormats[0], CultureInfo.InvariantCulture);
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
			DateTime dateTime;
			if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out dateTime) && dateTime.Kind == DateTimeKind.Utc) { // may be unspecified per our option above
				return dateTime;
			} else {
				Logger.OpenId.ErrorFormat("Invalid format for message part: {0}", value);
				return null;
			}
		}

		#endregion
	}
}
