//-----------------------------------------------------------------------
// <copyright file="Channel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Net;
	using System.Text;
	using System.Web;

	/// <summary>
	/// Manages sending direct messages to a remote party and receiving responses.
	/// </summary>
	internal abstract class Channel {
		/// <summary>
		/// A tool that can figure out what kind of message is being received
		/// so it can be deserialized.
		/// </summary>
		private IMessageTypeProvider messageTypeProvider;

		/// <summary>
		/// Gets or sets the HTTP response to send as a reply to the current incoming HTTP request.
		/// </summary>
		private Response queuedIndirectOrResponseMessage;

		/// <summary>
		/// Initializes a new instance of the <see cref="Channel"/> class.
		/// </summary>
		/// <param name="messageTypeProvider">
		/// A class prepared to analyze incoming messages and indicate what concrete
		/// message types can deserialize from it.
		/// </param>
		protected Channel(IMessageTypeProvider messageTypeProvider) {
			if (messageTypeProvider == null) {
				throw new ArgumentNullException("messageTypeProvider");
			}

			this.messageTypeProvider = messageTypeProvider;
		}

		/// <summary>
		/// Gets or sets the message that came in as a request, if any.
		/// </summary>
		/// <remarks>
		/// This message is used to help determine how to transmit the response.
		/// </remarks>
		internal IProtocolMessage RequestInProcess { get; set; }

		/// <summary>
		/// Gets a tool that can figure out what kind of message is being received
		/// so it can be deserialized.
		/// </summary>
		protected IMessageTypeProvider MessageTypeProvider {
			get { return this.messageTypeProvider; }
		}

		/// <summary>
		/// Retrieves the stored response for sending and clears it from the channel.
		/// </summary>
		/// <returns>The response to send as the HTTP response.</returns>
		internal Response DequeueIndirectOrResponseMessage() {
			Response response = this.queuedIndirectOrResponseMessage;
			this.queuedIndirectOrResponseMessage = null;
			return response;
		}

		/// <summary>
		/// Queues an indirect message (either a request or response) 
		/// or direct message response for transmission to a remote party.
		/// </summary>
		/// <param name="message">The one-way message to send</param>
		internal void Send(IProtocolMessage message) {
			if (message == null) {
				throw new ArgumentNullException("message");
			}

			var directedMessage = message as IDirectedProtocolMessage;
			if (directedMessage == null) {
				// This is a response to a direct message.
				this.SendDirectMessageResponse(message);
			} else {
				if (directedMessage.Recipient != null) {
					// This is an indirect message request or reply.
					this.SendIndirectMessage(directedMessage);
				} else {
					ProtocolException exception = message as ProtocolException;
					if (exception != null) {
						if (this.RequestInProcess is IDirectedProtocolMessage) {
							this.ReportErrorAsDirectResponse(exception);
						} else {
							this.ReportErrorToUser(exception);
						}
					} else {
						throw new InvalidOperationException();
					}
				}
			}
		}

		/// <summary>
		/// Takes a message and temporarily stores it for sending as the hosting site's
		/// HTTP response to the current request.
		/// </summary>
		/// <param name="response">The message to store for sending.</param>
		protected void QueueIndirectOrResponseMessage(Response response) {
			if (response == null) {
				throw new ArgumentNullException("response");
			}
			if (this.queuedIndirectOrResponseMessage != null) {
				throw new InvalidOperationException(MessagingStrings.QueuedMessageResponseAlreadyExists);
			}

			this.queuedIndirectOrResponseMessage = response;
		}

		/// <summary>
		/// Sends a direct message to a remote party and waits for the response.
		/// </summary>
		/// <param name="request">The message to send.</param>
		/// <returns>The remote party's response.</returns>
		protected abstract IProtocolMessage Request(IDirectedProtocolMessage request);

		/// <summary>
		/// Queues an indirect message for transmittal via the user agent.
		/// </summary>
		/// <param name="message">The message to send.</param>
		protected abstract void SendIndirectMessage(IDirectedProtocolMessage message);

		/// <summary>
		/// Queues a message for sending in the response stream where the fields
		/// are sent in the response stream in querystring style.
		/// </summary>
		/// <param name="response">The message to send as a response.</param>
		/// <remarks>
		/// This method implements spec V1.0 section 5.3.
		/// </remarks>
		protected abstract void SendDirectMessageResponse(IProtocolMessage response);

		/// <summary>
		/// Reports an error to the user via the user agent.
		/// </summary>
		/// <param name="exception">The error information.</param>
		protected abstract void ReportErrorToUser(ProtocolException exception);

		/// <summary>
		/// Sends an error result directly to the calling remote party according to the
		/// rules of the protocol.
		/// </summary>
		/// <param name="exception">The error information.</param>
		protected abstract void ReportErrorAsDirectResponse(ProtocolException exception);
	}
}
