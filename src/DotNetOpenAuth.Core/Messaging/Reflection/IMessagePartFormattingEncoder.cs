//-----------------------------------------------------------------------
// <copyright file="IMessagePartFormattingEncoder.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging.Reflection {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// An interface describing how various objects can be serialized and deserialized between their object and string forms.
	/// </summary>
	/// <remarks>
	/// Implementations of this interface must include a default constructor and must be thread-safe.
	/// </remarks>
	public interface IMessagePartFormattingEncoder : IMessagePartEncoder {
		/// <summary>
		/// Gets the type of the encoded values produced by this encoder, as they would appear in their preferred form.
		/// </summary>
		Type FormattingType { get; }
	}
}
