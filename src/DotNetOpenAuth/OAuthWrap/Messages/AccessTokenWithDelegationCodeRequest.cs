//-----------------------------------------------------------------------
// <copyright file="AccessTokenWithDelegationCodeRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuthWrap.ChannelElements;

	/// <summary>
	/// A message sent by the Consumer directly to the Token Issuer to exchange
	/// the delegation code for an Access Token.
	/// </summary>
	internal class AccessTokenWithDelegationCodeRequest : MessageBase, IDirectedProtocolMessage {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenWithDelegationCodeRequest"/> class.
		/// </summary>
		/// <param name="tokenIssuer">The token issuer.</param>
		/// <param name="version">The version.</param>
		internal AccessTokenWithDelegationCodeRequest(Uri tokenIssuer, Version version)
			: base(version, MessageTransport.Direct, tokenIssuer) {
			this.HttpMethods = HttpDeliveryMethods.PostRequest;
		}

		/// <summary>
		/// Gets or sets the consumer key.
		/// </summary>
		/// <value>The consumer key.</value>
		[MessagePart(Protocol.sa_consumer_key, IsRequired = true, AllowEmpty = false)]
		internal string ConsumerKey { get; set; }

		/// <summary>
		/// Gets or sets the consumer secret.
		/// </summary>
		/// <value>The consumer secret.</value>
		[MessagePart(Protocol.sa_consumer_secret, IsRequired = true, AllowEmpty = false)]
		internal string ConsumerSecret { get; set; }

		/// <summary>
		/// Gets or sets the delegation code.
		/// </summary>
		/// <value>The delegation code.</value>
		[MessagePart(Protocol.sa_delegation_code, IsRequired = true, AllowEmpty = false)]
		internal string DelegationCode { get; set; }

		/// <summary>
		/// Gets or sets the callback URL.
		/// </summary>
		/// <value>
		/// An absolute URL to which the Token Issuer will redirect the User back after
		/// the user has approved the authorization request.
		/// </value>
		/// <remarks>
		/// Consumers which are unable to receive callbacks MUST use <c>null</c> to indicate it
		/// will receive the Verification Code out of band.
		/// </remarks>
		[MessagePart(Protocol.sa_callback, IsRequired = true, AllowEmpty = false, Encoder = typeof(UriOrOutOfBandEncoding))]
		internal Uri Callback { get; set; }

		/// <summary>
		/// Checks the message state for conformity to the protocol specification
		/// and throws an exception if the message is invalid.
		/// </summary>
		/// <remarks>
		/// 	<para>Some messages have required fields, or combinations of fields that must relate to each other
		/// in specialized ways.  After deserializing a message, this method checks the state of the
		/// message to see if it conforms to the protocol.</para>
		/// 	<para>Note that this property should <i>not</i> check signatures or perform any state checks
		/// outside this scope of this particular message.</para>
		/// </remarks>
		/// <exception cref="ProtocolException">Thrown if the message is invalid.</exception>
		protected override void EnsureValidMessage() {
			base.EnsureValidMessage();
			ErrorUtilities.VerifyProtocol(this.Recipient.IsTransportSecure(), SimpleAuthStrings.HttpsRequired);
		}
	}
}
