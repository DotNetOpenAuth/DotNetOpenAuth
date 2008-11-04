//-----------------------------------------------------------------------
// <copyright file="UnauthorizedTokenRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.Messages {
	using System.Collections.Generic;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A direct message sent from Consumer to Service Provider to request a Request Token.
	/// </summary>
	public class UnauthorizedTokenRequest : SignedMessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="UnauthorizedTokenRequest"/> class.
		/// </summary>
		/// <param name="serviceProvider">The URI of the Service Provider endpoint to send this message to.</param>
		protected internal UnauthorizedTokenRequest(MessageReceivingEndpoint serviceProvider)
			: base(MessageTransport.Direct, serviceProvider) {
		}

		/// <summary>
		/// Gets the extra, non-OAuth parameters that will be included in the message.
		/// </summary>
		public new IDictionary<string, string> ExtraData {
			get { return base.ExtraData; }
		}
	}
}
