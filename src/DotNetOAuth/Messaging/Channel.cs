//-----------------------------------------------------------------------
// <copyright file="Channel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messaging {
	using System;
	using System.Text;

	/// <summary>
	/// Manages sending direct messages to a remote party and receiving responses.
	/// </summary>
	internal class Channel {
		/// <summary>
		/// Gets or sets the message that came in as a request, if any.
		/// </summary>
		/// <remarks>
		/// This message is used to help determine how to transmit the response.
		/// </remarks>
		internal IProtocolMessage RequestInProcess { get; set; }

		/// <summary>
		/// Gets or sets the HTTP response to send as a reply to the current incoming HTTP request.
		/// </summary>
		internal Response QueuedIndirectOrResponseMessage { get; set; }

		/// <summary>
		/// Sends a direct message to a remote party and waits for the response.
		/// </summary>
		/// <param name="request">The message to send.</param>
		/// <returns>The remote party's response.</returns>
		internal IProtocolMessage Request(IDirectedProtocolMessage request) {
			if (request == null) {
				throw new ArgumentNullException("request");
			}

			MessageScheme transmissionMethod = MessageScheme.AuthorizationHeaderRequest;
			switch (transmissionMethod) {
				case MessageScheme.AuthorizationHeaderRequest:
					throw new NotImplementedException();
					break;
				case MessageScheme.PostRequest:
					throw new NotImplementedException();
					break;
				case MessageScheme.GetRequest:
					throw new NotImplementedException();
					break;
				default:
					throw new NotSupportedException();
			}
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
			if (this.QueuedIndirectOrResponseMessage != null) {
				throw new InvalidOperationException(MessagingStrings.QueuedMessageResponseAlreadyExists);
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
					if (message is ProtocolException) {
						if (this.RequestInProcess is IDirectedProtocolMessage) {
							this.ReportErrorAsDirectResponse(directedMessage);
						} else {
							this.ReportErrorToUser(directedMessage);
						}
					}
				}
			}
		}

		private void SendIndirectMessage(IDirectedProtocolMessage directedMessage) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Queues a message for sending in the response stream where the fields
		/// are sent in the response stream in querystring style.
		/// </summary>
		/// <param name="response">The message to send as a response.</param>
		/// <remarks>
		/// This method implements spec V1.0 section 5.3.
		/// </remarks>
		private void SendDirectMessageResponse(IProtocolMessage response) {
			MessageSerializer serializer = MessageSerializer.Get(response.GetType());
			var fields = serializer.Serialize(response);
			string responseBody = MessagingUtilities.CreateQueryString(fields);

			Response encodedResponse = new Response {
				Body = Encoding.UTF8.GetBytes(responseBody),
				OriginalMessage = response,
				Status = System.Net.HttpStatusCode.OK,
				Headers = new System.Net.WebHeaderCollection(),
			};
			this.QueuedIndirectOrResponseMessage = encodedResponse;
		}

		private void ReportErrorToUser(IDirectedProtocolMessage directedMessage) {
			throw new NotImplementedException();
		}

		private void ReportErrorAsDirectResponse(IDirectedProtocolMessage directedMessage) {
			throw new NotImplementedException();
		}
	}
}
