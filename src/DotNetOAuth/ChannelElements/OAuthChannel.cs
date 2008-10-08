//-----------------------------------------------------------------------
// <copyright file="OAuthChannel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Net;
	using System.Text;
	using System.Web;
	using DotNetOAuth.Messages;
	using DotNetOAuth.Messaging;
	using DotNetOAuth.Messaging.Bindings;
	using DotNetOAuth.Messaging.Reflection;

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
		/// <param name="signingBindingElement">The binding element to use for signing.</param>
		/// <param name="store">The web application store to use for nonces.</param>
		/// <param name="tokenManager">The token manager instance to use.</param>
		/// <param name="isConsumer">A value indicating whether this channel is being constructed for a Consumer (as opposed to a Service Provider).</param>
		internal OAuthChannel(ITamperProtectionChannelBindingElement signingBindingElement, INonceStore store, ITokenManager tokenManager, bool isConsumer)
			: this(
			signingBindingElement,
			store,
			tokenManager,
			isConsumer ? (IMessageTypeProvider)new OAuthConsumerMessageTypeProvider(tokenManager) : new OAuthServiceProviderMessageTypeProvider(tokenManager),
			new StandardWebRequestHandler()) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthChannel"/> class.
		/// </summary>
		/// <param name="signingBindingElement">The binding element to use for signing.</param>
		/// <param name="store">The web application store to use for nonces.</param>
		/// <param name="tokenManager">The ITokenManager instance to use.</param>
		/// <param name="messageTypeProvider">
		/// An injected message type provider instance.
		/// Except for mock testing, this should always be one of
		/// <see cref="OAuthConsumerMessageTypeProvider"/> or <see cref="OAuthServiceProviderMessageTypeProvider"/>.
		/// </param>
		/// <param name="webRequestHandler">
		/// An instance to a <see cref="IWebRequestHandler"/> that will be used when submitting HTTP
		/// requests and waiting for responses.
		/// </param>
		/// <remarks>
		/// This overload for testing purposes only.
		/// </remarks>
		internal OAuthChannel(ITamperProtectionChannelBindingElement signingBindingElement, INonceStore store, ITokenManager tokenManager, IMessageTypeProvider messageTypeProvider, IWebRequestHandler webRequestHandler)
			: base(messageTypeProvider, new OAuthHttpMethodBindingElement(), signingBindingElement, new StandardExpirationBindingElement(), new StandardReplayProtectionBindingElement(store)) {
			if (tokenManager == null) {
				throw new ArgumentNullException("tokenManager");
			}
			if (webRequestHandler == null) {
				throw new ArgumentNullException("webRequestHandler");
			}

			this.webRequestHandler = webRequestHandler;
			this.TokenManager = tokenManager;
			if (signingBindingElement.SignatureVerificationCallback != null) {
				throw new ArgumentException(Strings.SigningElementAlreadyAssociatedWithChannel, "signingBindingElement");
			}

			signingBindingElement.SignatureVerificationCallback = this.TokenSignatureVerificationCallback;
		}

		/// <summary>
		/// Gets or sets the Consumer web application path.
		/// </summary>
		internal Uri Realm { get; set; }

		/// <summary>
		/// Gets the token manager being used.
		/// </summary>
		protected internal ITokenManager TokenManager { get; private set; }

		/// <summary>
		/// Encodes the names and values that are part of the message per OAuth 1.0 section 5.1.
		/// </summary>
		/// <param name="message">The message with data to encode.</param>
		/// <returns>A dictionary of name-value pairs with their strings encoded.</returns>
		internal static IDictionary<string, string> GetEncodedParameters(IProtocolMessage message) {
			var encodedDictionary = new Dictionary<string, string>();
			EncodeParameters(new MessageDictionary(message), encodedDictionary);
			return encodedDictionary;
		}

		/// <summary>
		/// Encodes the names and values in a dictionary per OAuth 1.0 section 5.1.
		/// </summary>
		/// <param name="source">The dictionary with names and values to encode.</param>
		/// <param name="destination">The dictionary to add the encoded pairs to.</param>
		internal static void EncodeParameters(IDictionary<string, string> source, IDictionary<string, string> destination) {
			if (source == null) {
				throw new ArgumentNullException("source");
			}
			if (destination == null) {
				throw new ArgumentNullException("destination");
			}

			foreach (var pair in source) {
				var key = Uri.EscapeDataString(pair.Key);
				var value = Uri.EscapeDataString(pair.Value);
				destination.Add(key, value);
			}
		}

		/// <summary>
		/// Initializes a web request for sending by attaching a message to it.
		/// Use this method to prepare a protected resource request that you do NOT
		/// expect an OAuth message response to.
		/// </summary>
		/// <param name="request">The message to attach.</param>
		/// <returns>The initialized web request.</returns>
		internal HttpWebRequest InitializeRequest(IDirectedProtocolMessage request) {
			if (request == null) {
				throw new ArgumentNullException("request");
			}

			PrepareMessageForSending(request);
			return this.InitializeRequestInternal(request);
		}

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

						return this.Receive(fields, request.GetRecipient());
					}
				}
			}

			// We didn't find an OAuth authorization header.  Revert to other payload methods.
			IProtocolMessage message = base.ReadFromRequestInternal(request);

			// Add receiving HTTP transport information required for signature generation.
			var signedMessage = message as ITamperResistantOAuthMessage;
			if (signedMessage != null) {
				signedMessage.Recipient = request.Url;
				signedMessage.HttpMethod = request.HttpMethod;
			}

			return message;
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
				return Receive(fields, null);
			}
		}

		/// <summary>
		/// Sends a direct message to a remote party and waits for the response.
		/// </summary>
		/// <param name="request">The message to send.</param>
		/// <returns>The remote party's response.</returns>
		protected override IProtocolMessage RequestInternal(IDirectedProtocolMessage request) {
			HttpWebRequest httpRequest = this.InitializeRequestInternal(request);

			Response response = this.webRequestHandler.GetResponse(httpRequest);
			if (response.ResponseStream == null) {
				return null;
			}
			var responseFields = HttpUtility.ParseQueryString(response.Body).ToDictionary();
			Type messageType = this.MessageTypeProvider.GetResponseMessageType(request, responseFields);
			if (messageType == null) {
				return null;
			}
			var responseSerialize = MessageSerializer.Get(messageType);
			var responseMessage = responseSerialize.Deserialize(responseFields, null);

			return responseMessage;
		}

		/// <summary>
		/// Queues a message for sending in the response stream where the fields
		/// are sent in the response stream in querystring style.
		/// </summary>
		/// <param name="response">The message to send as a response.</param>
		/// <returns>The pending user agent redirect based message to be sent as an HttpResponse.</returns>
		/// <remarks>
		/// This method implements spec V1.0 section 5.3.
		/// </remarks>
		protected override Response SendDirectMessageResponse(IProtocolMessage response) {
			MessageSerializer serializer = MessageSerializer.Get(response.GetType());
			var fields = serializer.Serialize(response);
			string responseBody = MessagingUtilities.CreateQueryString(fields);

			Response encodedResponse = new Response {
				Body = responseBody,
				OriginalMessage = response,
				Status = HttpStatusCode.OK,
				Headers = new System.Net.WebHeaderCollection(),
			};
			return encodedResponse;
		}

		/// <summary>
		/// Prepares to send a request to the Service Provider as the query string in a GET request.
		/// </summary>
		/// <param name="requestMessage">The message to be transmitted to the ServiceProvider.</param>
		/// <returns>The web request ready to send.</returns>
		/// <remarks>
		/// This method implements OAuth 1.0 section 5.2, item #3.
		/// </remarks>
		private static HttpWebRequest InitializeRequestAsGet(IDirectedProtocolMessage requestMessage) {
			var serializer = MessageSerializer.Get(requestMessage.GetType());
			var fields = serializer.Serialize(requestMessage);

			UriBuilder builder = new UriBuilder(requestMessage.Recipient);
			MessagingUtilities.AppendQueryArgs(builder, fields);
			HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(builder.Uri);

			return httpRequest;
		}

		/// <summary>
		/// Initializes a web request by attaching a message to it.
		/// </summary>
		/// <param name="request">The message to attach.</param>
		/// <returns>The initialized web request.</returns>
		private HttpWebRequest InitializeRequestInternal(IDirectedProtocolMessage request) {
			if (request == null) {
				throw new ArgumentNullException("request");
			}
			if (request.Recipient == null) {
				throw new ArgumentException(MessagingStrings.DirectedMessageMissingRecipient, "request");
			}
			IOAuthDirectedMessage oauthRequest = request as IOAuthDirectedMessage;
			if (oauthRequest == null) {
				throw new ArgumentException(
					string.Format(
						CultureInfo.CurrentCulture,
						MessagingStrings.UnexpectedType,
						typeof(IOAuthDirectedMessage),
						request.GetType()));
			}

			HttpWebRequest httpRequest;

			HttpDeliveryMethod transmissionMethod = oauthRequest.HttpMethods;
			if ((transmissionMethod & HttpDeliveryMethod.AuthorizationHeaderRequest) != 0) {
				httpRequest = this.InitializeRequestAsAuthHeader(request);
			} else if ((transmissionMethod & HttpDeliveryMethod.PostRequest) != 0) {
				httpRequest = this.InitializeRequestAsPost(request);
			} else if ((transmissionMethod & HttpDeliveryMethod.GetRequest) != 0) {
				httpRequest = InitializeRequestAsGet(request);
			} else {
				throw new NotSupportedException();
			}
			return httpRequest;
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
			var protocol = Protocol.Lookup(requestMessage.ProtocolVersion);
			var dictionary = new MessageDictionary(requestMessage);

			// copy so as to not modify original
			var fields = new Dictionary<string, string>();
			foreach (string key in dictionary.DeclaredKeys) {
				fields.Add(key, dictionary[key]);
			}
			if (this.Realm != null) {
				fields.Add("realm", this.Realm.AbsoluteUri);
			}

			UriBuilder builder = new UriBuilder(requestMessage.Recipient);
			MessagingUtilities.AppendQueryArgs(builder, requestMessage.ExtraData);
			HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(builder.Uri);

			StringBuilder authorization = new StringBuilder();
			authorization.Append(protocol.AuthorizationHeaderScheme);
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
		/// Fills out the secrets in an incoming message so that signature verification can be performed.
		/// </summary>
		/// <param name="message">The incoming message.</param>
		private void TokenSignatureVerificationCallback(ITamperResistantOAuthMessage message) {
			try {
				message.ConsumerSecret = this.TokenManager.GetConsumerSecret(message.ConsumerKey);

				var tokenMessage = message as ITokenContainingMessage;
				if (tokenMessage != null) {
					message.TokenSecret = this.TokenManager.GetTokenSecret(tokenMessage.Token);
				}
			} catch (KeyNotFoundException ex) {
				throw new ProtocolException(Strings.ConsumerOrTokenSecretNotFound, ex);
			}
		}
	}
}
