//-----------------------------------------------------------------------
// <copyright file="MessageTransport.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	/// <summary>
	/// The type of transport mechanism used for a message: either direct or indirect.
	/// </summary>
	public enum MessageTransport {
		/// <summary>
		/// A message that is sent directly from the Consumer to the Service Provider, or vice versa.
		/// </summary>
		Direct,

		/// <summary>
		/// A message that is sent from one party to another via a redirect in the user agent.
		/// </summary>
		Indirect,
	}
}
