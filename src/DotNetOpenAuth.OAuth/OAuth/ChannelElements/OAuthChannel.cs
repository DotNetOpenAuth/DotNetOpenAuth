//-----------------------------------------------------------------------
// <copyright file="OAuthChannel.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Net.Mime;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OAuth.Messages;
	using Validation;

	using HttpRequestHeaders = DotNetOpenAuth.Messaging.HttpRequestHeaders;

	/// <summary>
	/// An OAuth-specific implementation of the <see cref="Channel"/> class.
	/// </summary>
	internal abstract class OAuthChannel : Channel {
		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthChannel" /> class.
		/// </summary>
		/// <param name="signingBindingElement">The binding element to use for signing.</param>
		/// <param name="tokenManager">The ITokenManager instance to use.</param>
		/// <param name="securitySettings">The security settings.</param>
		/// <param name="messageTypeProvider">An injected message type provider instance.
		/// Except for mock testing, this should always be one of
		/// OAuthConsumerMessageFactory or OAuthServiceProviderMessageFactory.</param>
		/// <param name="bindingElements">The binding elements.</param>
		/// <param name="hostFactories">The host factories.</param>
		[SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Diagnostics.Contracts.__ContractsRuntime.Requires<System.ArgumentNullException>(System.Boolean,System.String,System.String)", Justification = "Code contracts"), SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "securitySettings", Justification = "Code contracts")]
		protected OAuthChannel(ITamperProtectionChannelBindingElement signingBindingElement, ITokenManager tokenManager, SecuritySettings securitySettings, IMessageFactory messageTypeProvider, IChannelBindingElement[] bindingElements, IHostFactories hostFactories = null)
			: base(messageTypeProvider, bindingElements, hostFactories ?? new DefaultOAuthHostFactories()) {
			Requires.NotNull(tokenManager, "tokenManager");
			Requires.NotNull(securitySettings, "securitySettings");
			Requires.NotNull(signingBindingElement, "signingBindingElement");
			Requires.That(signingBindingElement.SignatureCallback == null, "signingBindingElement", OAuthStrings.SigningElementAlreadyAssociatedWithChannel);
			Requires.NotNull(bindingElements, "bindingElements");

			this.TokenManager = tokenManager;
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
		internal static IDictionary<string, string> GetUriEscapedParameters(IEnumerable<KeyValuePair<string, string>> message) {
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
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The initialized web request.</returns>
		internal async Task<HttpRequestMessage> InitializeRequestAsync(IDirectedProtocolMessage request, CancellationToken cancellationToken) {
			Requires.NotNull(request, "request");

			await this.ProcessOutgoingMessageAsync(request, cancellationToken);
			return this.CreateHttpRequest(request);
		}

		/// <summary>
		/// Initializes the binding elements for the OAuth channel.
		/// </summary>
		/// <param name="signingBindingElement">The signing binding element.</param>
		/// <param name="store">The nonce store.</param>
		/// <returns>
		/// An array of binding elements used to initialize the channel.
		/// </returns>
		protected static List<IChannelBindingElement> InitializeBindingElements(ITamperProtectionChannelBindingElement signingBindingElement, INonceStore store) {
			var bindingElements = new List<IChannelBindingElement> {
				new OAuthHttpMethodBindingElement(),
				signingBindingElement,
				new StandardExpirationBindingElement(),
				new StandardReplayProtectionBindingElement(store),
			};

			return bindingElements;
		}

		/// <summary>
		/// Searches an incoming HTTP request for data that could be used to assemble
		/// a protocol request message.
		/// </summary>
		/// <param name="request">The HTTP request to search.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The deserialized message, if one is found.  Null otherwise.</returns>
		protected override async Task<IDirectedProtocolMessage> ReadFromRequestCoreAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
			// First search the Authorization header.
			var authorization = request.Headers.Authorization;
			var fields = MessagingUtilities.ParseAuthorizationHeader(Protocol.AuthorizationHeaderScheme, authorization).ToDictionary();
			fields.Remove("realm"); // ignore the realm parameter, since we don't use it, and it must be omitted from signature base string.

			// Scrape the entity
			foreach (var pair in await ParseUrlEncodedFormContentAsync(request, cancellationToken)) {
				if (pair.Key != null) {
					fields.Add(pair.Key, pair.Value);
				} else {
					Logger.OAuth.WarnFormat("Ignoring query string parameter '{0}' since it isn't a standard name=value parameter.", pair.Value);
				}
			}

			// Scrape the query string
			var qs = HttpUtility.ParseQueryString(request.RequestUri.Query);
			foreach (string key in qs) {
				if (key != null) {
					fields.Add(key, qs[key]);
				} else {
					Logger.OAuth.WarnFormat("Ignoring query string parameter '{0}' since it isn't a standard name=value parameter.", qs[key]);
				}
			}

			MessageReceivingEndpoint recipient;
			try {
				recipient = request.GetRecipient();
			} catch (ArgumentException ex) {
				Logger.OAuth.WarnFormat("Unrecognized HTTP request: " + ex.ToString());
				return null;
			}

			// Deserialize the message using all the data we've collected.
			var message = (IDirectedProtocolMessage)this.Receive(fields, recipient);

			// Add receiving HTTP transport information required for signature generation.
			var signedMessage = message as ITamperResistantOAuthMessage;
			if (signedMessage != null) {
				signedMessage.Recipient = request.RequestUri;
				signedMessage.HttpMethod = request.Method;
			}

			return message;
		}

		/// <summary>
		/// Gets the protocol message that may be in the given HTTP response.
		/// </summary>
		/// <param name="response">The response that is anticipated to contain an protocol message.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The deserialized message parts, if found.  Null otherwise.
		/// </returns>
		protected override async Task<IDictionary<string, string>> ReadFromResponseCoreAsync(HttpResponseMessage response, CancellationToken cancellationToken) {
			string body = await response.Content.ReadAsStringAsync();
			return HttpUtility.ParseQueryString(body).ToDictionary();
		}

		/// <summary>
		/// Prepares an HTTP request that carries a given message.
		/// </summary>
		/// <param name="request">The message to send.</param>
		/// <returns>
		/// The <see cref="HttpRequest"/> prepared to send the request.
		/// </returns>
		protected override HttpRequestMessage CreateHttpRequest(IDirectedProtocolMessage request) {
			HttpRequestMessage httpRequest;

			HttpDeliveryMethods transmissionMethod = request.HttpMethods;
			if ((transmissionMethod & HttpDeliveryMethods.AuthorizationHeaderRequest) != 0) {
				httpRequest = this.InitializeRequestAsAuthHeader(request);
			} else if ((transmissionMethod & HttpDeliveryMethods.PostRequest) != 0) {
				var requestMessageWithBinaryData = request as IMessageWithBinaryData;
				ErrorUtilities.VerifyProtocol(requestMessageWithBinaryData == null || !requestMessageWithBinaryData.SendAsMultipart, OAuthStrings.MultipartPostMustBeUsedWithAuthHeader);
				httpRequest = this.InitializeRequestAsPost(request);
			} else if ((transmissionMethod & HttpDeliveryMethods.GetRequest) != 0) {
				httpRequest = InitializeRequestAsGet(request);
			} else if ((transmissionMethod & HttpDeliveryMethods.HeadRequest) != 0) {
				httpRequest = InitializeRequestAsHead(request);
			} else if ((transmissionMethod & HttpDeliveryMethods.PutRequest) != 0) {
				httpRequest = this.InitializeRequestAsPut(request);
			} else if ((transmissionMethod & HttpDeliveryMethods.DeleteRequest) != 0) {
				httpRequest = InitializeRequestAsDelete(request);
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
		protected override HttpResponseMessage PrepareDirectResponse(IProtocolMessage response) {
			var messageAccessor = this.MessageDescriptions.GetAccessor(response);
			var fields = messageAccessor.Serialize();

			var encodedResponse = new HttpResponseMessage {
				Content = new FormUrlEncodedContent(fields),
			};

			ApplyMessageTemplate(response, encodedResponse);
			return encodedResponse;
		}

		/// <summary>
		/// Gets the consumer secret for a given consumer key.
		/// </summary>
		/// <param name="consumerKey">The consumer key.</param>
		/// <returns>A consumer secret.</returns>
		protected abstract string GetConsumerSecret(string consumerKey);

		/// <summary>
		/// Uri-escapes the names and values in a dictionary per OAuth 1.0 section 5.1.
		/// </summary>
		/// <param name="source">The dictionary with names and values to encode.</param>
		/// <param name="destination">The dictionary to add the encoded pairs to.</param>
		private static void UriEscapeParameters(IEnumerable<KeyValuePair<string, string>> source, IDictionary<string, string> destination) {
			Requires.NotNull(source, "source");
			Requires.NotNull(destination, "destination");

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
		private static HttpMethod GetHttpMethod(IDirectedProtocolMessage message) {
			Requires.NotNull(message, "message");

			var signedMessage = message as ITamperResistantOAuthMessage;
			if (signedMessage != null) {
				return signedMessage.HttpMethod;
			} else {
				return MessagingUtilities.GetHttpVerb(message.HttpMethods);
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
		private HttpRequestMessage InitializeRequestAsAuthHeader(IDirectedProtocolMessage requestMessage) {
			var dictionary = this.MessageDescriptions.GetAccessor(requestMessage);

			// copy so as to not modify original
			var fields = new Dictionary<string, string>();
			foreach (string key in dictionary.DeclaredKeys) {
				fields.Add(key, dictionary[key]);
			}

			if (this.Realm != null) {
				fields.Add("realm", this.Realm.AbsoluteUri);
			}

			UriBuilder recipientBuilder = new UriBuilder(requestMessage.Recipient);
			bool hasEntity = HttpMethodHasEntity(GetHttpMethod(requestMessage));

			if (!hasEntity) {
				MessagingUtilities.AppendQueryArgs(recipientBuilder, requestMessage.ExtraData);
			}

			var httpRequest = new HttpRequestMessage(GetHttpMethod(requestMessage), recipientBuilder.Uri);
			this.PrepareHttpWebRequest(httpRequest);

			httpRequest.Headers.Authorization = new AuthenticationHeaderValue(Protocol.AuthorizationHeaderScheme, MessagingUtilities.AssembleAuthorizationHeader(fields));

			if (hasEntity) {
				var requestMessageWithBinaryData = requestMessage as IMessageWithBinaryData;
				if (requestMessageWithBinaryData != null && requestMessageWithBinaryData.SendAsMultipart) {
					// Include the binary data in the multipart entity, and any standard text extra message data.
					// The standard declared message parts are included in the authorization header.
					var content = InitializeMultipartFormDataContent(requestMessageWithBinaryData);
					httpRequest.Content = content;

					foreach (var extraData in requestMessage.ExtraData) {
						content.Add(new StringContent(extraData.Value), extraData.Key);
					}
				} else {
					ErrorUtilities.VerifyProtocol(requestMessageWithBinaryData == null || requestMessageWithBinaryData.BinaryData.Count == 0, MessagingStrings.BinaryDataRequiresMultipart);
					if (requestMessage.ExtraData.Count > 0) {
						httpRequest.Content = new FormUrlEncodedContent(requestMessage.ExtraData);
					}
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
	}
}
