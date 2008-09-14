//-----------------------------------------------------------------------
// <copyright file="IChannelBindingElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// An interface that must be implemented by message transforms/validators in order
	/// to be included in the channel stack.
	/// </summary>
	internal interface IChannelBindingElement {
		/// <summary>
		/// Gets the protection offered (if any) by this binding element.
		/// </summary>
		MessageProtection Protection { get; }

		/// <summary>
		/// Prepares a message for sending based on the rules of this channel binding element.
		/// </summary>
		/// <param name="message">The message to prepare for sending.</param>
		void PrepareMessageForSending(IProtocolMessage message);

		/// <summary>
		/// Performs any transformation on an incoming message that may be necessary and/or
		/// validates an incoming message based on the rules of this channel binding element.
		/// </summary>
		/// <param name="message">The incoming message to process.</param>
		void PrepareMessageForReceiving(IProtocolMessage message);
	}
}
