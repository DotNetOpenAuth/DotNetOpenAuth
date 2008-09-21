//-----------------------------------------------------------------------
// <copyright file="ITamperResistantOAuthMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messages {
	using DotNetOAuth.Messaging.Bindings;

	/// <summary>
	/// An interface that OAuth messages implement to support signing.
	/// </summary>
	internal interface ITamperResistantOAuthMessage : ITamperResistantProtocolMessage {
		/// <summary>
		/// Gets or sets the method used to sign the message.
		/// </summary>
		string SignatureMethod { get; set; }
	}
}
