//-----------------------------------------------------------------------
// <copyright file="MessageDescription.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messaging.Reflection {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Globalization;

	internal class MessageDescription {
		private static Dictionary<Type, MessageDescription> reflectedMessageTypes = new Dictionary<Type,MessageDescription>();
		private Type messageType;
		private Dictionary<string, MessagePart> mapping;

		private MessageDescription(Type messageType) {
			if (messageType == null) {
				throw new ArgumentNullException("messageType");
			}

			if (!typeof(IProtocolMessage).IsAssignableFrom(messageType)) {
				throw new ArgumentOutOfRangeException(); // TODO: better message
			}

			this.messageType = messageType;
			this.ReflectMessageType();
		}

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

		internal Type MessageType {
			get { return this.messageType; }
		}

		internal IDictionary<string, MessagePart> Mapping {
			get { return this.mapping; }
		}

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
		/// <exception cref="ProtocolException">Thrown when required parts of a message are not in <paramref name="keys"/></exception>
		internal void EnsureRequiredMessagePartsArePresent(IEnumerable<string> keys) {
			var missingKeys = (from part in Mapping.Values
							   where part.IsRequired && !keys.Contains(part.Name)
							   select part.Name).ToArray();
			if (missingKeys.Length > 0) {
				throw new ProtocolException(string.Format(CultureInfo.CurrentCulture,
					MessagingStrings.RequiredParametersMissing,
					this.messageType.FullName,
					string.Join(", ", missingKeys)));
			}
		}
	}
}
