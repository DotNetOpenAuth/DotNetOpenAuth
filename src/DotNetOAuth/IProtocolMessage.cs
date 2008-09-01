namespace DotNetOAuth {
	using System;
	using System.Collections.Generic;
	using System.Text;

	/// <summary>
	/// The interface that classes must implement to be serialized/deserialized
	/// as OAuth messages.
	/// </summary>
	interface IProtocolMessage {
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
