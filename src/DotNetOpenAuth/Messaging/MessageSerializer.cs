//-----------------------------------------------------------------------
// <copyright file="MessageSerializer.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
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
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OAuth.ChannelElements;

	/// <summary>
	/// Serializes/deserializes OAuth messages for/from transit.
	/// </summary>
	[ContractVerification(true)]
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
		[ContractVerification(false)] // bugs/limitations in CC static analysis
		private MessageSerializer(Type messageType) {
			Contract.Requires(messageType != null);
			Contract.Requires(typeof(IMessage).IsAssignableFrom(messageType));
			Contract.Ensures(this.messageType != null);

			ErrorUtilities.VerifyArgumentNamed(
				typeof(IMessage).IsAssignableFrom(messageType),
				"messageType",
				MessagingStrings.UnexpectedType,
				typeof(IMessage).FullName,
				messageType.FullName);

			this.messageType = messageType;
		}

		/// <summary>
		/// Creates or reuses a message serializer for a given message type.
		/// </summary>
		/// <param name="messageType">The type of message that will be serialized/deserialized.</param>
		/// <returns>A message serializer for the given message type.</returns>
		[ContractVerification(false)] // bugs/limitations in CC static analysis
		internal static MessageSerializer Get(Type messageType) {
			Contract.Requires(messageType != null);
			Contract.Requires(typeof(IMessage).IsAssignableFrom(messageType));
			ErrorUtilities.VerifyArgumentNotNull(messageType, "messageType");

			return new MessageSerializer(messageType);
		}

		/// <summary>
		/// Reads the data from a message instance and returns a series of name=value pairs for the fields that must be included in the message.
		/// </summary>
		/// <param name="messageDictionary">The message to be serialized.</param>
		/// <returns>The dictionary of values to send for the message.</returns>
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Parallel design with Deserialize method.")]
		internal IDictionary<string, string> Serialize(MessageDictionary messageDictionary) {
			Contract.Requires(messageDictionary != null);
			Contract.Ensures(Contract.Result<IDictionary<string, string>>() != null);
			ErrorUtilities.VerifyArgumentNotNull(messageDictionary, "messageDictionary");

			// Rather than hand back the whole message dictionary (which 
			// includes keys with blank values), create a new dictionary
			// that only has required keys, and optional keys whose
			// values are not empty.
			var result = new Dictionary<string, string>();
			foreach (var pair in messageDictionary) {
				MessagePart partDescription;
				if (messageDictionary.Description.Mapping.TryGetValue(pair.Key, out partDescription)) {
					Contract.Assume(partDescription != null);
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
			Contract.Requires(fields != null);
			Contract.Requires(messageDictionary != null);
			ErrorUtilities.VerifyArgumentNotNull(fields, "fields");
			ErrorUtilities.VerifyArgumentNotNull(messageDictionary, "messageDictionary");

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
		}

#if CONTRACTS_FULL
		/// <summary>
		/// Verifies conditions that should be true for any valid state of this object.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called by code contracts.")]
		[ContractInvariantMethod]
		private void ObjectInvariant() {
			Contract.Invariant(this.messageType != null);
		}
#endif
	}
}
