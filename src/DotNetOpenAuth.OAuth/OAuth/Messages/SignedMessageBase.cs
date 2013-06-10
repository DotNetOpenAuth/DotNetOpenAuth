//-----------------------------------------------------------------------
// <copyright file="SignedMessageBase.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.Messages {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Net.Http;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OAuth.ChannelElements;

	/// <summary>
	/// A base class for all signed OAuth messages.
	/// </summary>
	public class SignedMessageBase : MessageBase, ITamperResistantOAuthMessage, IExpiringProtocolMessage, IReplayProtectedProtocolMessage {
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
		/// <param name="recipient">The URI that a directed message will be delivered to.</param>
		/// <param name="version">The OAuth version.</param>
		internal SignedMessageBase(MessageTransport transport, MessageReceivingEndpoint recipient, Version version)
			: base(MessageProtections.All, transport, recipient, version) {
			ITamperResistantOAuthMessage self = (ITamperResistantOAuthMessage)this;
			HttpDeliveryMethods methods = ((IDirectedProtocolMessage)this).HttpMethods;
			self.HttpMethod = MessagingUtilities.GetHttpVerb(methods);
		}

		#region ITamperResistantOAuthMessage Members

		/// <summary>
		/// Gets or sets the signature method used to sign the request.
		/// </summary>
		string ITamperResistantOAuthMessage.SignatureMethod {
			get { return this.SignatureMethod; }
			set { this.SignatureMethod = value; }
		}

		/// <summary>
		/// Gets or sets the Token Secret used to sign the message.
		/// </summary>
		string ITamperResistantOAuthMessage.TokenSecret {
			get { return this.TokenSecret; }
			set { this.TokenSecret = value; }
		}

		/// <summary>
		/// Gets or sets the Consumer key.
		/// </summary>
		[MessagePart("oauth_consumer_key", IsRequired = true)]
		public string ConsumerKey { get; set; }

		/// <summary>
		/// Gets or sets the Consumer Secret used to sign the message.
		/// </summary>
		string ITamperResistantOAuthMessage.ConsumerSecret {
			get { return this.ConsumerSecret; }
			set { this.ConsumerSecret = value; }
		}

		/// <summary>
		/// Gets or sets the HTTP method that will be used to transmit the message.
		/// </summary>
		HttpMethod ITamperResistantOAuthMessage.HttpMethod {
			get { return this.HttpMethod; }
			set { this.HttpMethod = value; }
		}

		/// <summary>
		/// Gets or sets the URI to the Service Provider endpoint to send this message to.
		/// </summary>
		Uri ITamperResistantOAuthMessage.Recipient {
			get { return this.Recipient; }
			set { this.Recipient = value; }
		}

		#endregion

		#region ITamperResistantProtocolMessage Members

		/// <summary>
		/// Gets or sets the message signature.
		/// </summary>
		string ITamperResistantProtocolMessage.Signature {
			get { return this.Signature; }
			set { this.Signature = value; }
		}

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
		/// Gets the context within which the nonce must be unique.
		/// </summary>
		/// <value>The consumer key.</value>
		string IReplayProtectedProtocolMessage.NonceContext {
			get { return this.ConsumerKey; }
		}

		/// <summary>
		/// Gets or sets the message nonce used for replay detection.
		/// </summary>
		[MessagePart("oauth_nonce", IsRequired = true)]
		string IReplayProtectedProtocolMessage.Nonce { get; set; }

		#endregion

		#region IMessageOriginalPayload Members

		/// <summary>
		/// Gets or sets the original message parts, before any normalization or default values were assigned.
		/// </summary>
		IDictionary<string, string> IMessageOriginalPayload.OriginalPayload {
			get { return this.OriginalPayload; }
			set { this.OriginalPayload = value; }
		}

		/// <summary>
		/// Gets or sets the original message parts, before any normalization or default values were assigned.
		/// </summary>
		[SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "By design")]
		protected IDictionary<string, string> OriginalPayload { get; set; }

		#endregion

		/// <summary>
		/// Gets or sets the signature method used to sign the request.
		/// </summary>
		[MessagePart("oauth_signature_method", IsRequired = true)]
		protected string SignatureMethod { get; set; }

		/// <summary>
		/// Gets or sets the Token Secret used to sign the message.
		/// </summary>
		protected string TokenSecret { get; set; }

		/// <summary>
		/// Gets or sets the Consumer Secret used to sign the message.
		/// </summary>
		protected string ConsumerSecret { get; set; }

		/// <summary>
		/// Gets or sets the HTTP method that will be used to transmit the message.
		/// </summary>
		protected HttpMethod HttpMethod { get; set; }

		/// <summary>
		/// Gets or sets the message signature.
		/// </summary>
		[MessagePart("oauth_signature", IsRequired = true)]
		protected string Signature { get; set; }

		/// <summary>
		/// Gets or sets the version of the protocol this message was created with.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Accessed via reflection.")]
		[MessagePart("oauth_version", IsRequired = false)]
		private string OAuthVersion {
			get {
				return Protocol.Lookup(Version).PublishedVersion;
			}

			set {
				if (value != this.OAuthVersion) {
					throw new ArgumentOutOfRangeException("value");
				}
			}
		}
	}
}
