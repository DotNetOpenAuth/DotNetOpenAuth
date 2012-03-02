//-----------------------------------------------------------------------
// <copyright file="IMessagePartOriginalEncoder.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging.Reflection {
	/// <summary>
	/// An interface describing how various objects can be serialized and deserialized between their object and string forms.
	/// </summary>
	/// <remarks>
	/// Implementations of this interface must include a default constructor and must be thread-safe.
	/// </remarks>
	public interface IMessagePartOriginalEncoder : IMessagePartEncoder {
		/// <summary>
		/// Encodes the specified value as the original value that was formerly decoded.
		/// </summary>
		/// <param name="value">The value.  Guaranteed to never be null.</param>
		/// <returns>The <paramref name="value"/> in string form, ready for message transport.</returns>
		string EncodeAsOriginalString(object value);
	}
}
