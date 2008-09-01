//-----------------------------------------------------------------------
// <copyright file="DirectMessageChannel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messaging {
	/// <summary>
	/// Manages sending direct messages to a remote party and receiving responses.
	/// </summary>
	internal class DirectMessageChannel {
		/// <summary>
		/// Sends a direct message and returns the response.
		/// </summary>
		/// <param name="message">The message to send.</param>
		/// <returns>The message response.</returns>
		public IProtocolMessage Send(IProtocolMessage message) {
			throw new System.NotImplementedException();
		}
	}
}
