//-----------------------------------------------------------------------
// <copyright file="IDirectedProtocolMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;

	/// <summary>
	/// Implemented by messages that have explicit recipients
	/// (direct requests and all indirect messages).
	/// </summary>
	public interface IDirectedProtocolMessage : IProtocolMessage {
		/// <summary>
		/// Gets the URL of the intended receiver of this message.
		/// </summary>
		Uri Recipient { get; }
	}
}
