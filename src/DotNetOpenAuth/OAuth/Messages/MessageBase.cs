//-----------------------------------------------------------------------
// <copyright file="MessageBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
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

	/// <summary>
	/// A base class for all OAuth messages.
	/// </summary>
	public abstract class MessageBase : IDirectedProtocolMessage {
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

#if DEBUG
		/// <summary>
		/// Initializes static members of the <see cref="MessageBase"/> class.
		/// </summary>
		static MessageBase() {
			LowSecurityMode = true;
		}
#endif

		/// <summary>
		/// Initializes a new instance of the <see cref="MessageBase"/> class.
		/// </summary>
		/// <param name="protectionRequired">The level of protection the message requires.</param>
		/// <param name="transport">A value indicating whether this message requires a direct or indirect transport.</param>
		protected MessageBase(MessageProtections protectionRequired, MessageTransport transport) {
			this.protectionRequired = protectionRequired;
			this.transport = transport;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MessageBase"/> class.
		/// </summary>
		/// <param name="protectionRequired">The level of protection the message requires.</param>
		/// <param name="transport">A value indicating whether this message requires a direct or indirect transport.</param>
		/// <param name="recipient">The URI that a directed message will be delivered to.</param>
		protected MessageBase(MessageProtections protectionRequired, MessageTransport transport, MessageReceivingEndpoint recipient) {
			if (recipient == null) {
				throw new ArgumentNullException("recipient");
			}

			this.protectionRequired = protectionRequired;
			this.transport = transport;
			this.recipient = recipient;
		}

		#region IProtocolMessage Properties

		/// <summary>
		/// Gets the version of the protocol this message is prepared to implement.
		/// </summary>
		Version IProtocolMessage.ProtocolVersion {
			get { return this.ProtocolVersion; }
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
		IDictionary<string, string> IProtocolMessage.ExtraData {
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

		/// <summary>
		/// Gets or sets a value indicating whether security sensitive strings are 
		/// emitted from the ToString() method.
		/// </summary>
		internal static bool LowSecurityMode { get; set; }

		/// <summary>
		/// Gets the version of the protocol this message is prepared to implement.
		/// </summary>
		protected virtual Version ProtocolVersion {
			get { return new Version(1, 0); }
		}

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

		#region IProtocolMessage Methods

		/// <summary>
		/// Checks the message state for conformity to the protocol specification
		/// and throws an exception if the message is invalid.
		/// </summary>
		void IProtocolMessage.EnsureValidMessage() {
			this.EnsureValidMessage();
		}

		#endregion

		/// <summary>
		/// Returns a human-friendly string describing the message and all serializable properties.
		/// </summary>
		/// <returns>The string representation of this object.</returns>
		public override string ToString() {
			StringBuilder builder = new StringBuilder();
			builder.AppendFormat(CultureInfo.InvariantCulture, "{0} message", GetType().Name);
			if (this.recipient != null) {
				builder.AppendFormat(CultureInfo.InvariantCulture, " as {0} to {1}", this.recipient.AllowedMethods, this.recipient.Location);
			}
			builder.AppendLine();
			MessageDictionary dictionary = new MessageDictionary(this);
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
		/// Checks the message state for conformity to the protocol specification
		/// and throws an exception if the message is invalid.
		/// </summary>
		protected virtual void EnsureValidMessage() { }
	}
}
