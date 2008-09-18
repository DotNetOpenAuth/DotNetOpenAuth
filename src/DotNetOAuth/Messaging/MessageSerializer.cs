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
	using System.Linq;
	using System.Runtime.Serialization;
	using System.Xml;
	using System.Xml.Linq;

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
		internal IDictionary<string, string> Serialize(IProtocolMessage message) {
			if (message == null) {
				throw new ArgumentNullException("message");
			}

			return new Reflection.MessageDictionary(message);
		}

		/// <summary>
		/// Reads name=value pairs into an OAuth message.
		/// </summary>
		/// <param name="fields">The name=value pairs that were read in from the transport.</param>
		/// <returns>The instantiated and initialized <see cref="IProtocolMessage"/> instance.</returns>
		internal IProtocolMessage Deserialize(IDictionary<string, string> fields) {
			if (fields == null) {
				throw new ArgumentNullException("fields");
			}

			IProtocolMessage result ;
			try {
				result = (IProtocolMessage)Activator.CreateInstance(this.messageType, true);
			} catch (MissingMethodException ex) {
				throw new ProtocolException("Failed to instantiate type " + this.messageType.FullName, ex);
			}
			foreach (var pair in fields) {
				IDictionary<string, string> dictionary = new Reflection.MessageDictionary(result);
				dictionary.Add(pair);
			}
			result.EnsureValidMessage();
			return result;
		}
	}
}
