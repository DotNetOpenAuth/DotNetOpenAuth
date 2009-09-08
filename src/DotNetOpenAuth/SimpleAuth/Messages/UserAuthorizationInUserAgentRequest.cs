//-----------------------------------------------------------------------
// <copyright file="UserAuthorizationInUserAgentRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.SimpleAuth.Messages {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.SimpleAuth.ChannelElements;

	/// <summary>
	/// A message sent by the Consumer to the Token Issuer via the user agent
	/// to get the Token Issuer to obtain authorization from the user and prepare
	/// to issue an access token to the Consumer if permission is granted.
	/// </summary>
	internal class UserAuthorizationInUserAgentRequest : MessageBase, IDirectedProtocolMessage {
		/// <summary>
		/// Initializes a new instance of the <see cref="UserAuthorizationInUserAgentRequest"/> class.
		/// </summary>
		/// <param name="tokenIssuer">The token issuer URL to direct the user to.</param>
		/// <param name="version">The protocol version.</param>
		internal UserAuthorizationInUserAgentRequest(Uri tokenIssuer, Version version)
			: base(version, MessageTransport.Indirect, tokenIssuer) {
		}

		/// <summary>
		/// Gets or sets the consumer key.
		/// </summary>
		/// <value>The consumer key.</value>
		[MessagePart(Protocol.sa_consumer_key, IsRequired = true, AllowEmpty = false)]
		internal string ConsumerKey { get; set; }

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
		/// Gets or sets the state of the consumer.
		/// </summary>
		/// <value>
		/// An opaque value that Consumers can use to maintain state associated with this request.
		/// </value>
		/// <remarks>
		/// If this value is present, the Token Issuer MUST return it to the Consumer's callback URL.
		/// </remarks>
		[MessagePart(Protocol.sa_consumer_state, IsRequired = false, AllowEmpty = true)]
		internal string ConsumerState { get; set; }
	}
}
