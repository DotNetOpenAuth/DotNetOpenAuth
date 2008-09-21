//-----------------------------------------------------------------------
// <copyright file="RequestTokenMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messages {
	using System;
	using System.Runtime.Serialization;
	using DotNetOAuth.Messaging;

	/// <summary>
	/// A direct message sent from Consumer to Service Provider to request a token.
	/// </summary>
	internal class RequestTokenMessage : MessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="RequestTokenMessage"/> class.
		/// </summary>
		/// <param name="serviceProvider">The URI of the Service Provider endpoint to send this message to.</param>
		internal RequestTokenMessage(Uri serviceProvider)
			: base(MessageProtection.All, MessageTransport.Direct, serviceProvider) {
		}

		/// <summary>
		/// Gets or sets the consumer key.
		/// </summary>
		[MessagePart(Name = "oauth_consumer_key", IsRequired = true)]
		public string ConsumerKey { get; set; }

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
