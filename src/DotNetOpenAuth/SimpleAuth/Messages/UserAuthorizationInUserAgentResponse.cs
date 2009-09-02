//-----------------------------------------------------------------------
// <copyright file="UserAuthorizationInUserAgentResponse.cs" company="Andrew Arnott">
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

	/// <summary>
	/// The message sent by the Token Issuer to the Consumer via the user agent
	/// to indicate that user authorization was granted, and to return the user
	/// to the Consumer where they started their experience.
	/// </summary>
	internal class UserAuthorizationInUserAgentResponse : IDirectedProtocolMessage {
		/// <summary>
		/// A dictionary to contain extra message data.
		/// </summary>
		private Dictionary<string, string> extraData = new Dictionary<string, string>();

		/// <summary>
		/// Initializes a new instance of the <see cref="UserAuthorizationInUserAgentResponse"/> class.
		/// </summary>
		/// <param name="consumerCallback">The consumer callback.</param>
		/// <param name="version">The protocol version.</param>
		internal UserAuthorizationInUserAgentResponse(Uri consumerCallback, Version version) {
			Contract.Requires<ArgumentNullException>(consumerCallback != null);
			Contract.Requires<ArgumentNullException>(version != null);

			this.Recipient = consumerCallback;
			this.Version = version;
		}

		/// <summary>
		/// Gets or sets the verifier.
		/// </summary>
		/// <value>The verification code, if the user authorized the Consumer.</value>
		[MessagePart("sa_verifier", IsRequired = true, AllowEmpty = true)]
		public string Verifier { get; set; }

		/// <summary>
		/// Gets or sets the state of the consumer as provided by the consumer in the
		/// authorization request.
		/// </summary>
		/// <value>The state of the consumer.</value>
		/// <remarks>
		/// REQUIRED if the Consumer sent the value in the <see cref="UserAuthorizationRequestInUserAgentRequest"/>.
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
