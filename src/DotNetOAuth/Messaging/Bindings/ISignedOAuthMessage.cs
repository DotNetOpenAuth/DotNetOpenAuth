//-----------------------------------------------------------------------
// <copyright file="ISignedOAuthMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messaging.Bindings {
	/// <summary>
	/// The contract a message that is signed must implement.
	/// </summary>
	internal interface ISignedOAuthMessage : IProtocolMessage {
		/// <summary>
		/// Gets or sets the message signature.
		/// </summary>
		string Signature { get; set; }
	}
}
