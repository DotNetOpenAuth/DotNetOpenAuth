//-----------------------------------------------------------------------
// <copyright file="IDataBagFormatter.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.ChannelElements {
	using System;
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A serializer for <see cref="DataBag"/>-derived types
	/// </summary>
	/// <typeparam name="T">The DataBag-derived type that is to be serialized/deserialized.</typeparam>
	[ContractClass(typeof(IDataBagFormatterContract<>))]
	internal interface IDataBagFormatter<T> where T : DataBag, new() {
		string Serialize(T message);

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

		string IDataBagFormatter<T>.Serialize(T message) {
			Contract.Requires<ArgumentNullException>(message != null, "message");
			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));

			throw new System.NotImplementedException();
		}

		T IDataBagFormatter<T>.Deserialize(IProtocolMessage containingMessage, string data) {
			Contract.Requires<ArgumentNullException>(containingMessage != null, "containingMessage");
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(data));
			Contract.Ensures(Contract.Result<T>() != null);

			throw new System.NotImplementedException();
		}

		#endregion
	}
}