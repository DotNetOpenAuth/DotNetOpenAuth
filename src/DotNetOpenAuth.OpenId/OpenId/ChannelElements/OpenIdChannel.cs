//-----------------------------------------------------------------------
// <copyright file="OpenIdChannel.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.ChannelElements {
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
	using System.Text;
	using System.Threading.Tasks;

	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Messages;
	using Validation;

	/// <summary>
	/// A channel that knows how to send and receive OpenID messages.
	/// </summary>
	internal class OpenIdChannel : Channel {
		/// <summary>
		/// The HTTP Content-Type to use in Key-Value Form responses.
		/// </summary>
		/// <remarks>
		/// OpenID 2.0 section 5.1.2 says this SHOULD be text/plain.  But this value 
		/// does not prevent free hosters like GoDaddy from tacking on their ads
		/// to the end of the direct response, corrupting the data.  So we deviate
		/// from the spec a bit here to improve the story for free Providers.
		/// </remarks>
		internal const string KeyValueFormContentType = "application/x-openid-kvf";

		/// <summary>
		/// The encoder that understands how to read and write Key-Value Form.
		/// </summary>
		private KeyValueFormEncoding keyValueForm = new KeyValueFormEncoding();

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdChannel"/> class.
		/// </summary>
		/// <param name="messageTypeProvider">A class prepared to analyze incoming messages and indicate what concrete
		/// message types can deserialize from it.</param>
		/// <param name="bindingElements">The binding elements to use in sending and receiving messages.</param>
		protected OpenIdChannel(IMessageFactory messageTypeProvider, IChannelBindingElement[] bindingElements, IHostFactories hostFactories)
			: base(messageTypeProvider, bindingElements, hostFactories ?? new DefaultOpenIdHostFactories()) {
			Requires.NotNull(messageTypeProvider, "messageTypeProvider");

			// Customize the binding element order, since we play some tricks for higher
			// security and backward compatibility with older OpenID versions.
			var outgoingBindingElements = new List<IChannelBindingElement>(bindingElements);
			var incomingBindingElements = new List<IChannelBindingElement>(bindingElements);
			incomingBindingElements.Reverse();

			// Customize the order of the incoming elements by moving the return_to elements in front.
			var backwardCompatibility = incomingBindingElements.OfType<BackwardCompatibilityBindingElement>().SingleOrDefault();
			var returnToSign = incomingBindingElements.OfType<ReturnToSignatureBindingElement>().SingleOrDefault();
			if (backwardCompatibility != null) {
				incomingBindingElements.MoveTo(0, backwardCompatibility);
			}
			if (returnToSign != null) {
				// Yes, this is intentionally, shifting the backward compatibility
				// binding element to second position.
				incomingBindingElements.MoveTo(0, returnToSign);
			}

			this.CustomizeBindingElementOrder(outgoingBindingElements, incomingBindingElements);
		}

		/// <summary>
		/// Verifies the integrity and applicability of an incoming message.
		/// </summary>
		/// <param name="message">The message just received.</param>
		/// <exception cref="ProtocolException">
		/// Thrown when the message is somehow invalid, except for check_authentication messages.
		/// This can be due to tampering, replay attack or expiration, among other things.
		/// </exception>
		protected override void ProcessIncomingMessage(IProtocolMessage message) {
			var checkAuthRequest = message as CheckAuthenticationRequest;
			if (checkAuthRequest != null) {
				IndirectSignedResponse originalResponse = new IndirectSignedResponse(checkAuthRequest, this);
				try {
					base.ProcessIncomingMessage(originalResponse);
					checkAuthRequest.IsValid = true;
				} catch (ProtocolException) {
					checkAuthRequest.IsValid = false;
				}
			} else {
				base.ProcessIncomingMessage(message);
			}

			// Convert an OpenID indirect error message, which we never expect
			// between two good OpenID implementations, into an exception.
			// We don't process DirectErrorResponse because associate negotiations
			// commonly get a derivative of that message type and handle it.
			var errorMessage = message as IndirectErrorResponse;
			if (errorMessage != null) {
				string exceptionMessage = string.Format(
					CultureInfo.CurrentCulture,
					OpenIdStrings.IndirectErrorFormattedMessage,
					errorMessage.ErrorMessage,
					errorMessage.Contact,
					errorMessage.Reference);
				throw new ProtocolException(exceptionMessage, message);
			}
		}

		/// <summary>
		/// Prepares an HTTP request that carries a given message.
		/// </summary>
		/// <param name="request">The message to send.</param>
		/// <returns>
		/// The <see cref="HttpWebRequest"/> prepared to send the request.
		/// </returns>
		protected override HttpRequestMessage CreateHttpRequest(IDirectedProtocolMessage request) {
			return this.InitializeRequestAsPost(request);
		}

		/// <summary>
		/// Gets the protocol message that may be in the given HTTP response.
		/// </summary>
		/// <param name="response">The response that is anticipated to contain an protocol message.</param>
		/// <returns>
		/// The deserialized message parts, if found.  Null otherwise.
		/// </returns>
		/// <exception cref="ProtocolException">Thrown when the response is not valid.</exception>
		protected override async Task<IDictionary<string, string>> ReadFromResponseCoreAsync(HttpResponseMessage response) {
			try {
				using (var responseStream = await response.Content.ReadAsStreamAsync()) {
					return await this.keyValueForm.GetDictionaryAsync(responseStream);
				}
			} catch (FormatException ex) {
				throw ErrorUtilities.Wrap(ex, ex.Message);
			}
		}

		/// <summary>
		/// Called when receiving a direct response message, before deserialization begins.
		/// </summary>
		/// <param name="response">The HTTP direct response.</param>
		/// <param name="message">The newly instantiated message, prior to deserialization.</param>
		protected override void OnReceivingDirectResponse(HttpResponseMessage response, IDirectResponseProtocolMessage message) {
			base.OnReceivingDirectResponse(response, message);

			// Verify that the expected HTTP status code was used for the message,
			// per OpenID 2.0 section 5.1.2.2.
			// Note: The v1.1 spec doesn't require 400 responses for some error messages
			if (message.Version.Major >= 2) {
				var httpDirectResponse = message as IHttpDirectResponse;
				if (httpDirectResponse != null) {
					ErrorUtilities.VerifyProtocol(
						httpDirectResponse.HttpStatusCode == response.StatusCode,
						MessagingStrings.UnexpectedHttpStatusCode,
						(int)httpDirectResponse.HttpStatusCode,
						(int)response.StatusCode);
				}
			}
		}

		/// <summary>
		/// Queues a message for sending in the response stream where the fields
		/// are sent in the response stream in querystring style.
		/// </summary>
		/// <param name="response">The message to send as a response.</param>
		/// <returns>
		/// The pending user agent redirect based message to be sent as an HttpResponse.
		/// </returns>
		/// <remarks>
		/// This method implements spec V1.0 section 5.3.
		/// </remarks>
		protected override HttpResponseMessage PrepareDirectResponse(IProtocolMessage response) {
			var messageAccessor = this.MessageDescriptions.GetAccessor(response);
			var fields = messageAccessor.Serialize();
			byte[] keyValueEncoding = KeyValueFormEncoding.GetBytes(fields);

			var preparedResponse = new HttpResponseMessage();
			ApplyMessageTemplate(response, preparedResponse);
			var content = new StreamContent(new MemoryStream(keyValueEncoding));
			content.Headers.ContentType = new MediaTypeHeaderValue(KeyValueFormContentType);
			preparedResponse.Content = content;

			IHttpDirectResponse httpMessage = response as IHttpDirectResponse;
			if (httpMessage != null) {
				preparedResponse.StatusCode = httpMessage.HttpStatusCode;
			}

			return preparedResponse;
		}

		protected override HttpMessageHandler WrapMessageHandler(HttpMessageHandler innerHandler) {
			return new ErrorFilteringMessageHandler(base.WrapMessageHandler(innerHandler));
		}

		private class ErrorFilteringMessageHandler : DelegatingHandler {
			internal ErrorFilteringMessageHandler(HttpMessageHandler innerHandler)
				: base(innerHandler) {
			}

			protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken) {
				var response = await base.SendAsync(request, cancellationToken);

				// Filter the responses to the allowable set of HTTP status codes.
				if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.BadRequest) {
					if (Logger.Channel.IsErrorEnabled) {
						var content = await response.Content.ReadAsStringAsync();
						Logger.Channel.ErrorFormat(
							"Unexpected HTTP status code {0} {1} received in direct response:{2}{3}",
							(int)response.StatusCode,
							response.StatusCode,
							Environment.NewLine,
							content);
					}

					// Call dispose before throwing since we're not including the response in the
					// exception we're throwing.
					response.Dispose();

					ErrorUtilities.ThrowProtocol(OpenIdStrings.UnexpectedHttpStatusCode, (int)response.StatusCode, response.StatusCode);
				}

				return response;
			}
		}
	}
}
