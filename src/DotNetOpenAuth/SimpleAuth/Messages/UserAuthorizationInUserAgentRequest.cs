//-----------------------------------------------------------------------
// <copyright file="UserAuthorizationInUserAgentRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.SimpleAuth.Messages {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth.ChannelElements;

	/// <summary>
	/// A message sent by the Consumer to the Token Issuer via the user agent
	/// to get the Token Issuer to obtain authorization from the user and prepare
	/// to issue an access token to the Consumer if permission is granted.
	/// </summary>
	internal class UserAuthorizationInUserAgentRequest : IDirectedProtocolMessage {
		/// <summary>
		/// A dictionary to contain extra message data.
		/// </summary>
		private Dictionary<string, string> extraData = new Dictionary<string, string>();

		/// <summary>
		/// Initializes a new instance of the <see cref="UserAuthorizationInUserAgentRequest"/> class.
		/// </summary>
		/// <param name="tokenIssuer">The token issuer URL to direct the user to.</param>
		/// <param name="version">The protocol version.</param>
		internal UserAuthorizationInUserAgentRequest(MessageReceivingEndpoint tokenIssuer, Version version) {
			Contract.Requires<ArgumentNullException>(tokenIssuer != null);
			Contract.Requires<ArgumentNullException>(version != null);

			this.Recipient = tokenIssuer.Location;
			this.Version = version;
		}

		/// <summary>
		/// Gets or sets the consumer key.
		/// </summary>
		/// <value>The consumer key.</value>
		[MessagePart("sa_consumer_key", IsRequired = true, AllowEmpty = false)]
		public string ConsumerKey { get; set; }

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
		[MessagePart("sa_callback", IsRequired = true, AllowEmpty = false, Encoder = typeof(UriOrOobEncoding))]
		public Uri Callback { get; set; }

		/// <summary>
		/// Gets or sets the state of the consumer.
		/// </summary>
		/// <value>
		/// An opaque value that Consumers can use to maintain state associated with this request.
		/// </value>
		/// <remarks>
		/// If this value is present, the Token Issuer MUST return it to the Consumer's callback URL.
		/// </remarks>
		[MessagePart("sa_consumer_state", IsRequired = false, AllowEmpty = true)]
		public string ConsumerState { get; set; }

		#region IDirectedProtocolMessage Members

		/// <summary>
		/// Gets the preferred method of transport for the message.
		/// </summary>
		/// <remarks>
		/// For indirect messages this will likely be GET+POST, which both can be simulated in the user agent:
		/// the GET with a simple 301 Redirect, and the POST with an HTML form in the response with javascript
		/// to automate submission.
		/// </remarks>
		public HttpDeliveryMethods HttpMethods {
			get { return HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.PostRequest; }
		}

		/// <summary>
		/// Gets the URL of the intended receiver of this message.
		/// </summary>
		public Uri Recipient { get; private set; }

		#endregion

		#region IProtocolMessage Members

		/// <summary>
		/// Gets the level of protection this message requires.
		/// </summary>
		/// <value><see cref="MessageProtections.None"/></value>
		public MessageProtections RequiredProtection {
			get { return MessageProtections.None; }
		}

		/// <summary>
		/// Gets a value indicating whether this is a direct or indirect message.
		/// </summary>
		/// <value><see cref="MessageTransport.Indirect"/></value>
		public MessageTransport Transport {
			get { return MessageTransport.Indirect; }
		}

		#endregion

		#region IMessage Members

		/// <summary>
		/// Gets the version of the protocol or extension this message is prepared to implement.
		/// </summary>
		/// <remarks>
		/// Implementations of this interface should ensure that this property never returns null.
		/// </remarks>
		public Version Version { get; private set; }

		/// <summary>
		/// Gets the extra, non-standard Protocol parameters included in the message.
		/// </summary>
		/// <remarks>
		/// Implementations of this interface should ensure that this property never returns null.
		/// </remarks>
		public IDictionary<string, string> ExtraData {
			get { return this.extraData; }
		}

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
		public void EnsureValidMessage() {
		}

		#endregion
	}
}
