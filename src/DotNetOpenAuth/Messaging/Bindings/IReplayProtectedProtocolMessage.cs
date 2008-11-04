//-----------------------------------------------------------------------
// <copyright file="IReplayProtectedProtocolMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging.Bindings {
	using System;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// The contract a message that has an allowable time window for processing must implement.
	/// </summary>
	/// <remarks>
	/// All replay-protected messages must also be set to expire so the nonces do not have
	/// to be stored indefinitely.
	/// </remarks>
	internal interface IReplayProtectedProtocolMessage : IExpiringProtocolMessage {
		/// <summary>
		/// Gets or sets the nonce that will protect the message from replay attacks.
		/// </summary>
		string Nonce { get; set; }
	}
}
