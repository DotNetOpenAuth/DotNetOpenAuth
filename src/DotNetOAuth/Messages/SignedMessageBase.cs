//-----------------------------------------------------------------------
// <copyright file="SignedMessageBase.cs" company="Andrew Arnott">
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
	/// A base class for all signed OAuth messages.
	/// </summary>
	internal class SignedMessageBase : MessageBase, ITamperResistantOAuthMessage, IExpiringProtocolMessage, IReplayProtectedProtocolMessage {
		/// <summary>
		/// The reference date and time for calculating time stamps.
		/// </summary>
		private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>
		/// The number of seconds since 1/1/1970, consistent with the OAuth timestamp requirement.
		/// </summary>
		[MessagePart("oauth_timestamp", IsRequired = true)]
		private long timestamp;

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
		internal SignedMessageBase(MessageTransport transport, ServiceProviderEndpoint recipient)
			: base(MessageProtection.All, transport, recipient) {
		}

		#region ITamperResistantOAuthMessage Members

		/// <summary>
		/// Gets or sets the signature method used to sign the request.
		/// </summary>
		[MessagePart("oauth_signature_method", IsRequired = true)]
		string ITamperResistantOAuthMessage.SignatureMethod { get; set; }

		/// <summary>
		/// Gets or sets the Token Secret used to sign the message.
		/// Only applicable to Consumer.
		/// </summary>
		public string TokenSecret { get; set; }

		/// <summary>
		/// Gets or sets the Consumer Secret used to sign the message.
		/// Only applicable to Consumer.
		/// </summary>
		public string ConsumerSecret { get; set; }

		/// <summary>
		/// Gets or sets the HTTP method that will be used to transmit the message.
		/// Only applicable to Consumer.
		/// </summary>
		string ITamperResistantOAuthMessage.HttpMethod { get; set; }

		/// <summary>
		/// Gets or sets the extra, non-OAuth parameters that will be included in the request.
		/// Only applicable to Consumer.
		/// </summary>
		IDictionary<string, string> ITamperResistantOAuthMessage.AdditionalParametersInHttpRequest { get; set; }

		#endregion

		#region ITamperResistantProtocolMessage Members

		/// <summary>
		/// Gets or sets the message signature.
		/// </summary>
		[MessagePart("oauth_signature", IsRequired = true)]
		string ITamperResistantProtocolMessage.Signature { get; set; }

		#endregion

		#region IExpiringProtocolMessage Members

		/// <summary>
		/// Gets or sets the OAuth timestamp of the message.
		/// </summary>
		DateTime IExpiringProtocolMessage.UtcCreationDate {
			get { return epoch + TimeSpan.FromSeconds(this.timestamp); }
			set { this.timestamp = (long)(value - epoch).TotalSeconds; }
		}

		#endregion

		#region IReplayProtectedProtocolMessage Members

		/// <summary>
		/// Gets or sets the message nonce used for replay detection.
		/// </summary>
		[MessagePart("oauth_nonce", IsRequired = true)]
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
