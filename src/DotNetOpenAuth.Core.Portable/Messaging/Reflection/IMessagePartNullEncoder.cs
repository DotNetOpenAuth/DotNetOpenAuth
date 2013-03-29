//-----------------------------------------------------------------------
// <copyright file="IMessagePartNullEncoder.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging.Reflection {
	/// <summary>
	/// A message part encoder that has a special encoding for a null value.
	/// </summary>
	public interface IMessagePartNullEncoder : IMessagePartEncoder {
		/// <summary>
		/// Gets the string representation to include in a serialized message 
		/// when the message part has a <c>null</c> value.
		/// </summary>
		string EncodedNullValue { get; }
	}
}
