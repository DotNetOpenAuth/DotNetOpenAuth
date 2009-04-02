//-----------------------------------------------------------------------
// <copyright file="MessageDescription.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging.Reflection {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.Linq;
	using System.Reflection;

	/// <summary>
	/// A mapping between serialized key names and <see cref="MessagePart"/> instances describing
	/// those key/values pairs.
	/// </summary>
	internal class MessageDescription {
		/// <summary>
		/// The type of message this instance was generated from.
		/// </summary>
		private Type messageType;

		/// <summary>
		/// The message version this instance was generated from.
		/// </summary>
		private Version messageVersion;

		/// <summary>
		/// A mapping between the serialized key names and their 
		/// describing <see cref="MessagePart"/> instances.
		/// </summary>
		private Dictionary<string, MessagePart> mapping;

		/// <summary>
		/// Initializes a new instance of the <see cref="MessageDescription"/> class.
		/// </summary>
		/// <param name="messageType">Type of the message.</param>
		/// <param name="messageVersion">The message version.</param>
		internal MessageDescription(Type messageType, Version messageVersion) {
			Contract.Requires(messageType != null && typeof(IMessage).IsAssignableFrom(messageType));
			Contract.Requires(messageVersion != null);
			ErrorUtilities.VerifyArgumentNotNull(messageType, "messageType");
			ErrorUtilities.VerifyArgumentNotNull(messageVersion, "messageVersion");
			if (!typeof(IMessage).IsAssignableFrom(messageType)) {
				throw new ArgumentException(string.Format(
					CultureInfo.CurrentCulture,
					MessagingStrings.UnexpectedType,
					typeof(IMessage),
					messageType));
			}

			this.messageType = messageType;
			this.messageVersion = messageVersion;
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
		/// Gets a dictionary that provides read/write access to a message.
		/// </summary>
		/// <param name="message">The message the dictionary should provide access to.</param>
		/// <returns>The dictionary accessor to the message</returns>
		[Pure]
		internal MessageDictionary GetDictionary(IMessage message) {
			Contract.Requires(message != null);
			Contract.Ensures(Contract.Result<MessageDictionary>() != null);
			ErrorUtilities.VerifyArgumentNotNull(message, "message");
			return new MessageDictionary(message, this);
		}

		/// <summary>
		/// Reflects over some <see cref="IMessage"/>-implementing type
		/// and prepares to serialize/deserialize instances of that type.
		/// </summary>
		internal void ReflectMessageType() {
			this.mapping = new Dictionary<string, MessagePart>();

			Type currentType = this.messageType;
			do {
				foreach (MemberInfo member in currentType.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)) {
					if (member is PropertyInfo || member is FieldInfo) {
						MessagePartAttribute partAttribute =
							(from a in member.GetCustomAttributes(typeof(MessagePartAttribute), true).OfType<MessagePartAttribute>()
							 orderby a.MinVersionValue descending
							 where a.MinVersionValue <= this.messageVersion
							 where a.MaxVersionValue >= this.messageVersion
							 select a).FirstOrDefault();
						if (partAttribute != null) {
							MessagePart part = new MessagePart(member, partAttribute);
							if (this.mapping.ContainsKey(part.Name)) {
								Logger.Messaging.WarnFormat(
									"Message type {0} has more than one message part named {1}.  Inherited members will be hidden.",
									this.messageType.Name,
									part.Name);
							} else {
								this.mapping.Add(part.Name, part);
							}
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
						this.messageType.FullName,
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
						this.messageType.FullName,
						string.Join(", ", emptyValuedKeys)));
			}
		}
	}
}
