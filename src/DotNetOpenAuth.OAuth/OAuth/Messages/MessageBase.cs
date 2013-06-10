//-----------------------------------------------------------------------
// <copyright file="MessageBase.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.Messages {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using Validation;

	/// <summary>
	/// A base class for all OAuth messages.
	/// </summary>
	[Serializable]
	public abstract class MessageBase : IDirectedProtocolMessage, IDirectResponseProtocolMessage {
		/// <summary>
		/// A store for extra name/value data pairs that are attached to this message.
		/// </summary>
		private Dictionary<string, string> extraData = new Dictionary<string, string>();

		/// <summary>
		/// Gets a value indicating whether signing this message is required.
		/// </summary>
		private MessageProtections protectionRequired;

		/// <summary>
		/// Gets a value indicating whether this is a direct or indirect message.
		/// </summary>
		private MessageTransport transport;

		/// <summary>
		/// The URI to the remote endpoint to send this message to.
		/// </summary>
		private MessageReceivingEndpoint recipient;

		/// <summary>
		/// Backing store for the <see cref="OriginatingRequest"/> properties.
		/// </summary>
		private IDirectedProtocolMessage originatingRequest;

		/// <summary>
		/// Backing store for the <see cref="Incoming"/> properties.
		/// </summary>
		private bool incoming;

#if DEBUG
		/// <summary>
		/// Initializes static members of the <see cref="MessageBase"/> class.
		/// </summary>
		static MessageBase() {
			LowSecurityMode = true;
		}
#endif

		/// <summary>
		/// Initializes a new instance of the <see cref="MessageBase"/> class for direct response messages.
		/// </summary>
		/// <param name="protectionRequired">The level of protection the message requires.</param>
		/// <param name="originatingRequest">The request that asked for this direct response.</param>
		/// <param name="version">The OAuth version.</param>
		protected MessageBase(MessageProtections protectionRequired, IDirectedProtocolMessage originatingRequest, Version version) {
			Requires.NotNull(originatingRequest, "originatingRequest");
			Requires.NotNull(version, "version");

			this.protectionRequired = protectionRequired;
			this.transport = MessageTransport.Direct;
			this.originatingRequest = originatingRequest;
			this.Version = version;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MessageBase"/> class for direct requests or indirect messages.
		/// </summary>
		/// <param name="protectionRequired">The level of protection the message requires.</param>
		/// <param name="transport">A value indicating whether this message requires a direct or indirect transport.</param>
		/// <param name="recipient">The URI that a directed message will be delivered to.</param>
		/// <param name="version">The OAuth version.</param>
		protected MessageBase(MessageProtections protectionRequired, MessageTransport transport, MessageReceivingEndpoint recipient, Version version) {
			Requires.NotNull(recipient, "recipient");
			Requires.NotNull(version, "version");

			this.protectionRequired = protectionRequired;
			this.transport = transport;
			this.recipient = recipient;
			this.Version = version;
		}

		#region IProtocolMessage Properties

		/// <summary>
		/// Gets the version of the protocol this message is prepared to implement.
		/// </summary>
		Version IMessage.Version {
			get { return this.Version; }
		}

		/// <summary>
		/// Gets the level of protection this message requires.
		/// </summary>
		MessageProtections IProtocolMessage.RequiredProtection {
			get { return this.RequiredProtection; }
		}

		/// <summary>
		/// Gets a value indicating whether this is a direct or indirect message.
		/// </summary>
		MessageTransport IProtocolMessage.Transport {
			get { return this.Transport; }
		}

		/// <summary>
		/// Gets the dictionary of additional name/value fields tacked on to this message.
		/// </summary>
		IDictionary<string, string> IMessage.ExtraData {
			get { return this.ExtraData; }
		}

		#endregion

		#region IDirectedProtocolMessage Members

		/// <summary>
		/// Gets the URI to the Service Provider endpoint to send this message to.
		/// </summary>
		Uri IDirectedProtocolMessage.Recipient {
			get { return this.recipient != null ? this.recipient.Location : null; }
		}

		/// <summary>
		/// Gets the preferred method of transport for the message.
		/// </summary>
		HttpDeliveryMethods IDirectedProtocolMessage.HttpMethods {
			get { return this.HttpMethods; }
		}

		#endregion

		#region IDirectResponseProtocolMessage Members

		/// <summary>
		/// Gets the originating request message that caused this response to be formed.
		/// </summary>
		IDirectedProtocolMessage IDirectResponseProtocolMessage.OriginatingRequest {
			get { return this.originatingRequest; }
		}

		#endregion

		/// <summary>
		/// Gets or sets a value indicating whether security sensitive strings are 
		/// emitted from the ToString() method.
		/// </summary>
		internal static bool LowSecurityMode { get; set; }

		/// <summary>
		/// Gets a value indicating whether this message was deserialized as an incoming message.
		/// </summary>
		protected internal bool Incoming {
			get { return this.incoming; }
		}

		/// <summary>
		/// Gets the version of the protocol this message is prepared to implement.
		/// </summary>
		protected internal Version Version { get; private set; }

		/// <summary>
		/// Gets the level of protection this message requires.
		/// </summary>
		protected MessageProtections RequiredProtection {
			get { return this.protectionRequired; }
		}

		/// <summary>
		/// Gets a value indicating whether this is a direct or indirect message.
		/// </summary>
		protected MessageTransport Transport {
			get { return this.transport; }
		}

		/// <summary>
		/// Gets the dictionary of additional name/value fields tacked on to this message.
		/// </summary>
		protected IDictionary<string, string> ExtraData {
			get { return this.extraData; }
		}

		/// <summary>
		/// Gets the preferred method of transport for the message.
		/// </summary>
		protected HttpDeliveryMethods HttpMethods {
			get { return this.recipient != null ? this.recipient.AllowedMethods : HttpDeliveryMethods.None; }
		}

		/// <summary>
		/// Gets or sets the URI to the Service Provider endpoint to send this message to.
		/// </summary>
		protected Uri Recipient {
			get {
				return this.recipient != null ? this.recipient.Location : null;
			}

			set {
				if (this.recipient != null) {
					this.recipient = new MessageReceivingEndpoint(value, this.recipient.AllowedMethods);
				} else if (value != null) {
					throw new InvalidOperationException();
				}
			}
		}

		/// <summary>
		/// Gets the originating request message that caused this response to be formed.
		/// </summary>
		protected IDirectedProtocolMessage OriginatingRequest {
			get { return this.originatingRequest; }
		}

		#region IProtocolMessage Methods

		/// <summary>
		/// Checks the message state for conformity to the protocol specification
		/// and throws an exception if the message is invalid.
		/// </summary>
		void IMessage.EnsureValidMessage() {
			this.EnsureValidMessage();
		}

		#endregion

		/// <summary>
		/// Returns a human-friendly string describing the message and all serializable properties.
		/// </summary>
		/// <param name="channel">The channel that will carry this message.</param>
		/// <returns>
		/// The string representation of this object.
		/// </returns>
		internal virtual string ToString(Channel channel) {
			Requires.NotNull(channel, "channel");

			StringBuilder builder = new StringBuilder();
			builder.AppendFormat(CultureInfo.InvariantCulture, "{0} message", GetType().Name);
			if (this.recipient != null) {
				builder.AppendFormat(CultureInfo.InvariantCulture, " as {0} to {1}", this.recipient.AllowedMethods, this.recipient.Location);
			}
			builder.AppendLine();
			MessageDictionary dictionary = channel.MessageDescriptions.GetAccessor(this);
			foreach (var pair in dictionary) {
				string value = pair.Value;
				if (pair.Key == "oauth_signature" && !LowSecurityMode) {
					value = "xxxxxxxxxxxxx (not shown)";
				}
				builder.Append('\t');
				builder.Append(pair.Key);
				builder.Append(": ");
				builder.AppendLine(value);
			}

			return builder.ToString();
		}

		/// <summary>
		/// Sets a flag indicating that this message is received (as opposed to sent).
		/// </summary>
		internal void SetAsIncoming() {
			this.incoming = true;
		}

		/// <summary>
		/// Checks the message state for conformity to the protocol specification
		/// and throws an exception if the message is invalid.
		/// </summary>
		protected virtual void EnsureValidMessage() { }
	}
}
