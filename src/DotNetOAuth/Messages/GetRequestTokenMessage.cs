//-----------------------------------------------------------------------
// <copyright file="GetRequestTokenMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messages {
	using System.Collections.Generic;
	using DotNetOAuth.Messaging;

	/// <summary>
	/// A direct message sent from Consumer to Service Provider to request a Request Token.
	/// </summary>
	public class GetRequestTokenMessage : SignedMessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="GetRequestTokenMessage"/> class.
		/// </summary>
		/// <param name="serviceProvider">The URI of the Service Provider endpoint to send this message to.</param>
		protected internal GetRequestTokenMessage(MessageReceivingEndpoint serviceProvider)
			: base(MessageTransport.Direct, serviceProvider) {
		}

		/// <summary>
		/// Gets the extra, non-OAuth parameters that will be included in the message.
		/// </summary>
		public IDictionary<string, string> ExtraData {
			get { return ((IProtocolMessage)this).ExtraData; }
		}
	}
}
