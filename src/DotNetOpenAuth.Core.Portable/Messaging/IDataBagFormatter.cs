//-----------------------------------------------------------------------
// <copyright file="IDataBagFormatter.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using Validation;

	/// <summary>
	/// A serializer for <see cref="DataBag"/>-derived types
	/// </summary>
	/// <typeparam name="T">The DataBag-derived type that is to be serialized/deserialized.</typeparam>
	internal interface IDataBagFormatter<in T> where T : DataBag {
		/// <summary>
		/// Serializes the specified message.
		/// </summary>
		/// <param name="message">The message to serialize.  Must not be null.</param>
		/// <returns>A non-null, non-empty value.</returns>
		string Serialize(T message);

		/// <summary>
		/// Deserializes a <see cref="DataBag"/>.
		/// </summary>
		/// <param name="message">The instance to deserialize into</param>
		/// <param name="data">The serialized form of the <see cref="DataBag"/> to deserialize.  Must not be null or empty.</param>
		/// <param name="containingMessage">The message that contains the <see cref="DataBag"/> serialized value.  May be null if no carrying message is applicable.</param>
		/// <param name="messagePartName">The name of the parameter whose value is to be deserialized.  Used for error message generation, but may be <c>null</c>.</param>
		void Deserialize(T message, string data, IProtocolMessage containingMessage = null, string messagePartName = null);
	}
}