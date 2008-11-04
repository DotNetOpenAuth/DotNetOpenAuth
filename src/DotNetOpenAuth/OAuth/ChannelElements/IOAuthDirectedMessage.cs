//-----------------------------------------------------------------------
// <copyright file="IOAuthDirectedMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Additional properties that apply specifically to OAuth messages.
	/// </summary>
	public interface IOAuthDirectedMessage : IDirectedProtocolMessage {
		/// <summary>
		/// Gets the preferred method of transport for the message.
		/// </summary>
		HttpDeliveryMethods HttpMethods { get; }

		/// <summary>
		/// Gets or sets the URL of the intended receiver of this message.
		/// </summary>
		new Uri Recipient { get; set; }
	}
}
