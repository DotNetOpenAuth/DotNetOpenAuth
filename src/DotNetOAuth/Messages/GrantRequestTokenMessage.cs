//-----------------------------------------------------------------------
// <copyright file="GrantRequestTokenMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messages {
	using System.Collections.Generic;
	using DotNetOAuth.Messaging;

	/// <summary>
	/// A direct message sent from Service Provider to Consumer in response to 
	/// a Consumer's <see cref="GetRequestTokenMessage"/> request.
	/// </summary>
	public class GrantRequestTokenMessage : MessageBase, ITokenSecretContainingMessage {
		/// <summary>
		/// Initializes a new instance of the <see cref="GrantRequestTokenMessage"/> class.
		/// </summary>
		/// <param name="requestToken">The request token.</param>
		/// <param name="tokenSecret">The token secret.</param>
		protected internal GrantRequestTokenMessage(string requestToken, string tokenSecret) : this() {
			this.RequestToken = requestToken;
			this.TokenSecret = tokenSecret;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GrantRequestTokenMessage"/> class.
		/// </summary>
		protected internal GrantRequestTokenMessage()
			: base(MessageProtection.None, MessageTransport.Direct) {
		}

		/// <summary>
		/// Gets or sets the Request or Access Token.
		/// </summary>
		string ITokenContainingMessage.Token {
			get { return this.RequestToken; }
			set { this.RequestToken = value; }
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
		/// Gets or sets the Request Token.
		/// </summary>
		[MessagePart(Name = "oauth_token", IsRequired = true)]
		internal string RequestToken { get; set; }

		/// <summary>
		/// Gets or sets the Token Secret.
		/// </summary>
		[MessagePart(Name = "oauth_token_secret", IsRequired = true)]
		internal string TokenSecret { get; set; }
	}
}
