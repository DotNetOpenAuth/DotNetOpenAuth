//-----------------------------------------------------------------------
// <copyright file="CoordinatingHttpRequestInfo.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;
using DotNetOpenAuth.Messaging;

	internal class CoordinatingHttpRequestInfo : HttpRequestInfo {
		private readonly Channel channel;

		private readonly IDictionary<string, string> messageData;

		private readonly IMessageFactory messageFactory;

		private readonly MessageReceivingEndpoint recipient;

		private IDirectedProtocolMessage message;

		/// <summary>
		/// Initializes a new instance of the <see cref="CoordinatingHttpRequestInfo"/> class
		/// that will generate a message when the <see cref="Message"/> property getter is called.
		/// </summary>
		/// <param name="channel">The channel.</param>
		/// <param name="messageFactory">The message factory.</param>
		/// <param name="messageData">The message data.</param>
		/// <param name="recipient">The recipient.</param>
		internal CoordinatingHttpRequestInfo(
			Channel channel,
			IMessageFactory messageFactory,
			IDictionary<string, string> messageData,
			MessageReceivingEndpoint recipient)
			: this(recipient) {
			Contract.Requires(channel != null);
			Contract.Requires(messageFactory != null);
			Contract.Requires(messageData != null);
			this.channel = channel;
			this.messageData = messageData;
			this.messageFactory = messageFactory;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CoordinatingHttpRequestInfo"/> class
		/// that will not generate any message.
		/// </summary>
		/// <param name="recipient">The recipient.</param>
		internal CoordinatingHttpRequestInfo(MessageReceivingEndpoint recipient)
			: base(GetHttpVerb(recipient), recipient != null ? recipient.Location : new Uri("http://host/path")) {
			this.recipient = recipient;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CoordinatingHttpRequestInfo"/> class.
		/// </summary>
		/// <param name="message">The message being passed in through a mock transport.  May be null.</param>
		/// <param name="httpMethod">The HTTP method that the incoming request came in on, whether or not <paramref name="message"/> is null.</param>
		internal CoordinatingHttpRequestInfo(IDirectedProtocolMessage message, HttpDeliveryMethods httpMethod)
			: base(GetHttpVerb(httpMethod), message.Recipient) {
			this.message = message;
		}

		/// <summary>
		/// Gets the message deserialized from the remote channel.
		/// </summary>
		internal IDirectedProtocolMessage Message {
			get {
				if (this.message == null && this.messageData != null) {
					var message = this.messageFactory.GetNewRequestMessage(this.recipient, this.messageData);
					if (message != null) {
						this.channel.MessageDescriptions.GetAccessor(message).Deserialize(this.messageData);
						this.message = message;
					}
				}

				return this.message;
			}
		}

		private static string GetHttpVerb(MessageReceivingEndpoint recipient) {
			if (recipient == null) {
				return "GET";
			}

			return GetHttpVerb(recipient.AllowedMethods);
		}

		private static string GetHttpVerb(HttpDeliveryMethods httpMethod) {
			if ((httpMethod & HttpDeliveryMethods.GetRequest) != 0) {
				return "GET";
			}

			if ((httpMethod & HttpDeliveryMethods.PostRequest) != 0) {
				return "POST";
			}

			throw new ArgumentOutOfRangeException();
		}
	}
}
