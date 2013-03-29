//-----------------------------------------------------------------------
// <copyright file="IProtocolMessage.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Text;

	/// <summary>
	/// The interface that classes must implement to be serialized/deserialized
	/// as protocol messages.
	/// </summary>
	public interface IProtocolMessage : IMessage {
		/// <summary>
		/// Gets the level of protection this message requires.
		/// </summary>
		MessageProtections RequiredProtection { get; }

		/// <summary>
		/// Gets a value indicating whether this is a direct or indirect message.
		/// </summary>
		MessageTransport Transport { get; }
	}
}
