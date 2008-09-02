//-----------------------------------------------------------------------
// <copyright file="OAuthChannel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Net;
	using System.Text;
	using System.Web;
	using DotNetOAuth.Messaging;

	internal class OAuthChannel : Channel {
		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthChannel"/> class.
		/// </summary>
		/// <param name="messageTypeProvider">
		/// A class prepared to analyze incoming messages and indicate what concrete
		/// message types can deserialize from it.
		/// </param>
		internal OAuthChannel(IMessageTypeProvider messageTypeProvider)
			: base(messageTypeProvider) {
		}

		protected override void SendDirectMessageResponse(IProtocolMessage response) {
			MessageSerializer serializer = MessageSerializer.Get(response.GetType());
			var fields = serializer.Serialize(response);
			string responseBody = MessagingUtilities.CreateQueryString(fields);

			Response encodedResponse = new Response {
				Body = Encoding.UTF8.GetBytes(responseBody),
				OriginalMessage = response,
				Status = System.Net.HttpStatusCode.OK,
				Headers = new System.Net.WebHeaderCollection(),
			};
			this.QueueIndirectOrResponseMessage(encodedResponse);
		}

		protected override IProtocolMessage Request(IDirectedProtocolMessage request) {
			if (request == null) {
				throw new ArgumentNullException("request");
			}

			HttpWebRequest httpRequest;

			MessageScheme transmissionMethod = MessageScheme.AuthorizationHeaderRequest;
			switch (transmissionMethod) {
				case MessageScheme.AuthorizationHeaderRequest:
					httpRequest = this.InitializeRequestAsAuthHeader(request);
					break;
				case MessageScheme.PostRequest:
					httpRequest = this.InitializeRequestAsPost(request);
					break;
				case MessageScheme.GetRequest:
					httpRequest = this.InitializeRequestAsGet(request);
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

			Type messageType = this.MessageTypeProvider.GetResponseMessageType(request, responseFields);
			var responseSerialize = MessageSerializer.Get(messageType);
			var responseMessage = responseSerialize.Deserialize(responseFields);

			return responseMessage;
		}

		protected override void SendIndirectMessage(IDirectedProtocolMessage message) {
			throw new NotImplementedException();
		}

		protected override void ReportErrorToUser(ProtocolException exception) {
			throw new NotImplementedException();
		}

		protected override void ReportErrorAsDirectResponse(ProtocolException exception) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Prepares to send a request to the Service Provider via the Authorization header.
		/// </summary>
		/// <param name="requestMessage">The message to be transmitted to the ServiceProvider.</param>
		/// <returns>The web request ready to send.</returns>
		/// <remarks>
		/// This method implements OAuth 1.0 section 5.2, item #1 (described in section 5.4).
		/// </remarks>
		private HttpWebRequest InitializeRequestAsAuthHeader(IDirectedProtocolMessage requestMessage) {
			var serializer = MessageSerializer.Get(requestMessage.GetType());
			var fields = serializer.Serialize(requestMessage);

			HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(requestMessage.Recipient);

			StringBuilder authorization = new StringBuilder();
			authorization.Append(requestMessage.Protocol.AuthorizationHeaderScheme);
			authorization.Append(" ");
			foreach (var pair in fields) {
				string key = Uri.EscapeDataString(pair.Key);
				string value = Uri.EscapeDataString(pair.Value);
				authorization.Append(key);
				authorization.Append("=\"");
				authorization.Append(value);
				authorization.Append("\",");
			}
			authorization.Length--; // remove trailing comma

			httpRequest.Headers.Add(HttpRequestHeader.Authorization, authorization.ToString());

			return httpRequest;
		}

		/// <summary>
		/// Prepares to send a request to the Service Provider as the payload of a POST request.
		/// </summary>
		/// <param name="requestMessage">The message to be transmitted to the ServiceProvider.</param>
		/// <returns>The web request ready to send.</returns>
		/// <remarks>
		/// This method implements OAuth 1.0 section 5.2, item #2.
		/// </remarks>
		private HttpWebRequest InitializeRequestAsPost(IDirectedProtocolMessage requestMessage) {
			var serializer = MessageSerializer.Get(requestMessage.GetType());
			var fields = serializer.Serialize(requestMessage);

			HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(requestMessage.Recipient);
			httpRequest.Method = "POST";
			httpRequest.ContentType = "application/x-www-form-urlencoded";
			string requestBody = MessagingUtilities.CreateQueryString(fields);
			httpRequest.ContentLength = requestBody.Length;
			using (StreamWriter writer = new StreamWriter(httpRequest.GetRequestStream())) {
				writer.Write(requestBody);
			}

			return httpRequest;
		}

		/// <summary>
		/// Prepares to send a request to the Service Provider as the query string in a GET request.
		/// </summary>
		/// <param name="requestMessage">The message to be transmitted to the ServiceProvider.</param>
		/// <returns>The web request ready to send.</returns>
		/// <remarks>
		/// This method implements OAuth 1.0 section 5.2, item #3.
		/// </remarks>
		private HttpWebRequest InitializeRequestAsGet(IDirectedProtocolMessage requestMessage) {
			var serializer = MessageSerializer.Get(requestMessage.GetType());
			var fields = serializer.Serialize(requestMessage);

			UriBuilder builder = new UriBuilder(requestMessage.Recipient);
			MessagingUtilities.AppendQueryArgs(builder, fields);
			HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(builder.Uri);

			return httpRequest;
		}
	}
}
