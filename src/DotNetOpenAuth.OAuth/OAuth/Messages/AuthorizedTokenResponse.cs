//-----------------------------------------------------------------------
// <copyright file="AuthorizedTokenResponse.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.Messages {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A direct message sent from Service Provider to Consumer in response to 
	/// a Consumer's <see cref="AuthorizedTokenRequest"/> request.
	/// </summary>
	public class AuthorizedTokenResponse : MessageBase, ITokenSecretContainingMessage {
		/// <summary>
		/// Initializes a new instance of the <see cref="AuthorizedTokenResponse"/> class.
		/// </summary>
		/// <param name="originatingRequest">The originating request.</param>
		protected internal AuthorizedTokenResponse(AuthorizedTokenRequest originatingRequest)
			: base(MessageProtections.None, originatingRequest, originatingRequest.Version) {
		}

		/// <summary>
		/// Gets or sets the Access Token assigned by the Service Provider.
		/// </summary>
		[MessagePart("oauth_token", IsRequired = true)]
		public string AccessToken { get; set; }

		/// <summary>
		/// Gets or sets the Request or Access Token.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "This property IS accessible by a different name.")]
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
		public new IDictionary<string, string> ExtraData {
			get { return base.ExtraData; }
		}

		/// <summary>
		/// Gets or sets the Token Secret.
		/// </summary>
		[MessagePart("oauth_token_secret", IsRequired = true)]
		protected internal string TokenSecret { get; set; }
	}
}
