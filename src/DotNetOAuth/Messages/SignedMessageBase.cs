//-----------------------------------------------------------------------
// <copyright file="SignedMessageBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messages {
	using System;
	using DotNetOAuth.ChannelElements;
	using DotNetOAuth.Messaging;
	using DotNetOAuth.Messaging.Bindings;

	/// <summary>
	/// A base class for all signed OAuth messages.
	/// </summary>
	internal class SignedMessageBase : MessageBase, ITamperResistantOAuthMessage, IExpiringProtocolMessage, IReplayProtectedProtocolMessage {
		/// <summary>
		/// Initializes a new instance of the <see cref="SignedMessageBase"/> class.
		/// </summary>
		/// <param name="transport">A value indicating whether this message requires a direct or indirect transport.</param>
		internal SignedMessageBase(MessageTransport transport)
			: base(MessageProtection.All, transport) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SignedMessageBase"/> class.
		/// </summary>
		/// <param name="transport">A value indicating whether this message requires a direct or indirect transport.</param>
		/// <param name="recipient">The URI that a directed message will be delivered to.</param>
		internal SignedMessageBase(MessageTransport transport, Uri recipient)
			: base(MessageProtection.All, transport, recipient) {
		}

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
		[MessagePart(Name = "oauth_version", IsRequired = false)]
		private string Version {
			get {
				return ((IProtocolMessage)this).ProtocolVersion.ToString();
			}

			set {
				if (value != this.Version) {
					throw new ArgumentOutOfRangeException("value");
				}
			}
		}
	}
}
