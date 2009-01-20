//-----------------------------------------------------------------------
// <copyright file="OpenIdChannel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
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
		/// <param name="secretStore">The secret store to use.</param>
		/// <param name="securitySettings">The security settings to apply.</param>
		internal OpenIdChannel(IAssociationStore<Uri> associationStore, INonceStore nonceStore, IPrivateSecretStore secretStore, RelyingPartySecuritySettings securitySettings)
			: this(associationStore, nonceStore, secretStore, new OpenIdMessageFactory(), securitySettings) {
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
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdChannel"/> class
		/// for use by a Relying Party.
		/// </summary>
		/// <param name="associationStore">The association store to use.</param>
		/// <param name="nonceStore">The nonce store to use.</param>
		/// <param name="secretStore">The secret store to use.</param>
		/// <param name="messageTypeProvider">An object that knows how to distinguish the various OpenID message types for deserialization purposes.</param>
		/// <param name="securitySettings">The security settings to apply.</param>
		private OpenIdChannel(IAssociationStore<Uri> associationStore, INonceStore nonceStore, IPrivateSecretStore secretStore, IMessageFactory messageTypeProvider, RelyingPartySecuritySettings securitySettings) :
			this(messageTypeProvider, InitializeBindingElements(new SigningBindingElement(associationStore), nonceStore, secretStore, securitySettings)) {
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
			this(messageTypeProvider, InitializeBindingElements(new SigningBindingElement(associationStore, securitySettings), nonceStore, null, securitySettings)) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdChannel"/> class.
		/// </summary>
		/// <param name="messageTypeProvider">A class prepared to analyze incoming messages and indicate what concrete
		/// message types can deserialize from it.</param>
		/// <param name="bindingElements">The binding elements to use in sending and receiving messages.</param>
		private OpenIdChannel(IMessageFactory messageTypeProvider, IChannelBindingElement[] bindingElements)
			: base(messageTypeProvider, bindingElements) {
			// Customize the binding element order, since we play some tricks for higher
			// security and backward compatibility with older OpenID versions.
			var outgoingBindingElements = new List<IChannelBindingElement>(bindingElements);
			var incomingBindingElements = new List<IChannelBindingElement>(bindingElements);
			incomingBindingElements.Reverse();

			// Customize the order of the incoming elements by moving the return_to elements in front.
			var backwardCompatibility = incomingBindingElements.OfType<BackwardCompatibilityBindingElement>().SingleOrDefault();
			var returnToSign = incomingBindingElements.OfType<ReturnToSignatureBindingElement>().SingleOrDefault();
			if (backwardCompatibility != null && returnToSign != null) {
				incomingBindingElements.MoveTo(0, returnToSign);
				incomingBindingElements.MoveTo(1, backwardCompatibility);
			}

			CustomizeBindingElementOrder(outgoingBindingElements, incomingBindingElements);

			// Change out the standard web request handler to reflect the standard
			// OpenID pattern that outgoing web requests are to unknown and untrusted
			// servers on the Internet.
			this.WebRequestHandler = new UntrustedWebRequestHandler();
		}

		/// <summary>
		/// Gets the extension factory that can be used to register OpenID extensions.
		/// </summary>
		internal OpenIdExtensionFactory Extensions { get; private set; }

		/// <summary>
		/// Verifies the integrity and applicability of an incoming message.
		/// </summary>
		/// <param name="message">The message just received.</param>
		/// <exception cref="ProtocolException">
		/// Thrown when the message is somehow invalid, except for check_authentication messages.
		/// This can be due to tampering, replay attack or expiration, among other things.
		/// </exception>
		protected override void VerifyMessageAfterReceiving(IProtocolMessage message) {
			var checkAuthRequest = message as CheckAuthenticationRequest;
			if (checkAuthRequest != null) {
				IndirectSignedResponse originalResponse = new IndirectSignedResponse(checkAuthRequest);
				try {
					base.VerifyMessageAfterReceiving(originalResponse);
					checkAuthRequest.IsValid = true;
				} catch (ProtocolException) {
					checkAuthRequest.IsValid = false;
				}
			} else {
				base.VerifyMessageAfterReceiving(message);
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
		protected override IDictionary<string, string> ReadFromResponseInternal(DirectWebResponse response) {
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
		protected override UserAgentResponse SendDirectMessageResponse(IProtocolMessage response) {
			if (response == null) {
				throw new ArgumentNullException("response");
			}

			var serializer = MessageSerializer.Get(response.GetType());
			var fields = serializer.Serialize(response);
			byte[] keyValueEncoding = KeyValueFormEncoding.GetBytes(fields);

			UserAgentResponse preparedResponse = new UserAgentResponse();
			preparedResponse.Headers.Add(HttpResponseHeader.ContentType, KeyValueFormContentType);
			preparedResponse.OriginalMessage = response;
			preparedResponse.ResponseStream = new MemoryStream(keyValueEncoding);

			return preparedResponse;
		}

		/// <summary>
		/// Initializes the binding elements.
		/// </summary>
		/// <param name="signingElement">The signing element, previously constructed.</param>
		/// <param name="nonceStore">The nonce store to use.</param>
		/// <param name="secretStore">The secret store to use.</param>
		/// <param name="securitySettings">The security settings to apply.  Must be an instance of either <see cref="RelyingPartySecuritySettings"/> or <see cref="ProviderSecuritySettings"/>.</param>
		/// <returns>
		/// An array of binding elements which may be used to construct the channel.
		/// </returns>
		private static IChannelBindingElement[] InitializeBindingElements(SigningBindingElement signingElement, INonceStore nonceStore, IPrivateSecretStore secretStore, SecuritySettings securitySettings) {
			ErrorUtilities.VerifyArgumentNotNull(signingElement, "signingElement");
			ErrorUtilities.VerifyArgumentNotNull(securitySettings, "securitySettings");

			RelyingPartySecuritySettings rpSecuritySettings = securitySettings as RelyingPartySecuritySettings;
			ProviderSecuritySettings opSecuritySettings = securitySettings as ProviderSecuritySettings;
			ErrorUtilities.VerifyInternal(rpSecuritySettings != null || opSecuritySettings != null, "Expected an RP or OP security settings instance.");
			bool isRelyingPartyRole = rpSecuritySettings != null;

			List<IChannelBindingElement> elements = new List<IChannelBindingElement>(7);
			if (isRelyingPartyRole) {
				elements.Add(new ExtensionsBindingElement(new OpenIdExtensionFactory(), rpSecuritySettings));
				elements.Add(new BackwardCompatibilityBindingElement());

				if (secretStore != null) {
					secretStore.InitializeSecretIfUnset();

					if (nonceStore != null) {
						// There is no point in having a ReturnToNonceBindingElement without
						// a ReturnToSignatureBindingElement because the nonce could be
						// artificially changed without it.
						elements.Add(new ReturnToNonceBindingElement(nonceStore));
					}

					// It is important that the return_to signing element comes last
					// so that the nonce is included in the signature.
					elements.Add(new ReturnToSignatureBindingElement(secretStore));
				}
			} else {
				elements.Add(new ExtensionsBindingElement(new OpenIdExtensionFactory(), opSecuritySettings));

				// Providers must always have a nonce store.
				ErrorUtilities.VerifyArgumentNotNull(nonceStore, "nonceStore");
			}

			if (nonceStore != null) {
				elements.Add(new StandardReplayProtectionBindingElement(nonceStore, true));
			}

			elements.Add(new StandardExpirationBindingElement());
			elements.Add(signingElement);

			return elements.ToArray();
		}
	}
}
