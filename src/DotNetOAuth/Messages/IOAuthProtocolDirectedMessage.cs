//-----------------------------------------------------------------------
// <copyright file="IOAuthProtocolDirectedMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messages {
	using DotNetOAuth.Messaging;

	/// <summary>
	/// Additional properties that apply specifically to OAuth messages.
	/// </summary>
	internal interface IOAuthProtocolDirectedMessage {
		/// <summary>
		/// Gets the preferred method of transport for the message.
		/// </summary>
		MessageScheme PreferredScheme { get; }
	}
}
