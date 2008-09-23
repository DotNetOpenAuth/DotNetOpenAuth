//-----------------------------------------------------------------------
// <copyright file="MessageBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messages {
	using System;
	using System.Collections.Generic;
	using DotNetOAuth.ChannelElements;
	using DotNetOAuth.Messaging;
	using DotNetOAuth.Messaging.Bindings;

	/// <summary>
	/// A base class for all OAuth messages.
	/// </summary>
	internal abstract class MessageBase : IOAuthDirectedMessage {
		/// <summary>
		/// A store for extra name/value data pairs that are attached to this message.
		/// </summary>
		private Dictionary<string, string> extraData = new Dictionary<string, string>();

		/// <summary>
		/// Gets a value indicating whether signing this message is required.
		/// </summary>
		private MessageProtection protectionRequired;

		/// <summary>
		/// Gets a value indicating whether this is a direct or indirect message.
		/// </summary>
		private MessageTransport transport;

		/// <summary>
		/// The URI to the remote endpoint to send this message to.
		/// </summary>
		private ServiceProviderEndpoint recipient;

		/// <summary>
		/// Initializes a new instance of the <see cref="MessageBase"/> class.
		/// </summary>
		/// <param name="protectionRequired">The level of protection the message requires.</param>
		/// <param name="transport">A value indicating whether this message requires a direct or indirect transport.</param>
		protected MessageBase(MessageProtection protectionRequired, MessageTransport transport) {
			this.protectionRequired = protectionRequired;
			this.transport = transport;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MessageBase"/> class.
		/// </summary>
		/// <param name="protectionRequired">The level of protection the message requires.</param>
		/// <param name="transport">A value indicating whether this message requires a direct or indirect transport.</param>
		/// <param name="recipient">The URI that a directed message will be delivered to.</param>
		protected MessageBase(MessageProtection protectionRequired, MessageTransport transport, ServiceProviderEndpoint recipient) {
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
			get { return new Version(1, 0); }
		}

		/// <summary>
		/// Gets the level of protection this message requires.
		/// </summary>
		MessageProtection IProtocolMessage.RequiredProtection {
			get { return this.protectionRequired; }
		}

		/// <summary>
		/// Gets a value indicating whether this is a direct or indirect message.
		/// </summary>
		MessageTransport IProtocolMessage.Transport {
			get { return this.transport; }
		}

		/// <summary>
		/// Gets the dictionary of additional name/value fields tacked on to this message.
		/// </summary>
		IDictionary<string, string> IProtocolMessage.ExtraData {
			get { return this.extraData; }
		}

		#endregion

		#region IDirectedProtocolMessage Members

		/// <summary>
		/// Gets the URI to the Service Provider endpoint to send this message to.
		/// </summary>
		Uri IDirectedProtocolMessage.Recipient {
			get { return this.recipient.Location; }
		}

		#endregion

		#region IOAuthDirectedMessage Properties

		/// <summary>
		/// Gets the preferred method of transport for the message.
		/// </summary>
		HttpDeliveryMethod IOAuthDirectedMessage.HttpMethods {
			get { return this.recipient.AllowedMethods; }
		}

		#endregion

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
		/// Checks the message state for conformity to the protocol specification
		/// and throws an exception if the message is invalid.
		/// </summary>
		protected virtual void EnsureValidMessage() { }
	}
}
