//-----------------------------------------------------------------------
// <copyright file="IMessageWithClientState.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	using DotNetOpenAuth.Messaging;

	internal interface IMessageWithClientState : IProtocolMessage {
		string ClientState { get; set; }
	}
}
