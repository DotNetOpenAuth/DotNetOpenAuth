//-----------------------------------------------------------------------
// <copyright file="OpenIdChannel.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// A channel that knows how to send and receive OpenID messages.
	/// </summary>
	[ContractVerification(true)]
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
		protected OpenIdChannel(IMessageFactory messageTypeProvider, IChannelBindingElement[] bindingElements)
			: base(messageTypeProvider, bindingElements) {
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

			// Change out the standard web request handler to reflect the standard
			// OpenID pattern that outgoing web requests are to unknown and untrusted
			// servers on the Internet.
			this.WebRequestHandler = new UntrustedWebRequestHandler();
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
		protected override HttpWebRequest CreateHttpRequest(IDirectedProtocolMessage request) {
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
		protected override IDictionary<string, string> ReadFromResponseCore(IncomingWebResponse response) {
			try {
				return this.keyValueForm.GetDictionary(response.ResponseStream);
			} catch (FormatException ex) {
				throw ErrorUtilities.Wrap(ex, ex.Message);
			}
		}

		/// <summary>
		/// Called when receiving a direct response message, before deserialization begins.
		/// </summary>
		/// <param name="response">The HTTP direct response.</param>
		/// <param name="message">The newly instantiated message, prior to deserialization.</param>
		protected override void OnReceivingDirectResponse(IncomingWebResponse response, IDirectResponseProtocolMessage message) {
			base.OnReceivingDirectResponse(response, message);

			// Verify that the expected HTTP status code was used for the message,
			// per OpenID 2.0 section 5.1.2.2.
			// Note: The v1.1 spec doesn't require 400 responses for some error messages
			if (message.Version.Major >= 2) {
				var httpDirectResponse = message as IHttpDirectResponse;
				if (httpDirectResponse != null) {
					ErrorUtilities.VerifyProtocol(
						httpDirectResponse.HttpStatusCode == response.Status,
						MessagingStrings.UnexpectedHttpStatusCode,
						(int)httpDirectResponse.HttpStatusCode,
						(int)response.Status);
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
		protected override OutgoingWebResponse PrepareDirectResponse(IProtocolMessage response) {
			var messageAccessor = this.MessageDescriptions.GetAccessor(response);
			var fields = messageAccessor.Serialize();
			byte[] keyValueEncoding = KeyValueFormEncoding.GetBytes(fields);

			OutgoingWebResponse preparedResponse = new OutgoingWebResponse();
			ApplyMessageTemplate(response, preparedResponse);
			preparedResponse.Headers.Add(HttpResponseHeader.ContentType, KeyValueFormContentType);
			preparedResponse.OriginalMessage = response;
			preparedResponse.ResponseStream = new MemoryStream(keyValueEncoding);

			IHttpDirectResponse httpMessage = response as IHttpDirectResponse;
			if (httpMessage != null) {
				preparedResponse.Status = httpMessage.HttpStatusCode;
			}

			return preparedResponse;
		}

		/// <summary>
		/// Gets the direct response of a direct HTTP request.
		/// </summary>
		/// <param name="webRequest">The web request.</param>
		/// <returns>The response to the web request.</returns>
		/// <exception cref="ProtocolException">Thrown on network or protocol errors.</exception>
		protected override IncomingWebResponse GetDirectResponse(HttpWebRequest webRequest) {
			IncomingWebResponse response = this.WebRequestHandler.GetResponse(webRequest, DirectWebRequestOptions.AcceptAllHttpResponses);

			// Filter the responses to the allowable set of HTTP status codes.
			if (response.Status != HttpStatusCode.OK && response.Status != HttpStatusCode.BadRequest) {
				if (Logger.Channel.IsErrorEnabled) {
					using (var reader = new StreamReader(response.ResponseStream)) {
						Logger.Channel.ErrorFormat(
							"Unexpected HTTP status code {0} {1} received in direct response:{2}{3}",
							(int)response.Status,
							response.Status,
							Environment.NewLine,
							reader.ReadToEnd());
					}
				}

				// Call dispose before throwing since we're not including the response in the
				// exception we're throwing.
				response.Dispose();

				ErrorUtilities.ThrowProtocol(OpenIdStrings.UnexpectedHttpStatusCode, (int)response.Status, response.Status);
			}

			return response;
		}
	}
}
