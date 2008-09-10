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
		/// The serializer that will be used as a reflection engine to extract
		/// the OAuth message properties out of their containing <see cref="IProtocolMessage"/>
		/// objects.
		/// </summary>
		private readonly DataContractSerializer serializer;

		/// <summary>
		/// The specific <see cref="IProtocolMessage"/>-derived type
		/// that will be serialized and deserialized using this class.
		/// </summary>
		private readonly Type messageType;

		/// <summary>
		/// An AppDomain-wide cache of shared serializers for optimization purposes.
		/// </summary>
		private static Dictionary<Type, MessageSerializer> prebuiltSerializers = new Dictionary<Type, MessageSerializer>();

		/// <summary>
		/// Backing field for the <see cref="RootElement"/> property
		/// </summary>
		private XName rootElement;

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
			this.serializer = new DataContractSerializer(
				messageType, this.RootElement.LocalName, this.RootElement.NamespaceName);
		}

		/// <summary>
		/// Gets the XML element that is used to surround all the XML values from the dictionary.
		/// </summary>
		private XName RootElement {
			get {
				if (this.rootElement == null) {
					DataContractAttribute attribute = this.messageType.GetCustomAttributes(typeof(DataContractAttribute), false).OfType<DataContractAttribute>().Single();
					this.rootElement = XName.Get("root", attribute.Namespace);
				}

				return this.rootElement;
			}
		}

		/// <summary>
		/// Returns a message serializer from a reusable collection of serializers.
		/// </summary>
		/// <param name="messageType">The type of message that will be serialized/deserialized.</param>
		/// <returns>A previously created serializer if one exists, or a newly created one.</returns>
		internal static MessageSerializer Get(Type messageType) {
			if (messageType == null) {
				throw new ArgumentNullException("messageType");
			}

			// We do this as efficiently as possible by first trying to fetch the
			// serializer out of the dictionary without taking a lock.
			MessageSerializer serializer;
			if (prebuiltSerializers.TryGetValue(messageType, out serializer)) {
				return serializer;
			}
			
			// Since it wasn't there, we'll be trying to write to the dictionary so
			// we take a lock and try reading again first, then creating the serializer
			// and storing it when we're sure it absolutely necessary.
			lock (prebuiltSerializers) {
				if (prebuiltSerializers.TryGetValue(messageType, out serializer)) {
					return serializer;
				}
				serializer = new MessageSerializer(messageType);
				prebuiltSerializers.Add(messageType, serializer);
			}
			return serializer;
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

			var fields = new Dictionary<string, string>(StringComparer.Ordinal);
			this.Serialize(fields, message);
			return fields;
		}

		/// <summary>
		/// Saves the [DataMember] properties of a message to an existing dictionary.
		/// </summary>
		/// <param name="fields">The dictionary to save values to.</param>
		/// <param name="message">The message to pull values from.</param>
		internal void Serialize(IDictionary<string, string> fields, IProtocolMessage message) {
			if (fields == null) {
				throw new ArgumentNullException("fields");
			}
			if (message == null) {
				throw new ArgumentNullException("message");
			}

			message.EnsureValidMessage();
			using (XmlWriter writer = DictionaryXmlWriter.Create(fields)) {
				this.serializer.WriteObjectContent(writer, message);
			}
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

			var reader = DictionaryXmlReader.Create(this.RootElement, fields);
			IProtocolMessage result;
			try {
				result = (IProtocolMessage)this.serializer.ReadObject(reader, false);
			} catch (SerializationException ex) {
				// Missing required fields is one cause of this exception.
				throw new ProtocolException(Strings.InvalidIncomingMessage, ex);
			}
			result.EnsureValidMessage();
			return result;
		}
	}
}
