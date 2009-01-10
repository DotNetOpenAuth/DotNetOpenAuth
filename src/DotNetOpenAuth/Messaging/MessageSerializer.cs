//-----------------------------------------------------------------------
// <copyright file="MessageSerializer.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.Reflection;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OAuth.ChannelElements;

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
		/// <param name="message">The message to deserialize into.</param>
		/// <exception cref="ProtocolException">Thrown when protocol rules are broken by the incoming message.</exception>
		internal void Deserialize(IDictionary<string, string> fields, IProtocolMessage message) {
			ErrorUtilities.VerifyArgumentNotNull(fields, "fields");
			ErrorUtilities.VerifyArgumentNotNull(message, "message");

			// Before we deserialize the message, make sure all the required parts are present.
			MessageDescription.Get(this.messageType, message.Version).EnsureMessagePartsPassBasicValidation(fields);

			try {
				foreach (var pair in fields) {
					IDictionary<string, string> dictionary = new MessageDictionary(message);
					dictionary[pair.Key] = pair.Value;
				}
			} catch (ArgumentException ex) {
				throw ErrorUtilities.Wrap(ex, MessagingStrings.ErrorDeserializingMessage, this.messageType.Name);
			}
			message.EnsureValidMessage();
		}
	}
}
