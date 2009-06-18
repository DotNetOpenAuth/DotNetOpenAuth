//-----------------------------------------------------------------------
// <copyright file="CoordinatingHttpRequestInfo.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.Messaging;

	internal class CoordinatingHttpRequestInfo : HttpRequestInfo {
		private IDictionary<string, string> messageData;
		private IMessageFactory messageFactory;
		private MessageReceivingEndpoint recipient;
		private Channel channel;

		/// <summary>
		/// Initializes a new instance of the <see cref="CoordinatingHttpRequestInfo"/> class
		/// that will generate a message when the <see cref="Message"/> property getter is called.
		/// </summary>
		/// <param name="channel">The channel.</param>
		/// <param name="messageFactory">The message factory.</param>
		/// <param name="messageData">The message data.</param>
		/// <param name="recipient">The recipient.</param>
		internal CoordinatingHttpRequestInfo(Channel channel, IMessageFactory messageFactory, IDictionary<string, string> messageData, MessageReceivingEndpoint recipient)
			: this(recipient) {
			Contract.Requires(channel != null);
			Contract.Requires(messageFactory != null);
			Contract.Requires(messageData != null);
			this.channel = channel;
			this.messageFactory = messageFactory;
			this.messageData = messageData;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CoordinatingHttpRequestInfo"/> class
		/// that will not generate any message.
		/// </summary>
		/// <param name="recipient">The recipient.</param>
		internal CoordinatingHttpRequestInfo(MessageReceivingEndpoint recipient) {
			this.recipient = recipient;
			if (recipient != null) {
				this.UrlBeforeRewriting = recipient.Location;
			}

			if (recipient == null || (recipient.AllowedMethods & HttpDeliveryMethods.GetRequest) != 0) {
				this.HttpMethod = "GET";
			} else if ((recipient.AllowedMethods & HttpDeliveryMethods.PostRequest) != 0) {
				this.HttpMethod = "POST";
			}
		}

		internal override IDirectedProtocolMessage Message {
			get {
				if (base.Message == null && this.messageData != null) {
					IDirectedProtocolMessage message = this.messageFactory.GetNewRequestMessage(this.recipient, this.messageData);
					if (message != null) {
						this.channel.MessageDescriptions.GetAccessor(message).Deserialize(this.messageData);
					}
					base.Message = message;
				}

				return base.Message;
			}

			set {
				base.Message = value;
			}
		}
	}
}
