//-----------------------------------------------------------------------
// <copyright file="GrantAccessTokenMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messages {
	using System.Collections.Generic;
	using DotNetOAuth.Messaging;

	/// <summary>
	/// A direct message sent from Service Provider to Consumer in response to 
	/// a Consumer's <see cref="GetAccessTokenMessage"/> request.
	/// </summary>
	public class GrantAccessTokenMessage : MessageBase, ITokenSecretContainingMessage {
		/// <summary>
		/// Initializes a new instance of the <see cref="GrantAccessTokenMessage"/> class.
		/// </summary>
		protected internal GrantAccessTokenMessage()
			: base(MessageProtection.None, MessageTransport.Direct) {
		}

		/// <summary>
		/// Gets or sets the Access Token assigned by the Service Provider.
		/// </summary>
		[MessagePart(Name = "oauth_token", IsRequired = true)]
		public string AccessToken { get; set; }

		/// <summary>
		/// Gets or sets the Request or Access Token.
		/// </summary>
		string ITokenContainingMessage.Token {
			get { return this.AccessToken; }
			set { this.AccessToken = value; }
		}

		/// <summary>
		/// Gets or sets the Request or Access Token secret.
		/// </summary>
		string ITokenSecretContainingMessage.TokenSecret {
			get { return this.TokenSecret; }
			set { this.TokenSecret = value; }
		}

		/// <summary>
		/// Gets the extra, non-OAuth parameters that will be included in the message.
		/// </summary>
		public IDictionary<string, string> ExtraData {
			get { return ((IProtocolMessage)this).ExtraData; }
		}

		/// <summary>
		/// Gets or sets the Token Secret.
		/// </summary>
		[MessagePart(Name = "oauth_token_secret", IsRequired = true)]
		internal string TokenSecret { get; set; }
	}
}
