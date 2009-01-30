//-----------------------------------------------------------------------
// <copyright file="OAuthChannel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Net;
	using System.Text;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OAuth.Messages;

	/// <summary>
	/// An OAuth-specific implementation of the <see cref="Channel"/> class.
	/// </summary>
	internal class OAuthChannel : Channel {
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
			isConsumer ? (IMessageFactory)new OAuthConsumerMessageFactory() : new OAuthServiceProviderMessageFactory(tokenManager)) {
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
		/// <see cref="OAuthConsumerMessageFactory"/> or <see cref="OAuthServiceProviderMessageFactory"/>.
		/// </param>
		/// <remarks>
		/// This overload for testing purposes only.
		/// </remarks>
		internal OAuthChannel(ITamperProtectionChannelBindingElement signingBindingElement, INonceStore store, ITokenManager tokenManager, IMessageFactory messageTypeProvider)
			: base(messageTypeProvider, new OAuthHttpMethodBindingElement(), signingBindingElement, new StandardExpirationBindingElement(), new StandardReplayProtectionBindingElement(store)) {
			if (tokenManager == null) {
				throw new ArgumentNullException("tokenManager");
			}

			this.TokenManager = tokenManager;
			ErrorUtilities.VerifyArgumentNamed(signingBindingElement.SignatureCallback == null, "signingBindingElement", OAuthStrings.SigningElementAlreadyAssociatedWithChannel);

			signingBindingElement.SignatureCallback = this.SignatureCallback;
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
		/// Uri-escapes the names and values in a dictionary per OAuth 1.0 section 5.1.
		/// </summary>
		/// <param name="message">The message with data to encode.</param>
		/// <returns>A dictionary of name-value pairs with their strings encoded.</returns>
		internal static IDictionary<string, string> GetUriEscapedParameters(IProtocolMessage message) {
			var encodedDictionary = new Dictionary<string, string>();
			UriEscapeParameters(new MessageDictionary(message), encodedDictionary);
			return encodedDictionary;
		}

		/// <summary>
		/// Initializes a web request for sending by attaching a message to it.
		/// Use this method to prepare a protected resource request that you do NOT
		/// expect an OAuth message response to.
		/// </summary>
		/// <param name="request">The message to attach.</param>
		/// <returns>The initialized web request.</returns>
		internal HttpWebRequest InitializeRequest(IDirectedProtocolMessage request) {
			ErrorUtilities.VerifyArgumentNotNull(request, "request");

			PrepareMessageForSending(request);
			return this.CreateHttpRequest(request);
		}

		/// <summary>
		/// Searches an incoming HTTP request for data that could be used to assemble
		/// a protocol request message.
		/// </summary>
		/// <param name="request">The HTTP request to search.</param>
		/// <returns>A dictionary of data in the request.  Should never be null, but may be empty.</returns>
		protected override IDirectedProtocolMessage ReadFromRequestInternal(HttpRequestInfo request) {
			ErrorUtilities.VerifyArgumentNotNull(request, "request");

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

						return (IDirectedProtocolMessage)this.Receive(fields, request.GetRecipient());
					}
				}
			}

			// We didn't find an OAuth authorization header.  Revert to other payload methods.
			IDirectedProtocolMessage message = base.ReadFromRequestInternal(request);

			// Add receiving HTTP transport information required for signature generation.
			var signedMessage = message as ITamperResistantOAuthMessage;
			if (signedMessage != null) {
				signedMessage.Recipient = request.Url;
				signedMessage.HttpMethod = request.HttpMethod;
			}

			return message;
		}

		/// <summary>
		/// Gets the protocol message that may be in the given HTTP response.
		/// </summary>
		/// <param name="response">The response that is anticipated to contain an protocol message.</param>
		/// <returns>
		/// The deserialized message parts, if found.  Null otherwise.
		/// </returns>
		protected override IDictionary<string, string> ReadFromResponseInternal(DirectWebResponse response) {
			ErrorUtilities.VerifyArgumentNotNull(response, "response");

			string body = response.GetResponseReader().ReadToEnd();
			return HttpUtility.ParseQueryString(body).ToDictionary();
		}

		/// <summary>
		/// Prepares an HTTP request that carries a given message.
		/// </summary>
		/// <param name="request">The message to send.</param>
		/// <returns>
		/// The <see cref="HttpRequest"/> prepared to send the request.
		/// </returns>
		protected override HttpWebRequest CreateHttpRequest(IDirectedProtocolMessage request) {
			ErrorUtilities.VerifyArgumentNotNull(request, "request");
			ErrorUtilities.VerifyArgumentNamed(request.Recipient != null, "request", MessagingStrings.DirectedMessageMissingRecipient);

			IDirectedProtocolMessage oauthRequest = request as IDirectedProtocolMessage;
			ErrorUtilities.VerifyArgument(oauthRequest != null, MessagingStrings.UnexpectedType, typeof(IDirectedProtocolMessage), request.GetType());

			HttpWebRequest httpRequest;

			HttpDeliveryMethods transmissionMethod = oauthRequest.HttpMethods;
			if ((transmissionMethod & HttpDeliveryMethods.AuthorizationHeaderRequest) != 0) {
				httpRequest = this.InitializeRequestAsAuthHeader(request);
			} else if ((transmissionMethod & HttpDeliveryMethods.PostRequest) != 0) {
				httpRequest = this.InitializeRequestAsPost(request);
			} else if ((transmissionMethod & HttpDeliveryMethods.GetRequest) != 0) {
				httpRequest = InitializeRequestAsGet(request);
			} else {
				throw new NotSupportedException();
			}
			return httpRequest;
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
		protected override UserAgentResponse SendDirectMessageResponse(IProtocolMessage response) {
			ErrorUtilities.VerifyArgumentNotNull(response, "response");

			MessageSerializer serializer = MessageSerializer.Get(response.GetType());
			var fields = serializer.Serialize(response);
			string responseBody = MessagingUtilities.CreateQueryString(fields);

			UserAgentResponse encodedResponse = new UserAgentResponse {
				Body = responseBody,
				OriginalMessage = response,
				Status = HttpStatusCode.OK,
				Headers = new System.Net.WebHeaderCollection(),
			};

			IHttpDirectResponse httpMessage = response as IHttpDirectResponse;
			if (httpMessage != null) {
				encodedResponse.Status = httpMessage.HttpStatusCode;
			}

			return encodedResponse;
		}

		/// <summary>
		/// Uri-escapes the names and values in a dictionary per OAuth 1.0 section 5.1.
		/// </summary>
		/// <param name="source">The dictionary with names and values to encode.</param>
		/// <param name="destination">The dictionary to add the encoded pairs to.</param>
		private static void UriEscapeParameters(IDictionary<string, string> source, IDictionary<string, string> destination) {
			ErrorUtilities.VerifyArgumentNotNull(source, "source");
			ErrorUtilities.VerifyArgumentNotNull(destination, "destination");

			foreach (var pair in source) {
				var key = Uri.EscapeDataString(pair.Key);
				var value = Uri.EscapeDataString(pair.Value);
				destination.Add(key, value);
			}
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
			var protocol = Protocol.Lookup(requestMessage.Version);
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
		/// Fills out the secrets in a message so that signing/verification can be performed.
		/// </summary>
		/// <param name="message">The message about to be signed or whose signature is about to be verified.</param>
		private void SignatureCallback(ITamperResistantProtocolMessage message) {
			var oauthMessage = message as ITamperResistantOAuthMessage;
			try {
				Logger.Debug("Applying secrets to message to prepare for signing or signature verification.");
				oauthMessage.ConsumerSecret = this.TokenManager.GetConsumerSecret(oauthMessage.ConsumerKey);

				var tokenMessage = message as ITokenContainingMessage;
				if (tokenMessage != null) {
					oauthMessage.TokenSecret = this.TokenManager.GetTokenSecret(tokenMessage.Token);
				}
			} catch (KeyNotFoundException ex) {
				throw new ProtocolException(OAuthStrings.ConsumerOrTokenSecretNotFound, ex);
			}
		}
	}
}
