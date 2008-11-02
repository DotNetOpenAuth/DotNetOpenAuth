//-----------------------------------------------------------------------
// <copyright file="GetAccessTokenMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messages {
	using System;
	using DotNetOAuth.Messaging;
	using System.Globalization;

	/// <summary>
	/// A direct message sent by the Consumer to exchange an authorized Request Token
	/// for an Access Token and Token Secret.
	/// </summary>
	/// <remarks>
	/// The class is sealed because the OAuth spec forbids adding parameters to this message.
	/// </remarks>
	public sealed class GetAccessTokenMessage : SignedMessageBase, ITokenContainingMessage {
		/// <summary>
		/// Initializes a new instance of the <see cref="GetAccessTokenMessage"/> class.
		/// </summary>
		/// <param name="serviceProvider">The URI of the Service Provider endpoint to send this message to.</param>
		internal GetAccessTokenMessage(MessageReceivingEndpoint serviceProvider)
			: base(MessageTransport.Direct, serviceProvider) {
		}

		/// <summary>
		/// Gets or sets the Token.
		/// </summary>
		string ITokenContainingMessage.Token {
			get { return this.RequestToken; }
			set { this.RequestToken = value; }
		}

		/// <summary>
		/// Gets or sets the Request Token.
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
				throw new ProtocolException(string.Format(CultureInfo.CurrentCulture, Strings.MessageNotAllowedExtraParameters, GetType().Name));
			}
		}
	}
}
