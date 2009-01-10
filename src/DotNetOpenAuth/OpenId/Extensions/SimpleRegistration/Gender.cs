//-----------------------------------------------------------------------
// <copyright file="Gender.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.SimpleRegistration {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;

	/// <summary>
	/// Indicates the gender of a user.
	/// </summary>
	public enum Gender {
		/// <summary>
		/// The user is male.
		/// </summary>
		Male,

		/// <summary>
		/// The user is female.
		/// </summary>
		Female,
	}

	/// <summary>
	/// Encodes/decodes the Simple Registration Gender type to its string representation.
	/// </summary>
	internal class GenderEncoder : IMessagePartEncoder {
		#region IMessagePartEncoder Members

		/// <summary>
		/// Encodes the specified value.
		/// </summary>
		/// <param name="value">The value.  Guaranteed to never be null.</param>
		/// <returns>
		/// The <paramref name="value"/> in string form, ready for message transport.
		/// </returns>
		public string Encode(object value) {
			Gender? gender = (Gender?)value;
			if (gender.HasValue) {
				switch (gender.Value) {
					case Gender.Male: return Constants.Genders.Male;
					case Gender.Female: return Constants.Genders.Female;
				}
			}

			return null;
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
				case Constants.Genders.Male: return SimpleRegistration.Gender.Male;
				case Constants.Genders.Female: return SimpleRegistration.Gender.Female;
				default: throw new FormatException();
			}
		}

		#endregion
	}
}