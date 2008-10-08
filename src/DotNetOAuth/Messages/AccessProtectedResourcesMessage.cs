//-----------------------------------------------------------------------
// <copyright file="AccessProtectedResourcesMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messages {
	using System;
	using DotNetOAuth.Messaging;

	/// <summary>
	/// A message attached to a request for protected resources that provides the necessary
	/// credentials to be granted access to those resources.
	/// </summary>
	public class AccessProtectedResourcesMessage : SignedMessageBase, ITokenContainingMessage {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessProtectedResourcesMessage"/> class.
		/// </summary>
		/// <param name="serviceProvider">The URI of the Service Provider endpoint to send this message to.</param>
		protected internal AccessProtectedResourcesMessage(MessageReceivingEndpoint serviceProvider)
			: base(MessageTransport.Direct, serviceProvider) {
		}

		/// <summary>
		/// Gets or sets the Token.
		/// </summary>
		string ITokenContainingMessage.Token {
			get { return this.AccessToken; }
			set { this.AccessToken = value; }
		}

		/// <summary>
		/// Gets or sets the Access Token.
		/// </summary>
		[MessagePart(Name = "oauth_token", IsRequired = true)]
		public string AccessToken { get; set; }
	}
}
