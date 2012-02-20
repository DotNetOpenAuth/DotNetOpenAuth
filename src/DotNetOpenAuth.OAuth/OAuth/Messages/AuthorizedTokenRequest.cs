//-----------------------------------------------------------------------
// <copyright file="AuthorizedTokenRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.Messages {
	using System;
	using System.Globalization;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A direct message sent by the Consumer to exchange an authorized Request Token
	/// for an Access Token and Token Secret.
	/// </summary>
	/// <remarks>
	/// The class is sealed because the OAuth spec forbids adding parameters to this message.
	/// </remarks>
	public sealed class AuthorizedTokenRequest : SignedMessageBase, ITokenContainingMessage {
		/// <summary>
		/// Initializes a new instance of the <see cref="AuthorizedTokenRequest"/> class.
		/// </summary>
		/// <param name="serviceProvider">The URI of the Service Provider endpoint to send this message to.</param>
		/// <param name="version">The OAuth version.</param>
		internal AuthorizedTokenRequest(MessageReceivingEndpoint serviceProvider, Version version)
			: base(MessageTransport.Direct, serviceProvider, version) {
		}

		/// <summary>
		/// Gets or sets the Token.
		/// </summary>
		string ITokenContainingMessage.Token {
			get { return this.RequestToken; }
			set { this.RequestToken = value; }
		}

		/// <summary>
		/// Gets or sets the verification code received by the Consumer from the Service Provider 
		/// in the <see cref="UserAuthorizationResponse.VerificationCode"/> property.
		/// </summary>
		[MessagePart("oauth_verifier", IsRequired = true, AllowEmpty = false, MinVersion = Protocol.V10aVersion)]
		public string VerificationCode { get; set; }

		/// <summary>
		/// Gets or sets the authorized Request Token used to obtain authorization.
		/// </summary>
		[MessagePart("oauth_token", IsRequired = true)]
		internal string RequestToken { get; set; }

		/// <summary>
		/// Checks the message state for conformity to the protocol specification
		/// and throws an exception if the message is invalid.
		/// </summary>
		protected override void EnsureValidMessage() {
			base.EnsureValidMessage();

			if (this.ExtraData.Count > 0) {
				throw new ProtocolException(string.Format(CultureInfo.CurrentCulture, OAuthStrings.MessageNotAllowedExtraParameters, GetType().Name));
			}
		}
	}
}
