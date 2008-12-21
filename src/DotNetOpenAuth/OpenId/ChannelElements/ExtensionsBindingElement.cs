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

	/// <summary>
	/// The binding element that serializes/deserializes OpenID extensions to/from
	/// their carrying OpenID messages.
	/// </summary>
	internal class ExtensionsBindingElement : IChannelBindingElement {
		/// <summary>
		/// The extension factory.
		/// </summary>
		private readonly IOpenIdExtensionFactory extensionFactory;

		/// <summary>
		/// Initializes a new instance of the <see cref="ExtensionsBindingElement"/> class.
		/// </summary>
		/// <param name="extensionFactory">The extension factory.</param>
		internal ExtensionsBindingElement(IOpenIdExtensionFactory extensionFactory) {
			ErrorUtilities.VerifyArgumentNotNull(extensionFactory, "extensionFactory");
			this.extensionFactory = extensionFactory;
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
						var extensionDictionary = new MessageDictionary(extension);
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
				Protocol protocol = Protocol.Lookup(message.Version);
				MessageDictionary baseMessageDictionary = new MessageDictionary(message);

				// We have a helper class that will do all the heavy-lifting of organizing
				// all the extensions, their aliases, and their parameters.
				var extensionManager = ExtensionArgumentsManager.CreateIncomingExtensions(baseMessageDictionary);
				foreach (string typeUri in extensionManager.GetExtensionTypeUris()) {
					var extensionData = extensionManager.GetExtensionArguments(typeUri);

					// Initialize this particular extension.
					IOpenIdMessageExtension extension = this.extensionFactory.Create(typeUri, extensionData, extendableMessage);
					MessageDictionary extensionDictionary = new MessageDictionary(extension);
					foreach (var pair in extensionData) {
						extensionDictionary[pair.Key] = pair.Value;
					}

					extendableMessage.Extensions.Add(extension);
				}

				return true;
			}

			return false;
		}

		#endregion
	}
}
