//-----------------------------------------------------------------------
// <copyright file="UnauthorizedRequestTokenMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messages {
	using System;
	using System.Runtime.Serialization;
	using DotNetOAuth.Messaging;

	/// <summary>
	/// A direct message sent from Service Provider to Consumer in response to 
	/// a Consumer's <see cref="RequestTokenMessage"/> request.
	/// </summary>
	internal class UnauthorizedRequestTokenMessage : MessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="UnauthorizedRequestTokenMessage"/> class.
		/// </summary>
		internal UnauthorizedRequestTokenMessage()
			: base(MessageProtection.None, MessageTransport.Direct) {
		}

		/// <summary>
		/// Gets or sets the Request Token.
		/// </summary>
		[MessagePart(Name = "oauth_token", IsRequired = true)]
		public string RequestToken { get; set; }

		/// <summary>
		/// Gets or sets the Token Secret.
		/// </summary>
		[MessagePart(Name = "oauth_token_secret", IsRequired = true)]
		public string TokenSecret { get; set; }
	}
}
