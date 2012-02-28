//-----------------------------------------------------------------------
// <copyright file="UserAuthorizationResponse.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.Messages {
	using System;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A message used to redirect the user from a Service Provider to a Consumer's web site.
	/// </summary>
	/// <remarks>
	/// The class is sealed because extra parameters are determined by the callback URI provided by the Consumer.
	/// </remarks>
	[Serializable]
	public sealed class UserAuthorizationResponse : MessageBase, ITokenContainingMessage {
		/// <summary>
		/// Initializes a new instance of the <see cref="UserAuthorizationResponse"/> class.
		/// </summary>
		/// <param name="consumer">The URI of the Consumer endpoint to send this message to.</param>
		/// <param name="version">The OAuth version.</param>
		internal UserAuthorizationResponse(Uri consumer, Version version)
			: base(MessageProtections.None, MessageTransport.Indirect, new MessageReceivingEndpoint(consumer, HttpDeliveryMethods.GetRequest), version) {
		}

		/// <summary>
		/// Gets or sets the Request or Access Token.
		/// </summary>
		string ITokenContainingMessage.Token {
			get { return this.RequestToken; }
			set { this.RequestToken = value; }
		}

		/// <summary>
		/// Gets or sets the verification code that must accompany the request to exchange the
		/// authorized request token for an access token.
		/// </summary>
		/// <value>An unguessable value passed to the Consumer via the User and REQUIRED to complete the process.</value>
		/// <remarks>
		/// If the Consumer did not provide a callback URL, the Service Provider SHOULD display the value of the 
		/// verification code, and instruct the User to manually inform the Consumer that authorization is 
		/// completed. If the Service Provider knows a Consumer to be running on a mobile device or set-top box, 
		/// the Service Provider SHOULD ensure that the verifier value is suitable for manual entry.
		/// </remarks>
		[MessagePart("oauth_verifier", IsRequired = true, AllowEmpty = false, MinVersion = Protocol.V10aVersion)]
		public string VerificationCode { get; set; }

		/// <summary>
		/// Gets or sets the Request Token.
		/// </summary>
		[MessagePart("oauth_token", IsRequired = true)]
		internal string RequestToken { get; set; }
	}
}
