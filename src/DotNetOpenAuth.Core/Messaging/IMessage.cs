//-----------------------------------------------------------------------
// <copyright file="IMessage.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Text;

	/// <summary>
	/// The interface that classes must implement to be serialized/deserialized
	/// as protocol or extension messages.
	/// </summary>
	[ContractClass(typeof(IMessageContract))]
	public interface IMessage {
		/// <summary>
		/// Gets the version of the protocol or extension this message is prepared to implement.
		/// </summary>
		/// <remarks>
		/// Implementations of this interface should ensure that this property never returns null.
		/// </remarks>
		Version Version { get; }

		/// <summary>
		/// Gets the extra, non-standard Protocol parameters included in the message.
		/// </summary>
		/// <remarks>
		/// Implementations of this interface should ensure that this property never returns null.
		/// </remarks>
		IDictionary<string, string> ExtraData { get; }

		/// <summary>
		/// Checks the message state for conformity to the protocol specification
		/// and throws an exception if the message is invalid.
		/// </summary>
		/// <remarks>
		/// <para>Some messages have required fields, or combinations of fields that must relate to each other
		/// in specialized ways.  After deserializing a message, this method checks the state of the 
		/// message to see if it conforms to the protocol.</para>
		/// <para>Note that this property should <i>not</i> check signatures or perform any state checks
		/// outside this scope of this particular message.</para>
		/// </remarks>
		/// <exception cref="ProtocolException">Thrown if the message is invalid.</exception>
		void EnsureValidMessage();
	}

	/// <summary>
	/// Code contract for the <see cref="IMessage"/> interface.
	/// </summary>
	[ContractClassFor(typeof(IMessage))]
	internal abstract class IMessageContract : IMessage {
		/// <summary>
		/// Prevents a default instance of the <see cref="IMessageContract"/> class from being created.
		/// </summary>
		private IMessageContract() {
		}

		/// <summary>
		/// Gets the version of the protocol or extension this message is prepared to implement.
		/// </summary>
		Version IMessage.Version {
			get {
				Contract.Ensures(Contract.Result<Version>() != null);
				return default(Version); // dummy return
			}
		}

		/// <summary>
		/// Gets the extra, non-standard Protocol parameters included in the message.
		/// </summary>
		/// <value></value>
		/// <remarks>
		/// Implementations of this interface should ensure that this property never returns null.
		/// </remarks>
		IDictionary<string, string> IMessage.ExtraData {
			get {
				Contract.Ensures(Contract.Result<IDictionary<string, string>>() != null);
				return default(IDictionary<string, string>);
			}
		}

		/// <summary>
		/// Checks the message state for conformity to the protocol specification
		/// and throws an exception if the message is invalid.
		/// </summary>
		/// <remarks>
		/// 	<para>Some messages have required fields, or combinations of fields that must relate to each other
		/// in specialized ways.  After deserializing a message, this method checks the state of the
		/// message to see if it conforms to the protocol.</para>
		/// 	<para>Note that this property should <i>not</i> check signatures or perform any state checks
		/// outside this scope of this particular message.</para>
		/// </remarks>
		/// <exception cref="ProtocolException">Thrown if the message is invalid.</exception>
		void IMessage.EnsureValidMessage() {
		}
	}
}
