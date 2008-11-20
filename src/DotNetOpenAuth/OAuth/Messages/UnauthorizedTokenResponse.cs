//-----------------------------------------------------------------------
// <copyright file="UnauthorizedTokenResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.Messages {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A direct message sent from Service Provider to Consumer in response to 
	/// a Consumer's <see cref="UnauthorizedTokenRequest"/> request.
	/// </summary>
	public class UnauthorizedTokenResponse : MessageBase, ITokenSecretContainingMessage {
		/// <summary>
		/// Initializes a new instance of the <see cref="UnauthorizedTokenResponse"/> class.
		/// </summary>
		/// <param name="requestMessage">The unauthorized request token message that this message is being generated in response to.</param>
		/// <param name="requestToken">The request token.</param>
		/// <param name="tokenSecret">The token secret.</param>
		/// <remarks>
		/// This constructor is used by the Service Provider to send the message.
		/// </remarks>
		protected internal UnauthorizedTokenResponse(UnauthorizedTokenRequest requestMessage, string requestToken, string tokenSecret)
			: this(requestMessage) {
			ErrorUtilities.VerifyArgumentNotNull(requestToken, "requestToken");
			ErrorUtilities.VerifyArgumentNotNull(tokenSecret, "tokenSecret");

			this.RequestToken = requestToken;
			this.TokenSecret = tokenSecret;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UnauthorizedTokenResponse"/> class.
		/// </summary>
		/// <param name="originatingRequest">The originating request.</param>
		/// <remarks>This constructor is used by the consumer to deserialize the message.</remarks>
		protected internal UnauthorizedTokenResponse(UnauthorizedTokenRequest originatingRequest)
			: base(MessageProtections.None, originatingRequest) {
		}

		/// <summary>
		/// Gets or sets the Request or Access Token.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "This property IS accessible by a different name.")]
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
		public new IDictionary<string, string> ExtraData {
			get { return base.ExtraData; }
		}

		/// <summary>
		/// Gets or sets the Request Token.
		/// </summary>
		[MessagePart("oauth_token", IsRequired = true)]
		internal string RequestToken { get; set; }

		/// <summary>
		/// Gets the original request for an unauthorized token.
		/// </summary>
		internal UnauthorizedTokenRequest RequestMessage {
			get { return (UnauthorizedTokenRequest)this.OriginatingRequest; }
		}

		/// <summary>
		/// Gets or sets the Token Secret.
		/// </summary>
		[MessagePart("oauth_token_secret", IsRequired = true)]
		protected internal string TokenSecret { get; set; }
	}
}
