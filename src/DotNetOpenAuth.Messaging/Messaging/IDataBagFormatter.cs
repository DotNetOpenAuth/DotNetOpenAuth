//-----------------------------------------------------------------------
// <copyright file="IDataBagFormatter.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Diagnostics.Contracts;

	/// <summary>
	/// A serializer for <see cref="DataBag"/>-derived types
	/// </summary>
	/// <typeparam name="T">The DataBag-derived type that is to be serialized/deserialized.</typeparam>
	[ContractClass(typeof(IDataBagFormatterContract<>))]
	internal interface IDataBagFormatter<T> where T : DataBag, new() {
		/// <summary>
		/// Serializes the specified message.
		/// </summary>
		/// <param name="message">The message to serialize.  Must not be null.</param>
		/// <returns>A non-null, non-empty value.</returns>
		string Serialize(T message);

		/// <summary>
		/// Deserializes a <see cref="DataBag"/>.
		/// </summary>
		/// <param name="containingMessage">The message that contains the <see cref="DataBag"/> serialized value.  Must not be nulll.</param>
		/// <param name="data">The serialized form of the <see cref="DataBag"/> to deserialize.  Must not be null or empty.</param>
		/// <returns>The deserialized value.  Never null.</returns>
		T Deserialize(IProtocolMessage containingMessage, string data);
	}

	/// <summary>
	/// Contract class for the IDataBagFormatter interface.
	/// </summary>
	/// <typeparam name="T">The type of DataBag to serialize.</typeparam>
	[ContractClassFor(typeof(IDataBagFormatter<>))]
	internal abstract class IDataBagFormatterContract<T> : IDataBagFormatter<T> where T : DataBag, new() {
		/// <summary>
		/// Prevents a default instance of the <see cref="IDataBagFormatterContract&lt;T&gt;"/> class from being created.
		/// </summary>
		private IDataBagFormatterContract() {
		}

		#region IDataBagFormatter<T> Members

		/// <summary>
		/// Serializes the specified message.
		/// </summary>
		/// <param name="message">The message to serialize.  Must not be null.</param>
		/// <returns>A non-null, non-empty value.</returns>
		string IDataBagFormatter<T>.Serialize(T message) {
			Requires.NotNull(message, "message");
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));

			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Deserializes a <see cref="DataBag"/>.
		/// </summary>
		/// <param name="containingMessage">The message that contains the <see cref="DataBag"/> serialized value.  Must not be nulll.</param>
		/// <param name="data">The serialized form of the <see cref="DataBag"/> to deserialize.  Must not be null or empty.</param>
		/// <returns>The deserialized value.  Never null.</returns>
		T IDataBagFormatter<T>.Deserialize(IProtocolMessage containingMessage, string data) {
			Requires.NotNull(containingMessage, "containingMessage");
			Requires.NotNullOrEmpty(data, "data");
			Contract.Ensures(Contract.Result<T>() != null);

			throw new System.NotImplementedException();
		}

		#endregion
	}
}