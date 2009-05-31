//-----------------------------------------------------------------------
// <copyright file="OpenIdChannel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
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
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;

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
		private const string KeyValueFormContentType = "application/x-openid-kvf";

		/// <summary>
		/// The encoder that understands how to read and write Key-Value Form.
		/// </summary>
		private KeyValueFormEncoding keyValueForm = new KeyValueFormEncoding();

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdChannel"/> class
		/// for use by a Relying Party.
		/// </summary>
		/// <param name="associationStore">The association store to use.</param>
		/// <param name="nonceStore">The nonce store to use.</param>
		/// <param name="securitySettings">The security settings to apply.</param>
		internal OpenIdChannel(IAssociationStore<Uri> associationStore, INonceStore nonceStore, RelyingPartySecuritySettings securitySettings)
			: this(associationStore, nonceStore, new OpenIdMessageFactory(), securitySettings, false) {
			Contract.Requires(securitySettings != null);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdChannel"/> class
		/// for use by a Provider.
		/// </summary>
		/// <param name="associationStore">The association store to use.</param>
		/// <param name="nonceStore">The nonce store to use.</param>
		/// <param name="securitySettings">The security settings.</param>
		internal OpenIdChannel(IAssociationStore<AssociationRelyingPartyType> associationStore, INonceStore nonceStore, ProviderSecuritySettings securitySettings)
			: this(associationStore, nonceStore, new OpenIdMessageFactory(), securitySettings) {
			Contract.Requires(securitySettings != null);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdChannel"/> class
		/// for use by a Relying Party.
		/// </summary>
		/// <param name="associationStore">The association store to use.</param>
		/// <param name="nonceStore">The nonce store to use.</param>
		/// <param name="messageTypeProvider">An object that knows how to distinguish the various OpenID message types for deserialization purposes.</param>
		/// <param name="securitySettings">The security settings to apply.</param>
		/// <param name="nonVerifying">A value indicating whether the channel is set up with no functional security binding elements.</param>
		private OpenIdChannel(IAssociationStore<Uri> associationStore, INonceStore nonceStore, IMessageFactory messageTypeProvider, RelyingPartySecuritySettings securitySettings, bool nonVerifying) :
			this(messageTypeProvider, InitializeBindingElements(associationStore, nonceStore, securitySettings, nonVerifying)) {
			Contract.Requires(messageTypeProvider != null);
			Contract.Requires(securitySettings != null);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdChannel"/> class
		/// for use by a Provider.
		/// </summary>
		/// <param name="associationStore">The association store to use.</param>
		/// <param name="nonceStore">The nonce store to use.</param>
		/// <param name="messageTypeProvider">An object that knows how to distinguish the various OpenID message types for deserialization purposes.</param>
		/// <param name="securitySettings">The security settings.</param>
		private OpenIdChannel(IAssociationStore<AssociationRelyingPartyType> associationStore, INonceStore nonceStore, IMessageFactory messageTypeProvider, ProviderSecuritySettings securitySettings) :
			this(messageTypeProvider, InitializeBindingElements(associationStore, nonceStore, securitySettings, false)) {
			Contract.Requires(messageTypeProvider != null);
			Contract.Requires(securitySettings != null);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdChannel"/> class.
		/// </summary>
		/// <param name="messageTypeProvider">A class prepared to analyze incoming messages and indicate what concrete
		/// message types can deserialize from it.</param>
		/// <param name="bindingElements">The binding elements to use in sending and receiving messages.</param>
		private OpenIdChannel(IMessageFactory messageTypeProvider, IChannelBindingElement[] bindingElements)
			: base(messageTypeProvider, bindingElements) {
			Contract.Requires(messageTypeProvider != null);

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
		/// A value indicating whether the channel is set up
		/// with no functional security binding elements.
		/// </summary>
		/// <returns>A new <see cref="OpenIdChannel"/> instance that will not perform verification on incoming messages or apply any security to outgoing messages.</returns>
		/// <remarks>
		/// 	<para>A value of <c>true</c> allows the relying party to preview incoming
		/// messages without invalidating nonces or checking signatures.</para>
		/// 	<para>Setting this to <c>true</c> poses a great security risk and is only
		/// present to support the <see cref="OpenIdAjaxTextBox"/> which needs to preview
		/// messages, and will validate them later.</para>
		/// </remarks>
		internal static OpenIdChannel CreateNonVerifyingChannel() {
			Contract.Ensures(Contract.Result<OpenIdChannel>() != null);

			return new OpenIdChannel(null, null, new OpenIdMessageFactory(), new RelyingPartySecuritySettings(), true);
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
			if (response == null) {
				throw new ArgumentNullException("response");
			}

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
			ErrorUtilities.VerifyArgumentNotNull(response, "response");

			var messageAccessor = this.MessageDescriptions.GetAccessor(response);
			var fields = messageAccessor.Serialize();
			byte[] keyValueEncoding = KeyValueFormEncoding.GetBytes(fields);

			OutgoingWebResponse preparedResponse = new OutgoingWebResponse();
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

		/// <summary>
		/// Initializes the binding elements.
		/// </summary>
		/// <typeparam name="T">The distinguishing factor used by the association store.</typeparam>
		/// <param name="associationStore">The association store.</param>
		/// <param name="nonceStore">The nonce store to use.</param>
		/// <param name="securitySettings">The security settings to apply.  Must be an instance of either <see cref="RelyingPartySecuritySettings"/> or <see cref="ProviderSecuritySettings"/>.</param>
		/// <param name="nonVerifying">A value indicating whether the channel is set up with no functional security binding elements.</param>
		/// <returns>
		/// An array of binding elements which may be used to construct the channel.
		/// </returns>
		[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "Needed for code contracts.")]
		private static IChannelBindingElement[] InitializeBindingElements<T>(IAssociationStore<T> associationStore, INonceStore nonceStore, SecuritySettings securitySettings, bool nonVerifying) {
			Contract.Requires(securitySettings != null);
			Contract.Requires(!nonVerifying || securitySettings is RelyingPartySecuritySettings);
			ErrorUtilities.VerifyArgumentNotNull(securitySettings, "securitySettings");

			var rpSecuritySettings = securitySettings as RelyingPartySecuritySettings;
			var opSecuritySettings = securitySettings as ProviderSecuritySettings;
			ErrorUtilities.VerifyInternal(rpSecuritySettings != null || opSecuritySettings != null, "Expected an RP or OP security settings instance.");
			ErrorUtilities.VerifyInternal(!nonVerifying || rpSecuritySettings != null, "Non-verifying channels can only be constructed for relying parties.");
			bool isRelyingPartyRole = rpSecuritySettings != null;

			var rpAssociationStore = associationStore as IAssociationStore<Uri>;
			var opAssociationStore = associationStore as IAssociationStore<AssociationRelyingPartyType>;
			ErrorUtilities.VerifyInternal(isRelyingPartyRole || opAssociationStore != null, "Providers MUST have an association store.");

			SigningBindingElement signingElement;
			if (isRelyingPartyRole) {
				signingElement = nonVerifying ? null : new SigningBindingElement(rpAssociationStore);
			} else {
				signingElement = new SigningBindingElement(opAssociationStore, opSecuritySettings);
			}

			var extensionFactory = OpenIdExtensionFactoryAggregator.LoadFromConfiguration();

			List<IChannelBindingElement> elements = new List<IChannelBindingElement>(8);
			elements.Add(new ExtensionsBindingElement(extensionFactory, securitySettings));
			if (isRelyingPartyRole) {
				elements.Add(new RelyingPartySecurityOptions(rpSecuritySettings));
				elements.Add(new BackwardCompatibilityBindingElement());
				ReturnToNonceBindingElement requestNonceElement = null;

				if (associationStore != null) {
					if (nonceStore != null) {
						// There is no point in having a ReturnToNonceBindingElement without
						// a ReturnToSignatureBindingElement because the nonce could be
						// artificially changed without it.
						requestNonceElement = new ReturnToNonceBindingElement(nonceStore, rpSecuritySettings);
						elements.Add(requestNonceElement);
					}

					// It is important that the return_to signing element comes last
					// so that the nonce is included in the signature.
					elements.Add(new ReturnToSignatureBindingElement(rpAssociationStore, rpSecuritySettings));
				}

				ErrorUtilities.VerifyOperation(!rpSecuritySettings.RejectUnsolicitedAssertions || requestNonceElement != null, OpenIdStrings.UnsolicitedAssertionRejectionRequiresNonceStore);
			} else {
				// Providers must always have a nonce store.
				ErrorUtilities.VerifyArgumentNotNull(nonceStore, "nonceStore");
			}

			if (nonVerifying) {
				elements.Add(new SkipSecurityBindingElement());
			} else {
				if (nonceStore != null) {
					elements.Add(new StandardReplayProtectionBindingElement(nonceStore, true));
				}

				elements.Add(new StandardExpirationBindingElement());
				elements.Add(signingElement);
			}

			return elements.ToArray();
		}
	}
}
