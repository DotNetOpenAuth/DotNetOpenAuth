//-----------------------------------------------------------------------
// <copyright file="MessageSerializer.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.Reflection;
	using DotNetOAuth.Messaging.Reflection;
	using DotNetOAuth.OAuth.ChannelElements;

	/// <summary>
	/// Serializes/deserializes OAuth messages for/from transit.
	/// </summary>
	internal class MessageSerializer {
		/// <summary>
		/// The specific <see cref="IProtocolMessage"/>-derived type
		/// that will be serialized and deserialized using this class.
		/// </summary>
		private readonly Type messageType;

		/// <summary>
		/// Initializes a new instance of the MessageSerializer class.
		/// </summary>
		/// <param name="messageType">The specific <see cref="IProtocolMessage"/>-derived type
		/// that will be serialized and deserialized using this class.</param>
		private MessageSerializer(Type messageType) {
			Debug.Assert(messageType != null, "messageType == null");

			if (!typeof(IProtocolMessage).IsAssignableFrom(messageType)) {
				throw new ArgumentException(
					string.Format(
						CultureInfo.CurrentCulture,
						MessagingStrings.UnexpectedType,
						typeof(IProtocolMessage).FullName,
						messageType.FullName),
						"messageType");
			}

			this.messageType = messageType;
		}

		/// <summary>
		/// Creates or reuses a message serializer for a given message type.
		/// </summary>
		/// <param name="messageType">The type of message that will be serialized/deserialized.</param>
		/// <returns>A message serializer for the given message type.</returns>
		internal static MessageSerializer Get(Type messageType) {
			if (messageType == null) {
				throw new ArgumentNullException("messageType");
			}

			return new MessageSerializer(messageType);
		}

		/// <summary>
		/// Reads the data from a message instance and returns a series of name=value pairs for the fields that must be included in the message.
		/// </summary>
		/// <param name="message">The message to be serialized.</param>
		/// <returns>The dictionary of values to send for the message.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Parallel design with Deserialize method.")]
		internal IDictionary<string, string> Serialize(IProtocolMessage message) {
			if (message == null) {
				throw new ArgumentNullException("message");
			}

			var result = new Reflection.MessageDictionary(message);

			return result;
		}

		/// <summary>
		/// Reads name=value pairs into an OAuth message.
		/// </summary>
		/// <param name="fields">The name=value pairs that were read in from the transport.</param>
		/// <param name="recipient">The recipient of the message.</param>
		/// <returns>The instantiated and initialized <see cref="IProtocolMessage"/> instance.</returns>
		internal IProtocolMessage Deserialize(IDictionary<string, string> fields, MessageReceivingEndpoint recipient) {
			if (fields == null) {
				throw new ArgumentNullException("fields");
			}

			// Before we deserialize the message, make sure all the required parts are present.
			MessageDescription.Get(this.messageType).EnsureRequiredMessagePartsArePresent(fields.Keys);

			IProtocolMessage result = this.CreateMessage(recipient);
			foreach (var pair in fields) {
				IDictionary<string, string> dictionary = new MessageDictionary(result);
				dictionary[pair.Key] = pair.Value;
			}
			result.EnsureValidMessage();
			return result;
		}

		/// <summary>
		/// Instantiates a new message to deserialize data into.
		/// </summary>
		/// <param name="recipient">The recipient this message is directed to, if any.</param>
		/// <returns>The newly created message object.</returns>
		private IProtocolMessage CreateMessage(MessageReceivingEndpoint recipient) {
			IProtocolMessage result;
			BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			if (typeof(IOAuthDirectedMessage).IsAssignableFrom(this.messageType)) {
				// Some OAuth messages take just the recipient, while others take the whole endpoint
				ConstructorInfo ctor;
				if ((ctor = this.messageType.GetConstructor(bindingFlags, null, new Type[] { typeof(Uri) }, null)) != null) {
					if (recipient == null) {
						// We need a recipient to deserialize directed messages.
						throw new ArgumentNullException("recipient");
					}

					result = (IProtocolMessage)ctor.Invoke(new object[] { recipient.Location });
				} else if ((ctor = this.messageType.GetConstructor(bindingFlags, null, new Type[] { typeof(MessageReceivingEndpoint) }, null)) != null) {
					if (recipient == null) {
						// We need a recipient to deserialize directed messages.
						throw new ArgumentNullException("recipient");
					}

					result = (IProtocolMessage)ctor.Invoke(new object[] { recipient });
				} else if ((ctor = this.messageType.GetConstructor(bindingFlags, null, new Type[0], null)) != null) {
					result = (IProtocolMessage)ctor.Invoke(new object[0]);
				} else {
					throw new InvalidOperationException("Unrecognized constructor signature on type " + this.messageType);
				}
			} else {
				result = (IProtocolMessage)Activator.CreateInstance(this.messageType, true);
			}

			return result;
		}
	}
}
