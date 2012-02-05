//-----------------------------------------------------------------------
// <copyright file="UnauthorizedTokenRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.Messages {
	using System;
	using System.Collections.Generic;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth.ChannelElements;

	/// <summary>
	/// A direct message sent from Consumer to Service Provider to request a Request Token.
	/// </summary>
	public class UnauthorizedTokenRequest : SignedMessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="UnauthorizedTokenRequest"/> class.
		/// </summary>
		/// <param name="serviceProvider">The URI of the Service Provider endpoint to send this message to.</param>
		/// <param name="version">The OAuth version.</param>
		protected internal UnauthorizedTokenRequest(MessageReceivingEndpoint serviceProvider, Version version)
			: base(MessageTransport.Direct, serviceProvider, version) {
		}

		/// <summary>
		/// Gets or sets the absolute URL to which the Service Provider will redirect the
		/// User back when the Obtaining User Authorization step is completed.
		/// </summary>
		/// <value>
		/// The callback URL; or <c>null</c> if the Consumer is unable to receive
		/// callbacks or a callback URL has been established via other means.
		/// </value>
		[MessagePart("oauth_callback", IsRequired = true, AllowEmpty = false, MinVersion = Protocol.V10aVersion, Encoder = typeof(UriOrOobEncoding))]
		public Uri Callback { get; set; }

		/// <summary>
		/// Gets the extra, non-OAuth parameters that will be included in the message.
		/// </summary>
		public new IDictionary<string, string> ExtraData {
			get { return base.ExtraData; }
		}
	}
}
