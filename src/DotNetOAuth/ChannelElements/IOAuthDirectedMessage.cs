//-----------------------------------------------------------------------
// <copyright file="IOAuthDirectedMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.ChannelElements {
	using DotNetOAuth.Messaging;

	/// <summary>
	/// Additional properties that apply specifically to OAuth messages.
	/// </summary>
	internal interface IOAuthDirectedMessage : IDirectedProtocolMessage {
		/// <summary>
		/// Gets the preferred method of transport for the message.
		/// </summary>
		HttpDeliveryMethod HttpMethods { get; }
	}
}
