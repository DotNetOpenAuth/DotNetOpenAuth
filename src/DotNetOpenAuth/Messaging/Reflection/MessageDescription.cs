//-----------------------------------------------------------------------
// <copyright file="MessageDescription.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging.Reflection {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Globalization;
	using System.Linq;
	using System.Reflection;

	/// <summary>
	/// A mapping between serialized key names and <see cref="MessagePart"/> instances describing
	/// those key/values pairs.
	/// </summary>
	internal class MessageDescription {
		/// <summary>
		/// A dictionary of reflected message types and the generated reflection information.
		/// </summary>
		private static Dictionary<MessageTypeAndVersion, MessageDescription> reflectedMessageTypes = new Dictionary<MessageTypeAndVersion, MessageDescription>();

		/// <summary>
		/// The type of message this instance was generated from.
		/// </summary>
		private MessageTypeAndVersion messageTypeAndVersion;

		/// <summary>
		/// A mapping between the serialized key names and their 
		/// describing <see cref="MessagePart"/> instances.
		/// </summary>
		private Dictionary<string, MessagePart> mapping;

		/// <summary>
		/// Initializes a new instance of the <see cref="MessageDescription"/> class.
		/// </summary>
		/// <param name="messageTypeAndVersion">The type and protocol version of the message to reflect over.</param>
		private MessageDescription(MessageTypeAndVersion messageTypeAndVersion) {
			ErrorUtilities.VerifyArgumentNotNull(messageTypeAndVersion, "messageTypeAndVersion");

			if (!typeof(IMessage).IsAssignableFrom(messageTypeAndVersion.Type)) {
				throw new ArgumentException(string.Format(
					CultureInfo.CurrentCulture,
					MessagingStrings.UnexpectedType,
					typeof(IMessage),
					messageTypeAndVersion.Type));
			}

			this.messageTypeAndVersion = messageTypeAndVersion;
			this.ReflectMessageType();
		}

		/// <summary>
		/// Gets the mapping between the serialized key names and their describing
		/// <see cref="MessagePart"/> instances.
		/// </summary>
		internal IDictionary<string, MessagePart> Mapping {
			get { return this.mapping; }
		}

		/// <summary>
		/// Gets a <see cref="MessageDescription"/> instance prepared for the
		/// given message type.
		/// </summary>
		/// <param name="messageType">A type that implements <see cref="IMessage"/>.</param>
		/// <param name="messageVersion">The protocol version of the message.</param>
		/// <returns>A <see cref="MessageDescription"/> instance.</returns>
		internal static MessageDescription Get(Type messageType, Version messageVersion) {
			ErrorUtilities.VerifyArgumentNotNull(messageType, "messageType");
			ErrorUtilities.VerifyArgumentNotNull(messageVersion, "messageVersion");

			MessageTypeAndVersion key = new MessageTypeAndVersion(messageType, messageVersion);

			MessageDescription result;
			if (!reflectedMessageTypes.TryGetValue(key, out result)) {
				lock (reflectedMessageTypes) {
					if (!reflectedMessageTypes.TryGetValue(key, out result)) {
						reflectedMessageTypes[key] = result = new MessageDescription(key);
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Reflects over some <see cref="IMessage"/>-implementing type
		/// and prepares to serialize/deserialize instances of that type.
		/// </summary>
		internal void ReflectMessageType() {
			this.mapping = new Dictionary<string, MessagePart>();

			Type currentType = this.messageTypeAndVersion.Type;
			do {
				foreach (MemberInfo member in currentType.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)) {
					if (member is PropertyInfo || member is FieldInfo) {
						MessagePartAttribute partAttribute =
							(from a in member.GetCustomAttributes(typeof(MessagePartAttribute), true).OfType<MessagePartAttribute>()
							 orderby a.MinVersionValue descending
							 where a.MinVersionValue <= this.messageTypeAndVersion.Version
							 where a.MaxVersionValue >= this.messageTypeAndVersion.Version
							 select a).FirstOrDefault();
						if (partAttribute != null) {
							MessagePart part = new MessagePart(member, partAttribute);
							this.mapping.Add(part.Name, part);
						}
					}
				}
				currentType = currentType.BaseType;
			} while (currentType != null);
		}

		/// <summary>
		/// Ensures the message parts pass basic validation.
		/// </summary>
		/// <param name="parts">The key/value pairs of the serialzied message.</param>
		internal void EnsureMessagePartsPassBasicValidation(IDictionary<string, string> parts) {
			this.EnsureRequiredMessagePartsArePresent(parts.Keys);
			this.EnsureRequiredProtocolMessagePartsAreNotEmpty(parts);
		}

		/// <summary>
		/// Verifies that a given set of keys include all the required parameters
		/// for this message type or throws an exception.
		/// </summary>
		/// <param name="keys">The names of all parameters included in a message.</param>
		/// <exception cref="ProtocolException">Thrown when required parts of a message are not in <paramref name="keys"/></exception>
		private void EnsureRequiredMessagePartsArePresent(IEnumerable<string> keys) {
			var missingKeys = (from part in Mapping.Values
							   where part.IsRequired && !keys.Contains(part.Name)
							   select part.Name).ToArray();
			if (missingKeys.Length > 0) {
				throw new ProtocolException(
					string.Format(
						CultureInfo.CurrentCulture,
						MessagingStrings.RequiredParametersMissing,
						this.messageTypeAndVersion.Type.FullName,
						string.Join(", ", missingKeys)));
			}
		}

		/// <summary>
		/// Ensures the protocol message parts that must not be empty are in fact not empty.
		/// </summary>
		/// <param name="partValues">A dictionary of key/value pairs that make up the serialized message.</param>
		private void EnsureRequiredProtocolMessagePartsAreNotEmpty(IDictionary<string, string> partValues) {
			string value;
			var emptyValuedKeys = (from part in Mapping.Values
								   where !part.AllowEmpty && partValues.TryGetValue(part.Name, out value) && value != null && value.Length == 0
								   select part.Name).ToArray();
			if (emptyValuedKeys.Length > 0) {
				throw new ProtocolException(
					string.Format(
						CultureInfo.CurrentCulture,
						MessagingStrings.RequiredNonEmptyParameterWasEmpty,
						this.messageTypeAndVersion.Type.FullName,
						string.Join(", ", emptyValuedKeys)));
			}
		}

		/// <summary>
		/// A struct used as the key to bundle message type and version.
		/// </summary>
		private struct MessageTypeAndVersion {
			/// <summary>
			/// Backing store for the <see cref="Type"/> property.
			/// </summary>
			private readonly Type type;

			/// <summary>
			/// Backing store for the <see cref="Version"/> property.
			/// </summary>
			private readonly Version version;

			/// <summary>
			/// Initializes a new instance of the <see cref="MessageTypeAndVersion"/> struct.
			/// </summary>
			/// <param name="messageType">Type of the message.</param>
			/// <param name="messageVersion">The message version.</param>
			internal MessageTypeAndVersion(Type messageType, Version messageVersion) {
				ErrorUtilities.VerifyArgumentNotNull(messageType, "messageType");
				ErrorUtilities.VerifyArgumentNotNull(messageVersion, "messageVersion");

				this.type = messageType;
				this.version = messageVersion;
			}

			/// <summary>
			/// Gets the message type.
			/// </summary>
			internal Type Type { get { return this.type; } }

			/// <summary>
			/// Gets the message version.
			/// </summary>
			internal Version Version { get { return this.version; } }

			/// <summary>
			/// Implements the operator ==.
			/// </summary>
			/// <param name="first">The first object to compare.</param>
			/// <param name="second">The second object to compare.</param>
			/// <returns>The result of the operator.</returns>
			public static bool operator ==(MessageTypeAndVersion first, MessageTypeAndVersion second) {
				// structs cannot be null, so this is safe
				return first.Equals(second);
			}

			/// <summary>
			/// Implements the operator !=.
			/// </summary>
			/// <param name="first">The first object to compare.</param>
			/// <param name="second">The second object to compare.</param>
			/// <returns>The result of the operator.</returns>
			public static bool operator !=(MessageTypeAndVersion first, MessageTypeAndVersion second) {
				// structs cannot be null, so this is safe
				return !first.Equals(second);
			}

			/// <summary>
			/// Indicates whether this instance and a specified object are equal.
			/// </summary>
			/// <param name="obj">Another object to compare to.</param>
			/// <returns>
			/// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
			/// </returns>
			public override bool Equals(object obj) {
				if (obj is MessageTypeAndVersion) {
					MessageTypeAndVersion other = (MessageTypeAndVersion)obj;
					return this.type == other.type && this.version == other.version;
				} else {
					return false;
				}
			}

			/// <summary>
			/// Returns the hash code for this instance.
			/// </summary>
			/// <returns>
			/// A 32-bit signed integer that is the hash code for this instance.
			/// </returns>
			public override int GetHashCode() {
				return this.type.GetHashCode();
			}
		}
	}
}
