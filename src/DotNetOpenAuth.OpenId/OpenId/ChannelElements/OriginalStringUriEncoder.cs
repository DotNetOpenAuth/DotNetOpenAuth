//-----------------------------------------------------------------------
// <copyright file="OriginalStringUriEncoder.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging.Reflection;

	/// <summary>
	/// A Uri encoder that serializes using <see cref="Uri.OriginalString"/>
	/// rather than the standard <see cref="Uri.AbsoluteUri"/>.
	/// </summary>
	internal class OriginalStringUriEncoder : IMessagePartEncoder {
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
			return uriValue != null ? uriValue.OriginalString : null;
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
			return value != null ? new Uri(value) : null;
		}

		#endregion
	}
}
