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

	internal class MessageDescription {
		private Type messageType;
		private Dictionary<string, MessagePart> mapping;

		internal MessageDescription(Type messageType) {
			if (messageType == null) {
				throw new ArgumentNullException("messageType");
			}

			if (!typeof(IProtocolMessage).IsAssignableFrom(messageType)) {
				throw new ArgumentOutOfRangeException(); // TODO: better message
			}

			this.messageType = messageType;
			this.ReflectMessageType();
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
	}
}
