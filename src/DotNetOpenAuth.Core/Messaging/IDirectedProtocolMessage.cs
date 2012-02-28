//-----------------------------------------------------------------------
// <copyright file="IDirectedProtocolMessage.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
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
		/// Gets the preferred method of transport for the message.
		/// </summary>
		/// <remarks>
		/// For indirect messages this will likely be GET+POST, which both can be simulated in the user agent:
		/// the GET with a simple 301 Redirect, and the POST with an HTML form in the response with javascript
		/// to automate submission.
		/// </remarks>
		HttpDeliveryMethods HttpMethods { get; }

		/// <summary>
		/// Gets the URL of the intended receiver of this message.
		/// </summary>
		Uri Recipient { get; }
	}
}
