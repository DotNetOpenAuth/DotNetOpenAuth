//-----------------------------------------------------------------------
// <copyright file="OAuthChannel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.Contracts;
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
		internal OAuthChannel(ITamperProtectionChannelBindingElement signingBindingElement, INonceStore store, IConsumerTokenManager tokenManager)
			: this(
			signingBindingElement,
			store,
			tokenManager,
			new OAuthConsumerMessageFactory()) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthChannel"/> class.
		/// </summary>
		/// <param name="signingBindingElement">The binding element to use for signing.</param>
		/// <param name="store">The web application store to use for nonces.</param>
		/// <param name="tokenManager">The token manager instance to use.</param>
		internal OAuthChannel(ITamperProtectionChannelBindingElement signingBindingElement, INonceStore store, IServiceProviderTokenManager tokenManager)
			: this(
			signingBindingElement,
			store,
			tokenManager,
			new OAuthServiceProviderMessageFactory(tokenManager)) {
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
		internal OAuthChannel(ITamperProtectionChannelBindingElement signingBindingElement, INonceStore store, ITokenManager tokenManager, IMessageFactory messageTypeProvider)
			: base(messageTypeProvider, InitializeBindingElements(signingBindingElement, store, tokenManager)) {
			ErrorUtilities.VerifyArgumentNotNull(tokenManager, "tokenManager");

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
		internal static IDictionary<string, string> GetUriEscapedParameters(MessageDictionary message) {
			var encodedDictionary = new Dictionary<string, string>();
			UriEscapeParameters(message, encodedDictionary);
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

			ProcessOutgoingMessage(request);
			return this.CreateHttpRequest(request);
		}

		/// <summary>
		/// Searches an incoming HTTP request for data that could be used to assemble
		/// a protocol request message.
		/// </summary>
		/// <param name="request">The HTTP request to search.</param>
		/// <returns>The deserialized message, if one is found.  Null otherwise.</returns>
		protected override IDirectedProtocolMessage ReadFromRequestCore(HttpRequestInfo request) {
			ErrorUtilities.VerifyArgumentNotNull(request, "request");

			var fields = new Dictionary<string, string>();

			// First search the Authorization header.
			string authorization = request.Headers[HttpRequestHeader.Authorization];
			if (authorization != null) {
				string[] authorizationSections = authorization.Split(';'); // TODO: is this the right delimiter?
				string oauthPrefix = Protocol.AuthorizationHeaderScheme + " ";

				// The Authorization header may have multiple uses, and OAuth may be just one of them.
				// Go through each one looking for an OAuth one.
				foreach (string auth in authorizationSections) {
					string trimmedAuth = auth.Trim();
					if (trimmedAuth.StartsWith(oauthPrefix, StringComparison.Ordinal)) {
						// We found an Authorization: OAuth header.  
						// Parse it according to the rules in section 5.4.1 of the V1.0 spec.
						foreach (string stringPair in trimmedAuth.Substring(oauthPrefix.Length).Split(',')) {
							string[] keyValueStringPair = stringPair.Trim().Split('=');
							string key = Uri.UnescapeDataString(keyValueStringPair[0]);
							string value = Uri.UnescapeDataString(keyValueStringPair[1].Trim('"'));
							fields.Add(key, value);
						}
					}
				}

				fields.Remove("realm"); // ignore the realm parameter, since we don't use it, and it must be omitted from signature base string.
			}

			// Scrape the entity
			if (string.Equals(request.Headers[HttpRequestHeader.ContentType], HttpFormUrlEncoded, StringComparison.Ordinal)) {
				foreach (string key in request.Form) {
					fields.Add(key, request.Form[key]);
				}
			}

			// Scrape the query string
			foreach (string key in request.QueryStringBeforeRewriting) {
				if (key != null) {
					fields.Add(key, request.QueryStringBeforeRewriting[key]);
				} else {
					Logger.OAuth.WarnFormat("Ignoring query string parameter '{0}' since it isn't a standard name=value parameter.", request.QueryStringBeforeRewriting[key]);
				}
			}

			// Deserialize the message using all the data we've collected.
			var message = (IDirectedProtocolMessage)this.Receive(fields, request.GetRecipient());

			// Add receiving HTTP transport information required for signature generation.
			var signedMessage = message as ITamperResistantOAuthMessage;
			if (signedMessage != null) {
				signedMessage.Recipient = request.UrlBeforeRewriting;
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
		protected override IDictionary<string, string> ReadFromResponseCore(IncomingWebResponse response) {
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
		protected override OutgoingWebResponse PrepareDirectResponse(IProtocolMessage response) {
			ErrorUtilities.VerifyArgumentNotNull(response, "response");

			var messageAccessor = this.MessageDescriptions.GetAccessor(response);
			var fields = messageAccessor.Serialize();
			string responseBody = MessagingUtilities.CreateQueryString(fields);

			OutgoingWebResponse encodedResponse = new OutgoingWebResponse {
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
		/// Initializes the binding elements for the OAuth channel.
		/// </summary>
		/// <param name="signingBindingElement">The signing binding element.</param>
		/// <param name="store">The nonce store.</param>
		/// <param name="tokenManager">The token manager.</param>
		/// <returns>An array of binding elements used to initialize the channel.</returns>
		private static IChannelBindingElement[] InitializeBindingElements(ITamperProtectionChannelBindingElement signingBindingElement, INonceStore store, ITokenManager tokenManager) {
			var bindingElements = new List<IChannelBindingElement> {
				new OAuthHttpMethodBindingElement(),
				signingBindingElement,
				new StandardExpirationBindingElement(),
				new StandardReplayProtectionBindingElement(store),
			};

			var spTokenManager = tokenManager as IServiceProviderTokenManager;
			if (spTokenManager != null) {
				bindingElements.Insert(0, new TokenHandlingBindingElement(spTokenManager));
			}

			return bindingElements.ToArray();
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
				var key = MessagingUtilities.EscapeUriDataStringRfc3986(pair.Key);
				var value = MessagingUtilities.EscapeUriDataStringRfc3986(pair.Value);
				destination.Add(key, value);
			}
		}

		/// <summary>
		/// Gets the HTTP method to use for a message.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <returns>"POST", "GET" or some other similar http verb.</returns>
		private static string GetHttpMethod(IDirectedProtocolMessage message) {
			Contract.Requires(message != null);
			ErrorUtilities.VerifyArgumentNotNull(message, "message");

			var signedMessage = message as ITamperResistantOAuthMessage;
			if (signedMessage != null) {
				return signedMessage.HttpMethod;
			} else {
				return (message.HttpMethods & HttpDeliveryMethods.PostRequest) != 0 ? "POST" : "GET";
			}
		}

		/// <summary>
		/// Prepares to send a request to the Service Provider via the Authorization header.
		/// </summary>
		/// <param name="requestMessage">The message to be transmitted to the ServiceProvider.</param>
		/// <returns>The web request ready to send.</returns>
		/// <remarks>
		/// 	<para>If the message has non-empty ExtraData in it, the request stream is sent to
		/// the server automatically.  If it is empty, the request stream must be sent by the caller.</para>
		/// 	<para>This method implements OAuth 1.0 section 5.2, item #1 (described in section 5.4).</para>
		/// </remarks>
		private HttpWebRequest InitializeRequestAsAuthHeader(IDirectedProtocolMessage requestMessage) {
			var dictionary = this.MessageDescriptions.GetAccessor(requestMessage);

			// copy so as to not modify original
			var fields = new Dictionary<string, string>();
			foreach (string key in dictionary.DeclaredKeys) {
				fields.Add(key, dictionary[key]);
			}
			if (this.Realm != null) {
				fields.Add("realm", this.Realm.AbsoluteUri);
			}

			HttpWebRequest httpRequest;
			UriBuilder recipientBuilder = new UriBuilder(requestMessage.Recipient);
			bool hasEntity = HttpMethodHasEntity(GetHttpMethod(requestMessage));

			if (!hasEntity) {
				MessagingUtilities.AppendQueryArgs(recipientBuilder, requestMessage.ExtraData);
			}
			httpRequest = (HttpWebRequest)WebRequest.Create(recipientBuilder.Uri);
			httpRequest.Method = GetHttpMethod(requestMessage);

			StringBuilder authorization = new StringBuilder();
			authorization.Append(Protocol.AuthorizationHeaderScheme);
			authorization.Append(" ");
			foreach (var pair in fields) {
				string key = MessagingUtilities.EscapeUriDataStringRfc3986(pair.Key);
				string value = MessagingUtilities.EscapeUriDataStringRfc3986(pair.Value);
				authorization.Append(key);
				authorization.Append("=\"");
				authorization.Append(value);
				authorization.Append("\",");
			}
			authorization.Length--; // remove trailing comma

			httpRequest.Headers.Add(HttpRequestHeader.Authorization, authorization.ToString());

			if (hasEntity) {
				// WARNING: We only set up the request stream for the caller if there is
				// extra data.  If there isn't any extra data, the caller must do this themselves.
				if (requestMessage.ExtraData.Count > 0) {
					SendParametersInEntity(httpRequest, requestMessage.ExtraData);
				} else {
					// We'll assume the content length is zero since the caller may not have
					// anything.  They're responsible to change it when the add the payload if they have one.
					httpRequest.ContentLength = 0;
				}
			}

			return httpRequest;
		}

		/// <summary>
		/// Fills out the secrets in a message so that signing/verification can be performed.
		/// </summary>
		/// <param name="message">The message about to be signed or whose signature is about to be verified.</param>
		private void SignatureCallback(ITamperResistantProtocolMessage message) {
			var oauthMessage = message as ITamperResistantOAuthMessage;
			try {
				Logger.Channel.Debug("Applying secrets to message to prepare for signing or signature verification.");
				oauthMessage.ConsumerSecret = this.GetConsumerSecret(oauthMessage.ConsumerKey);

				var tokenMessage = message as ITokenContainingMessage;
				if (tokenMessage != null) {
					oauthMessage.TokenSecret = this.TokenManager.GetTokenSecret(tokenMessage.Token);
				}
			} catch (KeyNotFoundException ex) {
				throw new ProtocolException(OAuthStrings.ConsumerOrTokenSecretNotFound, ex);
			}
		}

		/// <summary>
		/// Gets the consumer secret for a given consumer key.
		/// </summary>
		/// <param name="consumerKey">The consumer key.</param>
		/// <returns>The consumer secret.</returns>
		private string GetConsumerSecret(string consumerKey) {
			var consumerTokenManager = this.TokenManager as IConsumerTokenManager;
			if (consumerTokenManager != null) {
				ErrorUtilities.VerifyInternal(consumerKey == consumerTokenManager.ConsumerKey, "The token manager consumer key and the consumer key set earlier do not match!");
				return consumerTokenManager.ConsumerSecret;
			} else {
				return ((IServiceProviderTokenManager)this.TokenManager).GetConsumer(consumerKey).Secret;
			}
		}
	}
}
