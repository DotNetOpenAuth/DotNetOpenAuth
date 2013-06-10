//-----------------------------------------------------------------------
// <copyright file="ValueMapping.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging.Reflection {
	using System;
	using Validation;

	/// <summary>
	/// A pair of conversion functions to map some type to a string and back again.
	/// </summary>
	internal struct ValueMapping {
		/// <summary>
		/// The mapping function that converts some custom type to a string.
		/// </summary>
		internal readonly Func<object, string> ValueToString;

		/// <summary>
		/// The mapping function that converts some custom type to the original string
		/// (possibly non-normalized) that represents it.
		/// </summary>
		internal readonly Func<object, string> ValueToOriginalString;

		/// <summary>
		/// The mapping function that converts a string to some custom type.
		/// </summary>
		internal readonly Func<string, object> StringToValue;

		/// <summary>
		/// Initializes a new instance of the <see cref="ValueMapping"/> struct.
		/// </summary>
		/// <param name="toString">The mapping function that converts some custom value to a string.</param>
		/// <param name="toOriginalString">The mapping function that converts some custom value to its original (non-normalized) string.  May be null if the same as the <paramref name="toString"/> function.</param>
		/// <param name="toValue">The mapping function that converts a string to some custom value.</param>
		internal ValueMapping(Func<object, string> toString, Func<object, string> toOriginalString, Func<string, object> toValue)
			: this() {
			Requires.NotNull(toString, "toString");
			Requires.NotNull(toValue, "toValue");

			this.ValueToString = toString;
			this.ValueToOriginalString = toOriginalString ?? toString;
			this.StringToValue = toValue;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ValueMapping"/> struct.
		/// </summary>
		/// <param name="encoder">The encoder.</param>
		internal ValueMapping(IMessagePartEncoder encoder)
			: this() {
			Requires.NotNull(encoder, "encoder");

			this.Encoder = encoder;
			var nullEncoder = encoder as IMessagePartNullEncoder;
			string nullString = nullEncoder != null ? nullEncoder.EncodedNullValue : null;

			var originalStringEncoder = encoder as IMessagePartOriginalEncoder;
			Func<object, string> originalStringEncode = encoder.Encode;
			if (originalStringEncoder != null) {
				originalStringEncode = originalStringEncoder.EncodeAsOriginalString;
			}

			this.ValueToString = obj => (obj != null) ? encoder.Encode(obj) : nullString;
			this.StringToValue = str => (str != null) ? encoder.Decode(str) : null;
			this.ValueToOriginalString = obj => (obj != null) ? originalStringEncode(obj) : nullString;
		}

		/// <summary>
		/// Gets the encoder.
		/// </summary>
		internal IMessagePartEncoder Encoder { get; private set; }
	}
}
