//-----------------------------------------------------------------------
// <copyright file="IMessagePartEncoder.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging.Reflection {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// An interface describing how various objects can be serialized and deserialized between their object and string forms.
	/// </summary>
	/// <remarks>
	/// Implementations of this interface must include a default constructor and must be thread-safe.
	/// </remarks>
	[ContractClass(typeof(IMessagePartEncoderContract))]
	public interface IMessagePartEncoder {
		/// <summary>
		/// Encodes the specified value.
		/// </summary>
		/// <param name="value">The value.  Guaranteed to never be null.</param>
		/// <returns>The <paramref name="value"/> in string form, ready for message transport.</returns>
		string Encode(object value);

		/// <summary>
		/// Decodes the specified value.
		/// </summary>
		/// <param name="value">The string value carried by the transport.  Guaranteed to never be null, although it may be empty.</param>
		/// <returns>The deserialized form of the given string.</returns>
		/// <exception cref="FormatException">Thrown when the string value given cannot be decoded into the required object type.</exception>
		object Decode(string value);
	}

	/// <summary>
	/// Code contract for the <see cref="IMessagePartEncoder"/> type.
	/// </summary>
	[ContractClassFor(typeof(IMessagePartEncoder))]
	internal abstract class IMessagePartEncoderContract : IMessagePartEncoder {
		/// <summary>
		/// Initializes a new instance of the <see cref="IMessagePartEncoderContract"/> class.
		/// </summary>
		protected IMessagePartEncoderContract() {
		}

		#region IMessagePartEncoder Members

		/// <summary>
		/// Encodes the specified value.
		/// </summary>
		/// <param name="value">The value.  Guaranteed to never be null.</param>
		/// <returns>
		/// The <paramref name="value"/> in string form, ready for message transport.
		/// </returns>
		string IMessagePartEncoder.Encode(object value) {
			Requires.NotNull(value, "value");
			throw new NotImplementedException();
		}

		/// <summary>
		/// Decodes the specified value.
		/// </summary>
		/// <param name="value">The string value carried by the transport.  Guaranteed to never be null, although it may be empty.</param>
		/// <returns>
		/// The deserialized form of the given string.
		/// </returns>
		/// <exception cref="FormatException">Thrown when the string value given cannot be decoded into the required object type.</exception>
		object IMessagePartEncoder.Decode(string value) {
			Requires.NotNull(value, "value");
			throw new NotImplementedException();
		}

		#endregion
	}
}
