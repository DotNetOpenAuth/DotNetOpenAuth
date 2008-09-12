//-----------------------------------------------------------------------
// <copyright file="ISignedProtocolMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messaging {
	/// <summary>
	/// The contract a message that is signed must implement.
	/// </summary>
	internal interface ISignedProtocolMessage : IProtocolMessage {
		/// <summary>
		/// Gets or sets the message signature.
		/// </summary>
		string Signature { get; set; }
	}
}
