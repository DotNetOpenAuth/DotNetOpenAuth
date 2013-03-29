//-----------------------------------------------------------------------
// <copyright file="IDirectResponseProtocolMessage.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	/// <summary>
	/// Undirected messages that serve as direct responses to direct requests.
	/// </summary>
	public interface IDirectResponseProtocolMessage : IProtocolMessage {
		/// <summary>
		/// Gets the originating request message that caused this response to be formed.
		/// </summary>
		IDirectedProtocolMessage OriginatingRequest { get; }
	}
}
