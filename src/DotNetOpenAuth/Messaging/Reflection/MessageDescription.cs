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
		private static Dictionary<Type, MessageDescription> reflectedMessageTypes = new Dictionary<Type, MessageDescription>();

		/// <summary>
		/// The type of message this instance was generated from.
		/// </summary>
		private Type messageType;

		/// <summary>
		/// A mapping between the serialized key names and their 
		/// describing <see cref="MessagePart"/> instances.
		/// </summary>
		private Dictionary<string, MessagePart> mapping;

		/// <summary>
		/// Initializes a new instance of the <see cref="MessageDescription"/> class.
		/// </summary>
		/// <param name="messageType">The type of message to reflect over.</param>
		private MessageDescription(Type messageType) {
			Debug.Assert(messageType != null, "messageType == null");

			if (!typeof(IProtocolMessage).IsAssignableFrom(messageType)) {
				throw new ArgumentException(string.Format(
					CultureInfo.CurrentCulture,
					MessagingStrings.UnexpectedType,
					typeof(IProtocolMessage),
					messageType));
			}

			this.messageType = messageType;
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
		/// <param name="messageType">A type that implements <see cref="IProtocolMessage"/>.</param>
		/// <returns>A <see cref="MessageDescription"/> instance.</returns>
		internal static MessageDescription Get(Type messageType) {
			if (messageType == null) {
				throw new ArgumentNullException("messageType");
			}

			MessageDescription result;
			if (!reflectedMessageTypes.TryGetValue(messageType, out result)) {
				lock (reflectedMessageTypes) {
					if (!reflectedMessageTypes.TryGetValue(messageType, out result)) {
						reflectedMessageTypes[messageType] = result = new MessageDescription(messageType);
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Reflects over some <see cref="IProtocolMessage"/>-implementing type
		/// and prepares to serialize/deserialize instances of that type.
		/// </summary>
		internal void ReflectMessageType() {
			this.mapping = new Dictionary<string, MessagePart>();

			Type currentType = this.messageType;
			do {
				foreach (MemberInfo member in currentType.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)) {
					if (member is PropertyInfo || member is FieldInfo) {
						MessagePartAttribute partAttribute = member.GetCustomAttributes(typeof(MessagePartAttribute), true).OfType<MessagePartAttribute>().FirstOrDefault();
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
		/// Verifies that a given set of keys include all the required parameters
		/// for this message type or throws an exception.
		/// </summary>
		/// <param name="keys">The names of all parameters included in a message.</param>
		/// <exception cref="ProtocolException">Thrown when required parts of a message are not in <paramref name="keys"/></exception>
		internal void EnsureRequiredMessagePartsArePresent(IEnumerable<string> keys) {
			var missingKeys = (from part in Mapping.Values
							   where part.IsRequired && !keys.Contains(part.Name)
							   select part.Name).ToArray();
			if (missingKeys.Length > 0) {
				throw new ProtocolException(
					string.Format(
						CultureInfo.CurrentCulture,
						MessagingStrings.RequiredParametersMissing,
						this.messageType.FullName,
						string.Join(", ", missingKeys)));
			}
		}
	}
}
