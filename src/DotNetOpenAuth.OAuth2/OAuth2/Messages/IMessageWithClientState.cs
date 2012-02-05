//-----------------------------------------------------------------------
// <copyright file="IMessageWithClientState.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A message carrying client state the authorization server should preserve on behalf of the client
	/// during an authorization.
	/// </summary>
	internal interface IMessageWithClientState : IProtocolMessage {
		/// <summary>
		/// Gets or sets the state of the client.
		/// </summary>
		/// <value>The state of the client.</value>
		string ClientState { get; set; }
	}
}
