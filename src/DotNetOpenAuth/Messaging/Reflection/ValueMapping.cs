//-----------------------------------------------------------------------
// <copyright file="ValueMapping.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging.Reflection {
	using System;

	/// <summary>
	/// A pair of conversion functions to map some type to a string and back again.
	/// </summary>
	internal struct ValueMapping {
		/// <summary>
		/// The mapping function that converts some custom type to a string.
		/// </summary>
		internal Func<object, string> ValueToString;

		/// <summary>
		/// The mapping function that converts a string to some custom type.
		/// </summary>
		internal Func<string, object> StringToValue;

		/// <summary>
		/// Initializes a new instance of the <see cref="ValueMapping"/> struct.
		/// </summary>
		/// <param name="toString">The mapping function that converts some custom type to a string.</param>
		/// <param name="toValue">The mapping function that converts a string to some custom type.</param>
		internal ValueMapping(Func<object, string> toString, Func<string, object> toValue) {
			if (toString == null) {
				throw new ArgumentNullException("toString");
			}

			if (toValue == null) {
				throw new ArgumentNullException("toValue");
			}

			this.ValueToString = toString;
			this.StringToValue = toValue;
		}
	}
}
