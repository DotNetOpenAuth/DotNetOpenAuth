//-----------------------------------------------------------------------
// <copyright file="ChannelEventArgs.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;

	/// <summary>
	/// The data packet sent with Channel events.
	/// </summary>
	public class ChannelEventArgs : EventArgs {
		/// <summary>
		/// Initializes a new instance of the <see cref="ChannelEventArgs"/> class.
		/// </summary>
		/// <param name="message">The message behind the fired event..</param>
		internal ChannelEventArgs(IProtocolMessage message) {
			ErrorUtilities.VerifyArgumentNotNull(message, "message");

			this.Message = message;
		}

		/// <summary>
		/// Gets the message that caused the event to fire.
		/// </summary>
		public IProtocolMessage Message { get; private set; }
	}
}
