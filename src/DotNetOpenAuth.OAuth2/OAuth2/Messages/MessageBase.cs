//-----------------------------------------------------------------------
// <copyright file="MessageBase.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// A common message base class for OAuth messages.
	/// </summary>
	[Serializable]
	public class MessageBase : IDirectedProtocolMessage, IDirectResponseProtocolMessage {
		/// <summary>
		/// A dictionary to contain extra message data.
		/// </summary>
		private Dictionary<string, string> extraData = new Dictionary<string, string>();

		/// <summary>
		/// The originating request.
		/// </summary>
		private IDirectedProtocolMessage originatingRequest;

		/// <summary>
		/// The backing field for the <see cref="IMessage.Version"/> property.
		/// </summary>
		private Version version;

		/// <summary>
		/// A value indicating whether this message is a direct or indirect message.
		/// </summary>
		private MessageTransport messageTransport;

		/// <summary>
		/// Initializes a new instance of the <see cref="MessageBase"/> class
		/// that is used for direct response messages.
		/// </summary>
		/// <param name="version">The version.</param>
		protected MessageBase(Version version) {
			Requires.NotNull(version, "version");
			this.messageTransport = MessageTransport.Direct;
			this.version = version;
			this.HttpMethods = HttpDeliveryMethods.GetRequest;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MessageBase"/> class.
		/// </summary>
		/// <param name="request">The originating request.</param>
		/// <param name="recipient">The recipient of the directed message.  Null if not applicable.</param>
		protected MessageBase(IDirectedProtocolMessage request, Uri recipient = null) {
			Requires.NotNull(request, "request");
			this.originatingRequest = request;
			this.messageTransport = request.Transport;
			this.version = request.Version;
			this.Recipient = recipient;
			this.HttpMethods = HttpDeliveryMethods.GetRequest;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MessageBase"/> class
		/// that is used for directed messages.
		/// </summary>
		/// <param name="version">The version.</param>
		/// <param name="messageTransport">The message transport.</param>
		/// <param name="recipient">The recipient.</param>
		protected MessageBase(Version version, MessageTransport messageTransport, Uri recipient) {
			Requires.NotNull(version, "version");
			Requires.NotNull(recipient, "recipient");

			this.version = version;
			this.messageTransport = messageTransport;
			this.Recipient = recipient;
			this.HttpMethods = HttpDeliveryMethods.GetRequest | HttpDeliveryMethods.PostRequest;
		}

		#region IMessage Properties

		/// <summary>
		/// Gets the version of the protocol or extension this message is prepared to implement.
		/// </summary>
		/// <remarks>
		/// Implementations of this interface should ensure that this property never returns null.
		/// </remarks>
		Version IMessage.Version {
			get { return this.Version; }
		}

		/// <summary>
		/// Gets the extra, non-standard Protocol parameters included in the message.
		/// </summary>
		/// <value></value>
		/// <remarks>
		/// Implementations of this interface should ensure that this property never returns null.
		/// </remarks>
		public IDictionary<string, string> ExtraData {
			get { return this.extraData; }
		}

		#endregion

		#region IProtocolMessage Members

		/// <summary>
		/// Gets the level of protection this message requires.
		/// </summary>
		/// <value><see cref="MessageProtections.None"/></value>
		MessageProtections IProtocolMessage.RequiredProtection {
			get { return RequiredProtection; }
		}

		/// <summary>
		/// Gets a value indicating whether this is a direct or indirect message.
		/// </summary>
		/// <value></value>
		MessageTransport IProtocolMessage.Transport {
			get { return this.Transport; }
		}

		#endregion

		#region IDirectedProtocolMessage Members

		/// <summary>
		/// Gets the preferred method of transport for the message.
		/// </summary>
		/// <remarks>
		/// For indirect messages this will likely be GET+POST, which both can be simulated in the user agent:
		/// the GET with a simple 301 Redirect, and the POST with an HTML form in the response with javascript
		/// to automate submission.
		/// </remarks>
		HttpDeliveryMethods IDirectedProtocolMessage.HttpMethods {
			get { return this.HttpMethods; }
		}

		/// <summary>
		/// Gets the URL of the intended receiver of this message.
		/// </summary>
		Uri IDirectedProtocolMessage.Recipient {
			get { return this.Recipient; }
		}

		#endregion

		#region IDirectResponseProtocolMessage Members

		/// <summary>
		/// Gets the originating request message that caused this response to be formed.
		/// </summary>
		IDirectedProtocolMessage IDirectResponseProtocolMessage.OriginatingRequest {
			get { return this.OriginatingRequest; }
		}

		#endregion

		/// <summary>
		/// Gets the level of protection this message requires.
		/// </summary>
		protected static MessageProtections RequiredProtection {
			get { return MessageProtections.None; }
		}

		/// <summary>
		/// Gets a value indicating whether this is a direct or indirect message.
		/// </summary>
		protected MessageTransport Transport {
			get { return this.messageTransport; }
		}

		/// <summary>
		/// Gets the version of the protocol or extension this message is prepared to implement.
		/// </summary>
		protected Version Version {
			get { return this.version; }
		}

		/// <summary>
		/// Gets or sets the preferred method of transport for the message.
		/// </summary>
		/// <remarks>
		/// For indirect messages this will likely be GET+POST, which both can be simulated in the user agent:
		/// the GET with a simple 301 Redirect, and the POST with an HTML form in the response with javascript
		/// to automate submission.
		/// </remarks>
		protected HttpDeliveryMethods HttpMethods { get; set; }

		/// <summary>
		/// Gets the originating request message that caused this response to be formed.
		/// </summary>
		protected IDirectedProtocolMessage OriginatingRequest {
			get { return this.originatingRequest; }
		}

		/// <summary>
		/// Gets the URL of the intended receiver of this message.
		/// </summary>
		protected Uri Recipient { get; private set; }

		#region IMessage Methods

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
		void IMessage.EnsureValidMessage() {
			this.EnsureValidMessage();
		}

		#endregion

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
		protected virtual void EnsureValidMessage() {
		}
	}
}
