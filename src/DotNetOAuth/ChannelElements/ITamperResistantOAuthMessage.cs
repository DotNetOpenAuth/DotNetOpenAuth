//-----------------------------------------------------------------------
// <copyright file="ITamperResistantOAuthMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.ChannelElements {
	using System.Collections.Generic;
	using DotNetOAuth.Messaging;
	using DotNetOAuth.Messaging.Bindings;

	/// <summary>
	/// An interface that OAuth messages implement to support signing.
	/// </summary>
	internal interface ITamperResistantOAuthMessage : IDirectedProtocolMessage, ITamperResistantProtocolMessage {
		/// <summary>
		/// Gets or sets the method used to sign the message.
		/// </summary>
		string SignatureMethod { get; set; }

		/// <summary>
		/// Gets or sets the Token Secret used to sign the message.
		/// Only applicable to Consumer.
		/// </summary>
		string TokenSecret { get; set; }

		/// <summary>
		/// Gets or sets the Consumer Secret used to sign the message.
		/// Only applicable to Consumer.
		/// </summary>
		string ConsumerSecret { get; set; }

		/// <summary>
		/// Gets or sets the HTTP method that will be used to transmit the message.
		/// Only applicable to Consumer.
		/// </summary>
		string HttpMethod { get; set; }

		/// <summary>
		/// Gets or sets the extra, non-OAuth parameters that will be included in the request.
		/// Only applicable to Consumer.
		/// </summary>
		IDictionary<string, string> AdditionalParametersInHttpRequest { get; set; }
	}
}
