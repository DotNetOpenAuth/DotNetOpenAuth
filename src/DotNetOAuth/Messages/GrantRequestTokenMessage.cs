//-----------------------------------------------------------------------
// <copyright file="GrantRequestTokenMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messages {
	using DotNetOAuth.Messaging;

	/// <summary>
	/// A direct message sent from Service Provider to Consumer in response to 
	/// a Consumer's <see cref="GetRequestTokenMessage"/> request.
	/// </summary>
	public class GrantRequestTokenMessage : MessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="GrantRequestTokenMessage"/> class.
		/// </summary>
		protected internal GrantRequestTokenMessage()
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
