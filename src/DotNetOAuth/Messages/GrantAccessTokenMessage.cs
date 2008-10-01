//-----------------------------------------------------------------------
// <copyright file="GrantAccessTokenMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messages {
	using DotNetOAuth.Messaging;

	/// <summary>
	/// A direct message sent from Service Provider to Consumer in response to 
	/// a Consumer's <see cref="RequestAccessTokenMessage"/> request.
	/// </summary>
	internal class GrantAccessTokenMessage : MessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="GrantAccessTokenMessage"/> class.
		/// </summary>
		internal GrantAccessTokenMessage()
			: base(MessageProtection.None, MessageTransport.Direct) {
		}

		/// <summary>
		/// Gets or sets the Access Token assigned by the Service Provider.
		/// </summary>
		[MessagePart(Name = "oauth_token", IsRequired = true)]
		internal string AccessToken { get; set; }

		/// <summary>
		/// Gets or sets the Token Secret.
		/// </summary>
		[MessagePart(Name = "oauth_token_secret", IsRequired = true)]
		internal string TokenSecret { get; set; }
	}
}
