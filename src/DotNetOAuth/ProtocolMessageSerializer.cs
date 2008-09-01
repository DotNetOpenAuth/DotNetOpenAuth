//-----------------------------------------------------------------------
// <copyright file="ProtocolMessageSerializer.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Serialization;
	using System.Xml;
	using System.Xml.Linq;

	/// <summary>
	/// Serializes/deserializes OAuth messages for/from transit.
	/// </summary>
	/// <typeparam name="T">The specific <see cref="IProtocolMessage"/>-derived type
	/// that will be serialized and deserialized using this class.</typeparam>
	internal class ProtocolMessageSerializer<T> where T : IProtocolMessage {
		/// <summary>
		/// Backing field for the <see cref="RootElement"/> property
		/// </summary>
		private XName rootElement;

		/// <summary>
		/// The serializer that will be used as a reflection engine to extract
		/// the OAuth message properties out of their containing <see cref="IProtocolMessage"/>
		/// objects.
		/// </summary>
		private DataContractSerializer serializer;

		/// <summary>
		/// Initializes a new instance of the ProtocolMessageSerializer class.
		/// </summary>
		internal ProtocolMessageSerializer() {
			this.serializer = new DataContractSerializer(
				typeof(T), this.RootElement.LocalName, this.RootElement.NamespaceName);
		}

		/// <summary>
		/// Gets the XML element that is used to surround all the XML values from the dictionary.
		/// </summary>
		private XName RootElement {
			get {
				if (this.rootElement == null) {
					DataContractAttribute attribute = typeof(T).GetCustomAttributes(typeof(DataContractAttribute), false).OfType<DataContractAttribute>().Single();
					this.rootElement = XName.Get("root", attribute.Namespace);
				}

				return this.rootElement;
			}
		}

		/// <summary>
		/// Reads the data from a message instance and returns a series of name=value pairs for the fields that must be included in the message.
		/// </summary>
		/// <param name="message">The message to be serialized.</param>
		/// <returns>The dictionary of values to send for the message.</returns>
		internal IDictionary<string, string> Serialize(T message) {
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
		internal void Serialize(IDictionary<string, string> fields, T message) {
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
		internal T Deserialize(IDictionary<string, string> fields) {
			if (fields == null) {
				throw new ArgumentNullException("fields");
			}

			var reader = DictionaryXmlReader.Create(this.RootElement, fields);
			T result;
			try {
				result = (T)this.serializer.ReadObject(reader, false);
			} catch (SerializationException ex) {
				// Missing required fields is one cause of this exception.
				throw new ProtocolException(Strings.InvalidIncomingMessage, ex);
			}
			result.EnsureValidMessage();
			return result;
		}
	}
}
