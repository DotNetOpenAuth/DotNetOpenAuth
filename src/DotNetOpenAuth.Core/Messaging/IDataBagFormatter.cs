//-----------------------------------------------------------------------
// <copyright file="IDataBagFormatter.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
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
			Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));

			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Deserializes a <see cref="DataBag"/>.
		/// </summary>
		/// <param name="message">The instance to deserialize into</param>
		/// <param name="data">The serialized form of the <see cref="DataBag"/> to deserialize.  Must not be null or empty.</param>
		/// <param name="containingMessage">The message that contains the <see cref="DataBag"/> serialized value.  Must not be nulll.</param>
		/// <param name="messagePartName">Name of the message part whose value is to be deserialized.  Used for exception messages.</param>
		void IDataBagFormatter<T>.Deserialize(T message, string data, IProtocolMessage containingMessage, string messagePartName) {
			Requires.NotNull(message, "message");
			Requires.NotNull(containingMessage, "containingMessage");
			Requires.NotNullOrEmpty(data, "data");
			Requires.NotNullOrEmpty(messagePartName, "messagePartName");
			Contract.Ensures(Contract.Result<T>() != null);

			throw new System.NotImplementedException();
		}

		#endregion
	}
}