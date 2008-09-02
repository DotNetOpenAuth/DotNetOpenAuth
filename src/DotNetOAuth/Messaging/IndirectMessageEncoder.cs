//-----------------------------------------------------------------------
// <copyright file="IndirectMessageEncoder.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messaging {
	/// <summary>
	/// A message encoder that prepares messages for transmittal.
	/// </summary>
	internal class IndirectMessageEncoder {
		/// <summary>
		/// Prepares a protocol message for sending
		/// </summary>
		/// <param name="message">The indirect message to send.</param>
		/// <returns>The encoded message to send to the user agent.</returns>
		internal ProtocolMessageResponse Encode(IProtocolMessage message) {
			throw new System.NotImplementedException();
		}
	}
}
