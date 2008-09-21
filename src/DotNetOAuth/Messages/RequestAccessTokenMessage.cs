//-----------------------------------------------------------------------
// <copyright file="RequestAccessTokenMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messages {
	using System;
	using DotNetOAuth.Messaging;

	/// <summary>
	/// A direct message sent by the Consumer to exchange a Request Token for an Access Token
	/// and Token Secret.
	/// </summary>
	internal class RequestAccessTokenMessage : MessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="RequestAccessTokenMessage"/> class.
		/// </summary>
		/// <param name="serviceProvider">The URI of the Service Provider endpoint to send this message to.</param>
		internal RequestAccessTokenMessage(Uri serviceProvider)
			: base(MessageProtection.All, MessageTransport.Direct, serviceProvider) {
		}

		/// <summary>
		/// Gets or sets the Consumer Key.
		/// </summary>
		[MessagePart(Name = "oauth_consumer_key", IsRequired = true)]
		public string ConsumerKey { get; set; }

		/// <summary>
		/// Gets or sets the Request Token.
		/// </summary>
		[MessagePart(Name = "oauth_token", IsRequired = true)]
		public string RequestToken { get; set; }

		/// <summary>
		/// Gets or sets the protocol version used in the construction of this message.
		/// </summary>
		[MessagePart(Name = "oauth_version", IsRequired = false)]
		public string Version {
			get { return this.VersionString; }
			set { this.VersionString = value; }
		}
	}
}
