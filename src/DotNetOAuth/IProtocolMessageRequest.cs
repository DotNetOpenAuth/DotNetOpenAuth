//-----------------------------------------------------------------------
// <copyright file="IProtocolMessageRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth {
	using System;

	/// <summary>
	/// Implemented by messages that are sent as requests.
	/// </summary>
	internal interface IProtocolMessageRequest : IProtocolMessage {
		/// <summary>
		/// Gets or sets the URL of the intended receiver of this message.
		/// </summary>
		Uri Recipient {
			get;
			set;
		}
	}
}
