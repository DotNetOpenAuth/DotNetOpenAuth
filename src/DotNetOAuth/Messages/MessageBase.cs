//-----------------------------------------------------------------------
// <copyright file="MessageBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOAuth.Messaging;
	using DotNetOAuth.Messaging.Bindings;

	/// <summary>
	/// A base class for all OAuth messages.
	/// </summary>
	internal abstract class MessageBase : IDirectedProtocolMessage, ITamperResistantOAuthMessage, IExpiringProtocolMessage, IReplayProtectedProtocolMessage {
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
		private Uri recipient;

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
		protected MessageBase(MessageProtection protectionRequired, MessageTransport transport, Uri recipient) {
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
			get { return this.recipient; }
		}

		#endregion

		#region ITamperResistantOAuthMessage Members

		/// <summary>
		/// Gets or sets the message signature.
		/// </summary>
		[MessagePart("oauth_signature")]
		string ITamperResistantProtocolMessage.Signature { get; set; }

		/// <summary>
		/// Gets or sets the signature method used to sign the request.
		/// </summary>
		[MessagePart("oauth_signature_method")]
		string ITamperResistantOAuthMessage.SignatureMethod { get; set; }

		#endregion

		#region IExpiringProtocolMessage Members

		/// <summary>
		/// Gets or sets the OAuth timestamp of the message.
		/// </summary>
		[MessagePart("oauth_timestamp")]
		DateTime IExpiringProtocolMessage.UtcCreationDate { get; set; }

		#endregion

		#region IReplayProtectedProtocolMessage Members

		/// <summary>
		/// Gets or sets the message nonce used for replay detection.
		/// </summary>
		[MessagePart("oauth_nonce")]
		string IReplayProtectedProtocolMessage.Nonce { get; set; }

		#endregion

		/// <summary>
		/// Gets or sets the version of the protocol this message was created with.
		/// </summary>
		/// <remarks>
		/// This property is useful for handling the oauth_version message part.
		/// </remarks>
		protected string VersionString {
			get {
				return ((IProtocolMessage)this).ProtocolVersion.ToString();
			}

			set {
				if (value != this.VersionString) {
					throw new ArgumentOutOfRangeException("value");
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
		/// Checks the message state for conformity to the protocol specification
		/// and throws an exception if the message is invalid.
		/// </summary>
		protected virtual void EnsureValidMessage() { }
	}
}
