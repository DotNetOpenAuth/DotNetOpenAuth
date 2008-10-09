//-----------------------------------------------------------------------
// <copyright file="AccessProtectedResourceMessage.cs" company="Andrew Arnott">
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
	public class AccessProtectedResourceMessage : SignedMessageBase, ITokenContainingMessage {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessProtectedResourceMessage"/> class.
		/// </summary>
		/// <param name="serviceProvider">The URI of the Service Provider endpoint to send this message to.</param>
		protected internal AccessProtectedResourceMessage(MessageReceivingEndpoint serviceProvider)
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
		/// <remarks>
		/// In addition to just allowing OAuth to verify a valid message,
		/// this property is useful on the Service Provider to verify that the access token
		/// has proper authorization for the resource being requested, and to know the
		/// context around which user provided the authorization.
		/// </remarks>
		[MessagePart(Name = "oauth_token", IsRequired = true)]
		public string AccessToken { get; set; }
	}
}
