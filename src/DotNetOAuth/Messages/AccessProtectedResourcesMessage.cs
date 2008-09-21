//-----------------------------------------------------------------------
// <copyright file="AccessProtectedResourcesMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messages {
	using System;
	using System.Runtime.Serialization;
	using DotNetOAuth.Messaging;

	/// <summary>
	/// A message attached to a request for protected resources that provides the necessary
	/// credentials to be granted access to those resources.
	/// </summary>
	internal class AccessProtectedResourcesMessage : MessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessProtectedResourcesMessage"/> class.
		/// </summary>
		/// <param name="serviceProvider">The URI of the Service Provider endpoint to send this message to.</param>
		internal AccessProtectedResourcesMessage(Uri serviceProvider)
			: base(MessageProtection.All, MessageTransport.Direct, serviceProvider) {
		}

		/// <summary>
		/// Gets or sets the Consumer key.
		/// </summary>
		[MessagePart(Name = "oauth_consumer_key", IsRequired = true)]
		public string ConsumerKey { get; set; }

		/// <summary>
		/// Gets or sets the Access Token.
		/// </summary>
		[MessagePart(Name = "oauth_token", IsRequired = true)]
		public string AccessToken { get; set; }

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
