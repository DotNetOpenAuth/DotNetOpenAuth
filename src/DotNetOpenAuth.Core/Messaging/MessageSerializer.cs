//-----------------------------------------------------------------------
// <copyright file="MessageSerializer.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.Reflection;
	using System.Xml;
	using DotNetOpenAuth.Messaging.Reflection;
	using Validation;

	/// <summary>
	/// Serializes/deserializes OAuth messages for/from transit.
	/// </summary>
	internal class MessageSerializer {
		/// <summary>
		/// The specific <see cref="IMessage"/>-derived type
		/// that will be serialized and deserialized using this class.
		/// </summary>
		private readonly Type messageType;

		/// <summary>
		/// Initializes a new instance of the MessageSerializer class.
		/// </summary>
		/// <param name="messageType">The specific <see cref="IMessage"/>-derived type
		/// that will be serialized and deserialized using this class.</param>
		private MessageSerializer(Type messageType) {
			RequiresEx.NotNullSubtype<IMessage>(messageType, "messageType");
			this.messageType = messageType;
		}

		/// <summary>
		/// Creates or reuses a message serializer for a given message type.
		/// </summary>
		/// <param name="messageType">The type of message that will be serialized/deserialized.</param>
		/// <returns>A message serializer for the given message type.</returns>
			internal static MessageSerializer Get(Type messageType) {
			RequiresEx.NotNullSubtype<IMessage>(messageType, "messageType");

			return new MessageSerializer(messageType);
		}

		/// <summary>
		/// Reads JSON as a flat dictionary into a message.
		/// </summary>
		/// <param name="messageDictionary">The message dictionary to fill with the JSON-deserialized data.</param>
		/// <param name="reader">The JSON reader.</param>
		internal static void DeserializeJsonAsFlatDictionary(IDictionary<string, string> messageDictionary, XmlDictionaryReader reader) {
			Requires.NotNull(messageDictionary, "messageDictionary");
			Requires.NotNull(reader, "reader");

			reader.Read(); // one extra one to skip the root node.
			while (reader.Read()) {
				if (reader.NodeType == XmlNodeType.EndElement) {
					// This is likely the closing </root> tag.
					continue;
				}

				string key = reader.Name;
				reader.Read();
				string value = reader.ReadContentAsString();
				messageDictionary[key] = value;
			}
		}

		/// <summary>
		/// Reads the data from a message instance and writes an XML/JSON encoding of it.
		/// </summary>
		/// <param name="messageDictionary">The message to be serialized.</param>
		/// <param name="writer">The writer to use for the serialized form.</param>
		/// <remarks>
		/// Use <see cref="System.Runtime.Serialization.Json.JsonReaderWriterFactory.CreateJsonWriter(System.IO.Stream)"/>
		/// to create the <see cref="XmlDictionaryWriter"/> instance capable of emitting JSON.
		/// </remarks>
		[Pure]
		internal static void Serialize(MessageDictionary messageDictionary, XmlDictionaryWriter writer) {
			Requires.NotNull(messageDictionary, "messageDictionary");
			Requires.NotNull(writer, "writer");

			writer.WriteStartElement("root");
			writer.WriteAttributeString("type", "object");
			foreach (var pair in messageDictionary) {
				bool include = false;
				string type = "string";
				MessagePart partDescription;
				if (messageDictionary.Description.Mapping.TryGetValue(pair.Key, out partDescription)) {
					Assumes.True(partDescription != null);
					if (partDescription.IsRequired || partDescription.IsNondefaultValueSet(messageDictionary.Message)) {
						include = true;
						Type formattingType = partDescription.PreferredFormattingType;
						if (IsNumeric(formattingType)) {
							type = "number";
						} else if (formattingType.IsAssignableFrom(typeof(bool))) {
							type = "boolean";
						}
					}
				} else {
					// This is extra data.  We always write it out.
					include = true;
				}

				if (include) {
					writer.WriteStartElement(pair.Key);
					writer.WriteAttributeString("type", type);
					writer.WriteString(pair.Value);
					writer.WriteEndElement();
				}
			}

			writer.WriteEndElement();
		}

		/// <summary>
		/// Reads XML/JSON into a message dictionary.
		/// </summary>
		/// <param name="messageDictionary">The message to deserialize into.</param>
		/// <param name="reader">The XML/JSON to read into the message.</param>
		/// <exception cref="ProtocolException">Thrown when protocol rules are broken by the incoming message.</exception>
		/// <remarks>
		/// Use <see cref="System.Runtime.Serialization.Json.JsonReaderWriterFactory.CreateJsonReader(System.IO.Stream, System.Xml.XmlDictionaryReaderQuotas)"/>
		/// to create the <see cref="XmlDictionaryReader"/> instance capable of reading JSON.
		/// </remarks>
		internal static void Deserialize(MessageDictionary messageDictionary, XmlDictionaryReader reader) {
			Requires.NotNull(messageDictionary, "messageDictionary");
			Requires.NotNull(reader, "reader");

			DeserializeJsonAsFlatDictionary(messageDictionary, reader);

			// Make sure all the required parts are present and valid.
			messageDictionary.Description.EnsureMessagePartsPassBasicValidation(messageDictionary);
			messageDictionary.Message.EnsureValidMessage();
		}

		/// <summary>
		/// Reads the data from a message instance and returns a series of name=value pairs for the fields that must be included in the message.
		/// </summary>
		/// <param name="messageDictionary">The message to be serialized.</param>
		/// <returns>The dictionary of values to send for the message.</returns>
		[Pure]
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Parallel design with Deserialize method.")]
		internal IDictionary<string, string> Serialize(MessageDictionary messageDictionary) {
			Requires.NotNull(messageDictionary, "messageDictionary");

			// Rather than hand back the whole message dictionary (which 
			// includes keys with blank values), create a new dictionary
			// that only has required keys, and optional keys whose
			// values are not empty (or default).
			var result = new Dictionary<string, string>();
			foreach (var pair in messageDictionary) {
				MessagePart partDescription;
				if (messageDictionary.Description.Mapping.TryGetValue(pair.Key, out partDescription)) {
					Assumes.True(partDescription != null);
					if (partDescription.IsRequired || partDescription.IsNondefaultValueSet(messageDictionary.Message)) {
						result.Add(pair.Key, pair.Value);
					}
				} else {
					// This is extra data.  We always write it out.
					result.Add(pair.Key, pair.Value);
				}
			}

			return result;
		}

		/// <summary>
		/// Reads name=value pairs into a message.
		/// </summary>
		/// <param name="fields">The name=value pairs that were read in from the transport.</param>
		/// <param name="messageDictionary">The message to deserialize into.</param>
		/// <exception cref="ProtocolException">Thrown when protocol rules are broken by the incoming message.</exception>
		internal void Deserialize(IDictionary<string, string> fields, MessageDictionary messageDictionary) {
			Requires.NotNull(fields, "fields");
			Requires.NotNull(messageDictionary, "messageDictionary");

			var messageDescription = messageDictionary.Description;

			// Before we deserialize the message, make sure all the required parts are present.
			messageDescription.EnsureMessagePartsPassBasicValidation(fields);

			try {
				foreach (var pair in fields) {
					messageDictionary[pair.Key] = pair.Value;
				}
			} catch (ArgumentException ex) {
				throw ErrorUtilities.Wrap(ex, MessagingStrings.ErrorDeserializingMessage, this.messageType.Name);
			}

			messageDictionary.Message.EnsureValidMessage();

			var originalPayloadMessage = messageDictionary.Message as IMessageOriginalPayload;
			if (originalPayloadMessage != null) {
				originalPayloadMessage.OriginalPayload = fields;
			}
		}

		/// <summary>
		/// Determines whether the specified type is numeric.
		/// </summary>
		/// <param name="type">The type to test.</param>
		/// <returns>
		/// 	<c>true</c> if the specified type is numeric; otherwise, <c>false</c>.
		/// </returns>
		private static bool IsNumeric(Type type) {
			return type.IsAssignableFrom(typeof(double))
				|| type.IsAssignableFrom(typeof(float))
				|| type.IsAssignableFrom(typeof(short))
				|| type.IsAssignableFrom(typeof(int))
				|| type.IsAssignableFrom(typeof(long))
				|| type.IsAssignableFrom(typeof(ushort))
				|| type.IsAssignableFrom(typeof(uint))
				|| type.IsAssignableFrom(typeof(ulong));
		}

#if CONTRACTS_FULL
		/// <summary>
		/// Verifies conditions that should be true for any valid state of this object.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Called by code contracts.")]
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called by code contracts.")]
		[ContractInvariantMethod]
		private void ObjectInvariant() {
			Contract.Invariant(this.messageType != null);
		}
#endif
	}
}
