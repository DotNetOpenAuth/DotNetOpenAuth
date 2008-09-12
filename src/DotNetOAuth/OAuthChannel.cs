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
		/// The object that will transmit <see cref="HttpWebRequest"/> instances
		/// and return their responses.
		/// </summary>
		private IWebRequestHandler webRequestHandler;

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthChannel"/> class.
		/// </summary>
		internal OAuthChannel()
			: this(new OAuthMessageTypeProvider(), new StandardWebRequestHandler()) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthChannel"/> class.
		/// </summary>
		/// <param name="messageTypeProvider">
		/// An injected message type provider instance.
		/// Except for mock testing, this should always be <see cref="OAuthMessageTypeProvider"/>.
		/// </param>
		/// <param name="webRequestHandler">
		/// An instance to a <see cref="IWebRequestHandler"/> that will be used when submitting HTTP
		/// requests and waiting for responses.
		/// </param>
		/// <remarks>
		/// This overload for testing purposes only.
		/// </remarks>
		internal OAuthChannel(IMessageTypeProvider messageTypeProvider, IWebRequestHandler webRequestHandler)
			: base(messageTypeProvider) {
			if (webRequestHandler == null) {
				throw new ArgumentNullException("webRequestHandler");
			}

			this.webRequestHandler = webRequestHandler;
			this.PreferredTransmissionScheme = MessageScheme.AuthorizationHeaderRequest;
		}

		/// <summary>
		/// Gets or sets the method used in direct requests to transmit the message payload.
		/// </summary>
		internal MessageScheme PreferredTransmissionScheme { get; set; }

		/// <summary>
		/// Searches an incoming HTTP request for data that could be used to assemble
		/// a protocol request message.
		/// </summary>
		/// <param name="request">The HTTP request to search.</param>
		/// <returns>A dictionary of data in the request.  Should never be null, but may be empty.</returns>
		protected override IProtocolMessage ReadFromRequestInternal(HttpRequestInfo request) {
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
			return base.ReadFromRequestInternal(request);
		}

		/// <summary>
		/// Gets the protocol message that may be in the given HTTP response stream.
		/// </summary>
		/// <param name="responseStream">The response that is anticipated to contain an OAuth message.</param>
		/// <returns>The deserialized message, if one is found.  Null otherwise.</returns>
		protected override IProtocolMessage ReadFromResponseInternal(Stream responseStream) {
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
		protected override IProtocolMessage RequestInternal(IDirectedProtocolMessage request) {
			if (request == null) {
				throw new ArgumentNullException("request");
			}
			if (request.Recipient == null) {
				throw new ArgumentException(MessagingStrings.DirectedMessageMissingRecipient, "request");
			}

			HttpWebRequest httpRequest;

			MessageScheme transmissionMethod = this.PreferredTransmissionScheme;
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

			Response response = this.webRequestHandler.GetResponse(httpRequest);
			if (response.Body == null) {
				return null;
			}
			var responseFields = HttpUtility.ParseQueryString(response.Body).ToDictionary();
			Type messageType = this.MessageTypeProvider.GetResponseMessageType(request, responseFields);
			if (messageType == null) {
				return null;
			}
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
			using (TextWriter writer = this.webRequestHandler.GetRequestStream(httpRequest)) {
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
