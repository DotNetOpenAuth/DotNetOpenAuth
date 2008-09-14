//-----------------------------------------------------------------------
// <copyright file="IProtocolMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Text;

	/// <summary>
	/// The interface that classes must implement to be serialized/deserialized
	/// as OAuth messages.
	/// </summary>
	internal interface IProtocolMessage {
		/// <summary>
		/// Gets the version of the protocol this message is prepared to implement.
		/// </summary>
		Version ProtocolVersion { get; }

		/// <summary>
		/// Gets the level of protection this message requires.
		/// </summary>
		MessageProtection RequiredProtection { get; }

		/// <summary>
		/// Gets whether this is a direct or indirect message.
		/// </summary>
		MessageTransport Transport { get; }

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
}
