//-----------------------------------------------------------------------
// <copyright file="ExtensionsBindingElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;

	/// <summary>
	/// The binding element that serializes/deserializes OpenID extensions to/from
	/// their carrying OpenID messages.
	/// </summary>
	internal class ExtensionsBindingElement : IChannelBindingElement {
		/// <summary>
		/// The security settings on the Relying Party that is hosting this binding element.
		/// </summary>
		private RelyingPartySecuritySettings rpSecuritySettings;

		/// <summary>
		/// The security settings on the Provider that is hosting this binding element.
		/// </summary>
		private ProviderSecuritySettings opSecuritySettings;

		/// <summary>
		/// Initializes a new instance of the <see cref="ExtensionsBindingElement"/> class.
		/// </summary>
		/// <param name="extensionFactory">The extension factory.</param>
		/// <param name="securitySettings">The security settings to apply.</param>
		internal ExtensionsBindingElement(IOpenIdExtensionFactory extensionFactory, RelyingPartySecuritySettings securitySettings) {
			ErrorUtilities.VerifyArgumentNotNull(extensionFactory, "extensionFactory");
			ErrorUtilities.VerifyArgumentNotNull(securitySettings, "securitySettings");

			this.ExtensionFactory = extensionFactory;
			this.rpSecuritySettings = securitySettings;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ExtensionsBindingElement"/> class.
		/// </summary>
		/// <param name="extensionFactory">The extension factory.</param>
		/// <param name="securitySettings">The security settings to apply.</param>
		internal ExtensionsBindingElement(IOpenIdExtensionFactory extensionFactory, ProviderSecuritySettings securitySettings) {
			ErrorUtilities.VerifyArgumentNotNull(extensionFactory, "extensionFactory");
			ErrorUtilities.VerifyArgumentNotNull(securitySettings, "securitySettings");

			this.ExtensionFactory = extensionFactory;
			this.opSecuritySettings = securitySettings;
		}

		#region IChannelBindingElement Members

		/// <summary>
		/// Gets or sets the channel that this binding element belongs to.
		/// </summary>
		/// <value></value>
		/// <remarks>
		/// This property is set by the channel when it is first constructed.
		/// </remarks>
		public Channel Channel { get; set; }

		/// <summary>
		/// Gets the extension factory.
		/// </summary>
		public IOpenIdExtensionFactory ExtensionFactory { get; private set; }

		/// <summary>
		/// Gets the protection offered (if any) by this binding element.
		/// </summary>
		/// <value><see cref="MessageProtections.None"/></value>
		public MessageProtections Protection {
			get { return MessageProtections.None; }
		}

		/// <summary>
		/// Prepares a message for sending based on the rules of this channel binding element.
		/// </summary>
		/// <param name="message">The message to prepare for sending.</param>
		/// <returns>
		/// True if the <paramref name="message"/> applied to this binding element
		/// and the operation was successful.  False otherwise.
		/// </returns>
		/// <remarks>
		/// Implementations that provide message protection must honor the
		/// <see cref="MessagePartAttribute.RequiredProtection"/> properties where applicable.
		/// </remarks>
		public bool PrepareMessageForSending(IProtocolMessage message) {
			ErrorUtilities.VerifyArgumentNotNull(message, "message");

			var extendableMessage = message as IProtocolMessageWithExtensions;
			if (extendableMessage != null) {
				Protocol protocol = Protocol.Lookup(message.Version);
				MessageDictionary baseMessageDictionary = new MessageDictionary(message);

				// We have a helper class that will do all the heavy-lifting of organizing
				// all the extensions, their aliases, and their parameters.
				var extensionManager = ExtensionArgumentsManager.CreateOutgoingExtensions(protocol);
				foreach (IExtensionMessage protocolExtension in extendableMessage.Extensions) {
					var extension = protocolExtension as IOpenIdMessageExtension;
					if (extension != null) {
						// Give extensions that require custom serialization a chance to do their work.
						var customSerializingExtension = extension as IMessageWithEvents;
						if (customSerializingExtension != null) {
							customSerializingExtension.OnSending();
						}

						// OpenID 2.0 Section 12 forbids two extensions with the same TypeURI in the same message.
						ErrorUtilities.VerifyProtocol(!extensionManager.ContainsExtension(extension.TypeUri), OpenIdStrings.ExtensionAlreadyAddedWithSameTypeURI, extension.TypeUri);

						var extensionDictionary = MessageSerializer.Get(extension.GetType()).Serialize(extension);
						extensionManager.AddExtensionArguments(extension.TypeUri, extensionDictionary);
					} else {
						Logger.WarnFormat("Unexpected extension type {0} did not implement {1}.", protocolExtension.GetType(), typeof(IOpenIdMessageExtension).Name);
					}
				}

				// We use a cheap trick (for now at least) to determine whether the 'openid.' prefix
				// belongs on the parameters by just looking at what other parameters do.
				// Technically, direct message responses from Provider to Relying Party are the only
				// messages that leave off the 'openid.' prefix.
				bool includeOpenIdPrefix = baseMessageDictionary.Keys.Any(key => key.StartsWith(protocol.openid.Prefix, StringComparison.Ordinal));

				// Add the extension parameters to the base message for transmission.
				extendableMessage.AddExtraParameters(extensionManager.GetArgumentsToSend(includeOpenIdPrefix));
				return true;
			}

			return false;
		}

		/// <summary>
		/// Performs any transformation on an incoming message that may be necessary and/or
		/// validates an incoming message based on the rules of this channel binding element.
		/// </summary>
		/// <param name="message">The incoming message to process.</param>
		/// <returns>
		/// True if the <paramref name="message"/> applied to this binding element
		/// and the operation was successful.  False if the operation did not apply to this message.
		/// </returns>
		/// <exception cref="ProtocolException">
		/// Thrown when the binding element rules indicate that this message is invalid and should
		/// NOT be processed.
		/// </exception>
		/// <remarks>
		/// Implementations that provide message protection must honor the
		/// <see cref="MessagePartAttribute.RequiredProtection"/> properties where applicable.
		/// </remarks>
		public bool PrepareMessageForReceiving(IProtocolMessage message) {
			var extendableMessage = message as IProtocolMessageWithExtensions;
			if (extendableMessage != null) {
				// We have a helper class that will do all the heavy-lifting of organizing
				// all the extensions, their aliases, and their parameters.
				var extensionManager = ExtensionArgumentsManager.CreateIncomingExtensions(this.GetExtensionsDictionary(message));
				foreach (string typeUri in extensionManager.GetExtensionTypeUris()) {
					var extensionData = extensionManager.GetExtensionArguments(typeUri);

					// Initialize this particular extension.
					IOpenIdMessageExtension extension = this.ExtensionFactory.Create(typeUri, extensionData, extendableMessage);
					if (extension != null) {
						MessageDictionary extensionDictionary = new MessageDictionary(extension);
						foreach (var pair in extensionData) {
							extensionDictionary[pair.Key] = pair.Value;
						}

						// Give extensions that require custom serialization a chance to do their work.
						var customSerializingExtension = extension as IMessageWithEvents;
						if (customSerializingExtension != null) {
							customSerializingExtension.OnReceiving();
						}

						extendableMessage.Extensions.Add(extension);
					} else {
						Logger.WarnFormat("Extension with type URI '{0}' ignored because it is not a recognized extension.", typeUri);
					}
				}

				return true;
			}

			return false;
		}

		#endregion

		/// <summary>
		/// Gets the dictionary of message parts that should be deserialized into extensions.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <returns>A dictionary of message parts, including only signed parts when appropriate.</returns>
		private IDictionary<string, string> GetExtensionsDictionary(IProtocolMessage message) {
			// An IndirectSignedResponse message (the only one we care to filter parts for)
			// can be received both by RPs and OPs (during check_auth).  
			// Whichever party is reading the extensions, apply their security policy regarding
			// signing.  (Although OPs have no reason to deserialize extensions during check_auth)
			// so that scenario might be optimized away eventually.
			bool extensionsShouldBeSigned = this.rpSecuritySettings != null ? !this.rpSecuritySettings.AllowUnsignedIncomingExtensions : this.opSecuritySettings.SignOutgoingExtensions;

			IndirectSignedResponse signedResponse = message as IndirectSignedResponse;
			if (signedResponse != null && extensionsShouldBeSigned) {
				return signedResponse.GetSignedMessageParts();
			} else {
				return new MessageDictionary(message);
			}
		}
	}
}
