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

	/// <summary>
	/// An OAuth-specific implementation of the <see cref="Channel"/> class.
	/// </summary>
	internal class OAuthChannel : Channel {
		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthChannel"/> class.
		/// </summary>
		internal OAuthChannel()
			: this(new OAuthMessageTypeProvider()) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthChannel"/> class.
		/// </summary>
		/// <param name="messageTypeProvider">
		/// An injected message type provider instance.
		/// Except for mock testing, this should always be <see cref="OAuthMessageTypeProvider"/>.
		/// </param>
		/// <remarks>
		/// This overload for testing purposes only.
		/// </remarks>
		internal OAuthChannel(IMessageTypeProvider messageTypeProvider)
			: base(messageTypeProvider) {
		}

		/// <summary>
		/// Searches an incoming HTTP request for data that could be used to assemble
		/// a protocol request message.
		/// </summary>
		/// <param name="request">The HTTP request to search.</param>
		/// <returns>A dictionary of data in the request.  Should never be null, but may be empty.</returns>
		protected internal override IProtocolMessage ReadFromRequest(HttpRequestInfo request) {
			if (request == null) {
				throw new ArgumentNullException("request");
			}

			// First search the Authorization header.  Use it exclusively if it's present.
			string authorization = request.Headers[HttpRequestHeader.Authorization];
			if (authorization != null) {
				string[] authorizationSections = authorization.Split(';'); // TODO: is this the right delimiter?
				string oauthPrefix = Protocol.Default.AuthorizationHeaderScheme + " ";

				// The Authorization header may have multiple uses, and OAuth may be just one of them.
				// Go through each one looking for an OAuth one.
				foreach (string auth in authorizationSections) {
					string trimmedAuth = auth.Trim();
					if (trimmedAuth.StartsWith(oauthPrefix, StringComparison.Ordinal)) {
						// We found an Authorization: OAuth header.  
						// Parse it according to the rules in section 5.4.1 of the V1.0 spec.
						var fields = new Dictionary<string, string>();
						foreach (string stringPair in trimmedAuth.Substring(oauthPrefix.Length).Split(',')) {
							string[] keyValueStringPair = stringPair.Trim().Split('=');
							string key = Uri.UnescapeDataString(keyValueStringPair[0]);
							string value = Uri.UnescapeDataString(keyValueStringPair[1].Trim('"'));
							fields.Add(key, value);
						}

						return this.Receive(fields);
					}
				}
			}

			// We didn't find an OAuth authorization header.  Revert to other payload methods.
			return base.ReadFromRequest(request);
		}

		/// <summary>
		/// Gets the protocol message that may be in the given HTTP response stream.
		/// </summary>
		/// <param name="responseStream">The response that is anticipated to contain an OAuth message.</param>
		/// <returns>The deserialized message, if one is found.  Null otherwise.</returns>
		protected internal override IProtocolMessage ReadFromResponse(Stream responseStream) {
			if (responseStream == null) {
				throw new ArgumentNullException("responseStream");
			}

			using (StreamReader reader = new StreamReader(responseStream)) {
				string response = reader.ReadToEnd();
				var fields = HttpUtility.ParseQueryString(response).ToDictionary();
				return Receive(fields);
			}
		}

		/// <summary>
		/// Sends a direct message to a remote party and waits for the response.
		/// </summary>
		/// <param name="request">The message to send.</param>
		/// <returns>The remote party's response.</returns>
		protected internal override IProtocolMessage Request(IDirectedProtocolMessage request) {
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

		/// <summary>
		/// Queues a message for sending in the response stream where the fields
		/// are sent in the response stream in querystring style.
		/// </summary>
		/// <param name="response">The message to send as a response.</param>
		/// <remarks>
		/// This method implements spec V1.0 section 5.3.
		/// </remarks>
		protected override void SendDirectMessageResponse(IProtocolMessage response) {
			MessageSerializer serializer = MessageSerializer.Get(response.GetType());
			var fields = serializer.Serialize(response);
			string responseBody = MessagingUtilities.CreateQueryString(fields);

			Response encodedResponse = new Response {
				Body = responseBody,
				OriginalMessage = response,
				Status = HttpStatusCode.OK,
				Headers = new System.Net.WebHeaderCollection(),
			};
			this.QueueIndirectOrResponseMessage(encodedResponse);
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
