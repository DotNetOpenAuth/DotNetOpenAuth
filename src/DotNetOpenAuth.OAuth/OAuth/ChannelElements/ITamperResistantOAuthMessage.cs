//-----------------------------------------------------------------------
// <copyright file="ITamperResistantOAuthMessage.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Net.Http;

	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;

	/// <summary>
	/// An interface that OAuth messages implement to support signing.
	/// </summary>
	public interface ITamperResistantOAuthMessage : IDirectedProtocolMessage, ITamperResistantProtocolMessage, IMessageOriginalPayload {
		/// <summary>
		/// Gets or sets the method used to sign the message.
		/// </summary>
		string SignatureMethod { get; set; }

		/// <summary>
		/// Gets or sets the Token Secret used to sign the message.
		/// </summary>
		string TokenSecret { get; set; }

		/// <summary>
		/// Gets or sets the Consumer key.
		/// </summary>
		string ConsumerKey { get; set; }

		/// <summary>
		/// Gets or sets the Consumer Secret used to sign the message.
		/// </summary>
		string ConsumerSecret { get; set; }

		/// <summary>
		/// Gets or sets the HTTP method that will be used to transmit the message.
		/// </summary>
		HttpMethod HttpMethod { get; set; }

		/// <summary>
		/// Gets or sets the URL of the intended receiver of this message.
		/// </summary>
		new Uri Recipient { get; set; }
	}
}
