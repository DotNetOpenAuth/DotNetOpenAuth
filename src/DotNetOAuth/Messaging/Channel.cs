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
	internal class Channel {
		IMessageTypeProvider messageTypeProvider;

		/// <summary>
		/// Initializes a new instance of the <see cref="Channel"/> class.
		/// </summary>
		/// <param name="messageTypeProvider">
		/// A class prepared to analyze incoming messages and indicate what concrete
		/// message types can deserialize from it.
		/// </param>
		internal Channel(IMessageTypeProvider messageTypeProvider) {
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

			HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(request.Recipient);

			MessageScheme transmissionMethod = MessageScheme.AuthorizationHeaderRequest;
			switch (transmissionMethod) {
				case MessageScheme.AuthorizationHeaderRequest:
					this.InitializeRequestAsAuthHeader(httpRequest, request);
					break;
				case MessageScheme.PostRequest:
					this.InitializeRequestAsPost(httpRequest, request);
					break;
				case MessageScheme.GetRequest:
					this.InitializeRequestAsGet(httpRequest, request);
					break;
				default:
					throw new NotSupportedException();
			}

			// Submit the request and await the reply.
			Dictionary<string, string> responseFields;
			try {
				using (HttpWebResponse response = (HttpWebResponse)httpRequest.GetResponse()) {
					using (StreamReader reader = new StreamReader(response.GetResponseStream())) {
						string queryString = reader.ReadToEnd();
						responseFields = HttpUtility.ParseQueryString(queryString).ToDictionary();
					}
				}
			} catch (WebException ex) {
				throw new ProtocolException(MessagingStrings.ErrorInRequestReplyMessage, ex);
			}

			Type messageType = this.messageTypeProvider.GetMessageType(responseFields);
			var responseSerialize = MessageSerializer.Get(messageType);
			var responseMessage = responseSerialize.Deserialize(responseFields);

			return responseMessage;
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

		private void InitializeRequestAsAuthHeader(HttpWebRequest httpRequest, IDirectedProtocolMessage requestMessage) {
			throw new NotImplementedException();
		}

		private void InitializeRequestAsPost(HttpWebRequest httpRequest, IDirectedProtocolMessage requestMessage) {
			throw new NotImplementedException();
		}

		private void InitializeRequestAsGet(HttpWebRequest httpRequest, IDirectedProtocolMessage requestMessage) {
			throw new NotImplementedException();
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
